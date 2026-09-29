import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { menuLabel, menuPath, visibleMenu } from '../data/menu.js'
import { api } from '../lib/api.js'
import { SOCIAL_NETWORKS, socialHref, socialIcon, socialsFromSettings } from '../lib/socials.js'
import { IconMail } from './Icons.jsx'
import { useState } from 'react'

/** Replace leftover template school names in stored CMS text. */
function withSchoolName(text, schoolName) {
  const raw = String(text || '').trim()
  if (!raw) return ''
  const name = String(schoolName || 'Demo').trim() || 'Demo'
  return raw
    .replace(/\bIISAC\b/gi, name)
    .replace(/\bDemo\b/g, name)
}

export default function Footer() {
  const { footerGallery, site, settings, institute, menus } = useApp()
  const { t, pageText, lang } = useLang()
  const footerLinks = visibleMenu(menus).filter((item) => item.type !== 'home')
  const name = institute.schoolName || settings.site_name || site.name || 'Demo'
  const { about } = pageText({ about: { lead: withSchoolName(site.about, name) } })
  const [email, setEmail] = useState('')
  const [done, setDone] = useState(false)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  // Short tagline only — never fall back to the long about paragraph.
  const dialog = withSchoolName(settings.site_dialog || institute.dialog || '', name)
  const aboutText = withSchoolName(
    institute.dialog && institute.dialog !== dialog
      ? institute.dialog
      : about.lead || site.about || t('pages.aboutLead'),
    name,
  )
  const gallery = (footerGallery || []).filter((item) => item.image).slice(0, 6)

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
              <img src={institute.logoUrl || '/logo.svg'} alt="" width="52" height="52" />
              <span className="brand-text">
                <strong className="brand-name">{name}</strong>
                {dialog ? <small className="brand-dialog">{dialog}</small> : null}
              </span>
            </Link>
            {aboutText ? <p className="footer-about">{aboutText}</p> : null}
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
              {footerLinks.map((item) => (
                <li key={item.id}>
                  <Link to={menuPath(item)}>{menuLabel(item, lang, name)}</Link>
                </li>
              ))}
              <li><Link to="/login">{t('nav.login')}</Link></li>
            </ul>
          </div>

          <div>
            <h4>{t('common.contact')}</h4>
            <ul>
              {site.phone ? <li><a href={`tel:${String(site.phone).replace(/\s/g, '')}`}>{site.phone}</a></li> : null}
              {site.email ? <li><a href={`mailto:${site.email}`}>{site.email}</a></li> : null}
              {site.address ? <li>{site.address}</li> : null}
            </ul>
          </div>

          <div>
            <h4>{t('common.gallery')}</h4>
            <div className="footer-gallery">
              {gallery.map((item, i) => (
                <Link key={`${item.image}-${i}`} to={item.link || '/gallery'}>
                  <img src={item.image} alt={item.title || t('common.gallery')} />
                </Link>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="footer-copy">
        <div className="container">© {new Date().getFullYear()} {name}. {t('common.rights')}</div>
      </div>
    </footer>
  )
}
