import { Link, Navigate, useParams } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

function formatNoticeDate(value, lang) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleDateString(lang === 'bn' ? 'bn-BD' : 'en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

export default function EduNotice() {
  const { id } = useParams()
  const { notices } = useApp()
  const { t, lang } = useLang()
  const items = Array.isArray(notices) ? notices.filter((item) => item?.title) : []
  const selected = items.find((item) => String(item.id) === String(id)) || items[0]

  if (!items.length) {
    return (
      <section className="page-pad">
        <div className="container">
          <p className="eyebrow">{t('pages.noticesEyebrow')}</p>
          <h1 className="page-title">{t('pages.noticesTitle')}</h1>
          <p className="lead">{t('pages.noNotices')}</p>
        </div>
      </section>
    )
  }

  if (id && String(selected.id) !== String(id)) {
    return <Navigate to={`/notices/${selected.id}`} replace />
  }

  if (!id) {
    return <Navigate to={`/notices/${selected.id}`} replace />
  }

  return (
    <section className="page-pad detail-page">
      <div className="container detail-grid">
        <div>
          <p className="eyebrow">{t('pages.noticesEyebrow')}</p>
          <h1 className="page-title">{selected.title}</h1>
          {selected.date ? <p className="notice-date">{formatNoticeDate(selected.date, lang)}</p> : null}
          {selected.body ? <div className="notice-body">{selected.body}</div> : <p className="lead">{t('pages.noNoticeBody')}</p>}
        </div>
        <aside className="sidebar">
          <div className="side-box">
            <h3>{t('pages.recentNotices')}</h3>
            <ul className="recent-list notice-side-list">
              {items.map((item) => (
                <li key={item.id}>
                  <Link className={String(item.id) === String(selected.id) ? 'is-active' : ''} to={`/notices/${item.id}`}>
                    <div>
                      {item.date ? <small>{formatNoticeDate(item.date, lang)}</small> : null}
                      <strong>{item.title}</strong>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        </aside>
      </div>
    </section>
  )
}
