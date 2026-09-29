import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { api } from '../lib/api.js'
import { SOCIAL_NETWORKS, socialHref, socialIcon, socialsFromSettings } from '../lib/socials.js'
import { IconMail } from './Icons.jsx'
import { useState } from 'react'

export default function Footer() {
  const { projects, footerGallery, site, settings } = useApp()
  const { t, feature, pageText } = useLang()
  const { about } = pageText({ about: { lead: site.about } })
  const [email, setEmail] = useState('')
  const [done, setDone] = useState(false)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const gallery = (footerGallery?.length
    ? footerGallery
    : projects.slice(0, 6).map((p) => ({ image: p.image, title: p.title, link: `/project/${p.slug}` }))
  ).filter((item) => item.image)

  async function subscribe(e) {
    e.preventDefault()
    if (!email.trim() || busy) return
    setBusy(true)
    setError('')
    try {
      await api.subscribe(email.trim())
      setDone(true)
      setEmail('')
    } catch (err) {
      setError(err.message || t('common.subscribeError'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <footer className="site-footer">
      <section className="newsletter" style={{ backgroundImage: `url(${gallery[0]?.image || ''})` }}>
        <div className="newsletter-overlay">
          <h2>{t('common.newsletterTitle')}</h2>
          <p>{t('common.newsletterText')}</p>
          <form onSubmit={subscribe}>
            <label>
              <IconMail />
              <input
                type="email"
                placeholder={t('common.yourEmail')}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </label>
            <button type="submit" className="btn-orange" disabled={busy}>
              {busy ? t('common.subscribeWait') : done ? t('common.subscribed') : t('common.subscribe')}
            </button>
          </form>
          {error ? <p className="form-error">{error}</p> : null}
        </div>
      </section>

      <div className="footer-main">
        <div className="container footer-grid">
          <div>
            <Link to="/" className="brand light">
              <img src="/logo.svg" alt="" width="52" height="52" />
              <span>
                <strong>SAFA</strong>
                <small>PROPERTIS</small>
              </span>
            </Link>
            <p className="footer-about">{about.lead || site.about}</p>
            <div className="socials">
              {SOCIAL_NETWORKS.map((item) => {
                const href = socialHref(item.id, socialsFromSettings(settings)[item.id])
                if (!href) return null
                const Icon = socialIcon(item.id)
                return (
                  <a key={item.id} href={href} target="_blank" rel="noreferrer" aria-label={t(item.labelKey)}>
                    <Icon />
                  </a>
                )
              })}
            </div>
          </div>

          <div>
            <h4>{t('common.usefulLinks')}</h4>
            <ul>
              <li><Link to="/blog">{t('nav.blog')}</Link></li>
              <li><Link to="/project">{t('common.property')}</Link></li>
              <li><Link to="/about">{t('nav.about')}</Link></li>
              <li><Link to="/contact">{t('nav.contact')}</Link></li>
              <li><Link to="/login">{t('nav.login')}</Link></li>
            </ul>
          </div>

          <div>
            <h4>{t('common.services')}</h4>
            <ul>
              {['Reliable Rentals', 'Golden Key Properties', 'Swift Home Sales', 'Elite Realty Services', 'Dream Property Solutions'].map((name) => (
                <li key={name}>
                  <Link to="/about">{feature(name)}</Link>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h4>{t('common.gallery')}</h4>
            <div className="footer-gallery">
              {gallery.map((item, i) => (
                <Link key={`${item.image}-${i}`} to={item.link || '/project'}>
                  <img src={item.image} alt={item.title || t('common.gallery')} />
                </Link>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="footer-copy">
        <div className="container">© {new Date().getFullYear()} SAFA PROPERTIS. {t('common.rights')}</div>
      </div>
    </footer>
  )
}
