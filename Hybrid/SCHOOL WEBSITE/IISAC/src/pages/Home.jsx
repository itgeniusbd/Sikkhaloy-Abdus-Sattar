import { useState } from 'react'
import { Link } from 'react-router-dom'
import { SERVICES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import HeroSlider from '../components/HeroSlider.jsx'
import PeopleSlider from '../components/PeopleSlider.jsx'
import BirthdayWishes from '../components/BirthdayWishes.jsx'
import { IconArrow, IconCheck, IconMail, IconPhone, IconPin, IconUser, SERVICE_ICONS } from '../components/Icons.jsx'

export default function Home() {
  const { sendMessage, pages, site, eduStats, teachers, birthdays } = useApp()
  const { t, serviceText, pageText } = useLang()
  const { about, contact } = pageText({ ...pages, contact: { ...pages.contact, ...site } })
  const [quote, setQuote] = useState({ name: '', phone: '', email: '', address: '', message: '' })
  const [sent, setSent] = useState(false)
  const [quoteError, setQuoteError] = useState('')
  const [quoteBusy, setQuoteBusy] = useState(false)

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
      </section>

      <section className="section stats-block">
        <div className="container">
          <p className="eyebrow">{t('home.statsEyebrow')}</p>
          <h2 className="section-title">{t('home.statsTitle')}</h2>
          <div className="stat-cards">
            <article className="stat-card">
              <strong>{Number(eduStats.students || 0).toLocaleString()}</strong>
              <span>{t('home.students')}</span>
            </article>
            <article className="stat-card">
              <strong>{Number(eduStats.teachers || 0).toLocaleString()}</strong>
              <span>{t('home.teachers')}</span>
            </article>
            <article className="stat-card">
              <strong>{Number(eduStats.staff || 0).toLocaleString()}</strong>
              <span>{t('home.staff')}</span>
            </article>
            <article className="stat-card">
              <strong>{Number(eduStats.classes?.length || 0).toLocaleString()}</strong>
              <span>{t('home.classes')}</span>
            </article>
          </div>
        </div>
      </section>

      <section className="section people-block">
        <div className="container">
          <p className="eyebrow">{t('home.teachersEyebrow')}</p>
          <h2 className="section-title">{t('home.teachersTitle')}</h2>
          <PeopleSlider people={teachers} resetKey="teachers" emptyLabel={t('home.noTeachers')} />
        </div>
      </section>

      {birthdays.some((person) => person?.name) ? (
        <BirthdayWishes
          people={birthdays}
          eyebrow={t('home.birthdayEyebrow')}
          title={t('home.birthdayTitle')}
          rollLabel={t('home.roll')}
        />
      ) : null}

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
                    <Link to="/p/glance" className="more-link">
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
            <Link to="/p/glance" className="btn-outline">
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
                <a href={`tel:${String(contact.phone || '').replace(/\s/g, '')}`}>{contact.phone}</a>
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
