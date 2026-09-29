import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function NewsTicker() {
  const { notices } = useApp()
  const { t } = useLang()
  const items = Array.isArray(notices) ? notices.filter((item) => item?.title) : []
  if (!items.length) return null

  const loop = items.length === 1 ? items : [...items, ...items]

  return (
    <div className="news-ticker-wrap">
      <div className="container news-ticker" role="marquee" aria-label={t('home.latestNews')}>
        <span className="news-ticker-label">{t('home.latestNews')}</span>
        <div className="news-ticker-window">
          <div className="news-ticker-track">
            {loop.map((item, index) => (
              <Link key={`${item.id || item.title}-${index}`} className="news-ticker-item" to={`/notices/${item.id}`}>
                {item.title}
              </Link>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
