import { Link } from 'react-router-dom'
import { SERVICES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { IconArrow, IconCheck, SERVICE_ICONS } from '../components/Icons.jsx'

export default function About() {
  const { pages } = useApp()
  const { t, pageText, serviceText } = useLang()
  const { about } = pageText(pages)

  return (
    <section className="page-pad">
      <div className="container about-grid">
        <div className="about-photo">
          <span className="tri" />
          <img src={about.image} alt={about.title} />
        </div>
        <div>
          <p className="eyebrow">{about.eyebrow}</p>
          <h1 className="page-title">{about.title}</h1>
          <p className="lead">{about.lead}</p>
          <p>{about.body}</p>
          <div className="feature-grid">
            {about.features.map((f) => (
              <div key={f} className="check-item">
                <span className="check">
                  <IconCheck />
                </span>
                {f}
              </div>
            ))}
          </div>
          <Link to="/contact" className="btn-outline">
            {t('common.learnMore')} <IconArrow />
          </Link>
        </div>
      </div>

      <div className="container" style={{ marginTop: 72 }}>
        <p className="eyebrow">{t('pages.serviceEyebrow')}</p>
        <h2 className="section-title">{t('pages.serviceTitle')}</h2>
        <div className="service-grid">
          {SERVICES.map((svc) => {
            const Icon = SERVICE_ICONS[svc.icon]
            const item = serviceText(svc)
            return (
              <article key={svc.id} className="service-card">
                <Icon className="svc-icon" />
                <div>
                  <h3>{item.title}</h3>
                  <p>{item.desc}</p>
                </div>
              </article>
            )
          })}
        </div>
      </div>
    </section>
  )
}
