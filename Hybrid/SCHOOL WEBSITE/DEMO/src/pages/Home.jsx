import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LOCATIONS, PROPERTY_TYPES, SERVICES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import HeroSlider from '../components/HeroSlider.jsx'
import ProjectSlider from '../components/ProjectSlider.jsx'
import { IconArrow, IconCheck, IconMail, IconPhone, IconPin, IconUser, SERVICE_ICONS } from '../components/Icons.jsx'

const STATUSES = ['completed', 'ongoing', 'upcoming']

export default function Home() {
  const { projects, sendMessage, pages, site } = useApp()
  const { t, label, serviceText, pageText } = useLang()
  const { about, contact } = pageText({ ...pages, contact: { ...pages.contact, ...site } })
  const navigate = useNavigate()
  const [status, setStatus] = useState('completed')
  const [city, setCity] = useState('')
  const [type, setType] = useState('')
  const [q, setQ] = useState('')
  const [quote, setQuote] = useState({ name: '', phone: '', email: '', address: '', message: '' })
  const [sent, setSent] = useState(false)
  const [quoteError, setQuoteError] = useState('')
  const [quoteBusy, setQuoteBusy] = useState(false)

  const filtered = useMemo(() => {
    return projects.filter((p) => {
      if (p.status !== status) return false
      if (city && p.city !== city) return false
      if (type && p.type !== type) return false
      if (q && !`${p.title} ${p.location}`.toLowerCase().includes(q.toLowerCase())) return false
      return true
    })
  }, [projects, status, city, type, q])

  function search(e) {
    e.preventDefault()
    const params = new URLSearchParams({ status })
    if (city) params.set('city', city)
    if (type) params.set('type', type)
    if (q) params.set('q', q)
    navigate(`/project?${params.toString()}`)
  }

  async function sendQuote(e) {
    e.preventDefault()
    setQuoteBusy(true)
    setQuoteError('')
    setSent(false)
    try {
      await sendMessage(quote)
      setSent(true)
      setQuote({ name: '', phone: '', email: '', address: '', message: '' })
    } catch (err) {
      setQuoteError(err.message || t('common.messageFailed'))
    } finally {
      setQuoteBusy(false)
    }
  }

  return (
    <>
      <section className="hero">
        <HeroSlider />
        <form className="search-bar container" onSubmit={search}>
          <label>
            {t('common.location')}
            <select value={city} onChange={(e) => setCity(e.target.value)}>
              <option value="">{t('common.allLocations')}</option>
              {LOCATIONS.map((loc) => (
                <option key={loc} value={loc}>
                  {label('loc', loc)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('common.propertyType')}
            <select value={type} onChange={(e) => setType(e.target.value)}>
              <option value="">{t('common.allTypes')}</option>
              {PROPERTY_TYPES.map((item) => (
                <option key={item} value={item}>
                  {label('type', item)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('common.keyword')}
            <input value={q} onChange={(e) => setQ(e.target.value)} placeholder={t('common.keywordPh')} />
          </label>
          <button className="btn-orange search-btn" type="submit">
            {t('common.search')}
          </button>
        </form>
      </section>

      <section className="section">
        <div className="container">
          <div className="status-tabs">
            {STATUSES.map((item) => (
              <button key={item} className={status === item ? 'is-active' : ''} onClick={() => setStatus(item)}>
                {t(`status.${item}`)}
              </button>
            ))}
          </div>
          <ProjectSlider projects={filtered} resetKey={`${status}-${city}-${type}-${q}`} />
        </div>
      </section>

      <section className="section">
        <div className="container">
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
                    <Link to="/about" className="more-link">
                      {t('common.moreAbout')} <IconArrow />
                    </Link>
                  </div>
                </article>
              )
            })}
          </div>
        </div>
      </section>

      <section className="section about-block">
        <div className="container about-grid">
          <div className="about-photo">
            <span className="tri" />
            <img src={about.image} alt={about.title} />
          </div>
          <div>
            <p className="eyebrow">{about.eyebrow}</p>
            <h2 className="section-title">{about.title}</h2>
            <p className="lead">{about.lead}</p>
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
            <Link to="/about" className="btn-outline">
              {t('common.learnMore')} <IconArrow />
            </Link>
          </div>
        </div>
      </section>

      <section className="section quote-block">
        <div className="container quote-grid">
          <div>
            <p className="eyebrow">{contact.eyebrow}</p>
            <h2 className="section-title">{contact.title}</h2>
            <p className="lead">{contact.lead}</p>
            <div className="help-box">
              <IconPhone />
              <div>
                <strong>{t('common.needHelp')}</strong>
                <a href={`tel:${contact.phone.replace(/\s/g, '')}`}>{contact.phone}</a>
                <p>{contact.address}</p>
              </div>
            </div>
          </div>

          <form className="quote-form" onSubmit={sendQuote}>
            <label>
              {t('common.yourName')}
              <span>
                <input required value={quote.name} onChange={(e) => setQuote({ ...quote, name: e.target.value })} placeholder={t('common.namePh')} />
                <IconUser />
              </span>
            </label>
            <label>
              {t('common.yourPhone')}
              <span>
                <input required value={quote.phone} onChange={(e) => setQuote({ ...quote, phone: e.target.value })} placeholder={t('common.phonePh')} />
                <IconPhone />
              </span>
            </label>
            <label>
              {t('common.yourEmail')}
              <span>
                <input type="email" required value={quote.email} onChange={(e) => setQuote({ ...quote, email: e.target.value })} placeholder={t('common.emailPh')} />
                <IconMail />
              </span>
            </label>
            <label>
              {t('common.yourAddress')}
              <span>
                <input value={quote.address} onChange={(e) => setQuote({ ...quote, address: e.target.value })} placeholder={t('common.addressPh')} />
                <IconPin />
              </span>
            </label>
            <label className="full">
              {t('common.yourMessage')}
              <span>
                <textarea required rows="4" value={quote.message} onChange={(e) => setQuote({ ...quote, message: e.target.value })} placeholder={t('common.messagePh')} />
                <IconMail />
              </span>
            </label>
            {quoteError && <p className="form-error">{quoteError}</p>}
            <button className="btn-orange full" type="submit" disabled={quoteBusy}>
              {quoteBusy ? t('common.sending') : sent ? t('common.messageSent') : t('common.sendMessage')}
            </button>
          </form>
        </div>
      </section>
    </>
  )
}
