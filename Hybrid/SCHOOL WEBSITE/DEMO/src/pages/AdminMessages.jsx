import { useEffect, useState } from 'react'
import { api } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

function formatTime(value) {
  if (!value) return ''
  const date = new Date(String(value).replace(' ', 'T'))
  if (Number.isNaN(date.getTime())) return String(value)
  return date.toLocaleString()
}

export default function AdminMessages() {
  const { t } = useLang()
  const [items, setItems] = useState([])
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function load() {
    setBusy(true)
    setError('')
    try {
      const data = await api.messages()
      setItems(Array.isArray(data.messages) ? data.messages : [])
    } catch (err) {
      setError(err.message || t('admin.messagesLoadError'))
    } finally {
      setBusy(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function remove(id) {
    if (!window.confirm(t('admin.deleteMessageConfirm'))) return
    try {
      await api.deleteMessage(id)
      setItems((prev) => prev.filter((item) => item.id !== id))
    } catch (err) {
      setError(err.message || t('admin.messagesLoadError'))
    }
  }

  return (
    <div className="container messages-admin">
      {error && <p className="form-error">{error}</p>}
      {busy && items.length === 0 ? <p className="lead">{t('admin.messagesLoading')}</p> : null}
      {!busy && items.length === 0 ? <p className="lead">{t('admin.noMessages')}</p> : null}
      <div className="messages-list">
        {items.map((item) => (
          <article key={item.id} className="message-card">
            <div className="message-card-head">
              <div>
                <h3>{item.name}</h3>
                <small>{formatTime(item.created_at)}</small>
              </div>
              <button type="button" className="btn-del" onClick={() => remove(item.id)}>
                {t('admin.deleteMessage')}
              </button>
            </div>
            <p>
              {item.phone ? (
                <a href={`tel:${String(item.phone).replace(/\s/g, '')}`}>{item.phone}</a>
              ) : null}
              {item.phone && item.email ? ' · ' : null}
              {item.email ? <a href={`mailto:${item.email}`}>{item.email}</a> : null}
            </p>
            {item.address ? <p>{item.address}</p> : null}
            <p className="message-body">{item.message}</p>
          </article>
        ))}
      </div>
    </div>
  )
}
