import { useEffect, useState } from 'react'
import { api } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

function formatTime(value) {
  if (!value) return ''
  const date = new Date(String(value).replace(' ', 'T'))
  if (Number.isNaN(date.getTime())) return String(value)
  return date.toLocaleString()
}

export default function AdminSubscribers() {
  const { t } = useLang()
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function load() {
    setBusy(true)
    setError('')
    try {
      const data = await api.subscribers()
      setItems(Array.isArray(data.subscribers) ? data.subscribers : [])
    } catch (err) {
      setError(err.message || t('admin.subscribersLoadError'))
    } finally {
      setBusy(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  function exportExcel() {
    if (!items.length) {
      setError(t('admin.exportExcelEmpty'))
      return
    }
    const xmlEscape = (value) =>
      String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
    const rows = [
      ['Email', 'Date'],
      ...items.map((item) => [item.email, formatTime(item.created_at)]),
    ]
      .map(
        (cells) =>
          `<Row>${cells.map((cell) => `<Cell><Data ss:Type="String">${xmlEscape(cell)}</Data></Cell>`).join('')}</Row>`,
      )
      .join('')
    const xml = `<?xml version="1.0"?>
<?mso-application progid="Excel.Sheet"?>
<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
  <Worksheet ss:Name="Newsletter"><Table>${rows}</Table></Worksheet>
</Workbook>`
    const blob = new Blob([xml], { type: 'application/vnd.ms-excel' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    const day = new Date().toISOString().slice(0, 10)
    link.href = url
    link.download = `safa-newsletter-${day}.xls`
    link.click()
    URL.revokeObjectURL(url)
  }

  async function remove(id) {
    if (!window.confirm(t('admin.deleteSubscriberConfirm'))) return
    try {
      await api.deleteSubscriber(id)
      setItems((prev) => prev.filter((item) => item.id !== id))
    } catch (err) {
      setError(err.message || t('admin.subscribersLoadError'))
    }
  }

  return (
    <div className="container messages-admin">
      <div className="admin-head">
        <p className="lead">{t('admin.subscribersLead')}</p>
        <button type="button" className="btn-orange" onClick={exportExcel} disabled={!items.length}>
          {t('admin.exportExcel')}
        </button>
      </div>
      {error && <p className="form-error">{error}</p>}
      {busy && items.length === 0 ? <p className="lead">{t('admin.subscribersLoading')}</p> : null}
      {!busy && items.length === 0 ? <p className="lead">{t('admin.noSubscribers')}</p> : null}
      <div className="messages-list">
        {items.map((item) => (
          <article key={item.id} className="message-card">
            <div className="message-card-head">
              <div>
                <h3>
                  <a href={`mailto:${item.email}`}>{item.email}</a>
                </h3>
                <small>{formatTime(item.created_at)}</small>
              </div>
              <button type="button" className="btn-del" onClick={() => remove(item.id)}>
                {t('admin.deleteSubscriber')}
              </button>
            </div>
          </article>
        ))}
      </div>
    </div>
  )
}
