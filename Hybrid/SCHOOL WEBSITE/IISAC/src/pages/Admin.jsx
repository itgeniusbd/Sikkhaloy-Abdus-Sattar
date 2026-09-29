import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import AdminSidebar from '../components/AdminSidebar.jsx'
import { IconHouse, IconLogout, IconMenu } from '../components/Icons.jsx'
import AdminBlog from './AdminBlog.jsx'
import AdminMessages from './AdminMessages.jsx'
import AdminSubscribers from './AdminSubscribers.jsx'
import AdminSocial from './AdminSocial.jsx'
import AdminPages from './AdminPages.jsx'
import AdminMenus from './AdminMenus.jsx'
import AdminProfile from './AdminProfile.jsx'
import AdminGallery from './AdminGallery.jsx'
import AdminAdmissions from './AdminAdmissions.jsx'
import AdminUsers from './AdminUsers.jsx'
import AdminCareer from './AdminCareer.jsx'
import AdminCareerApps from './AdminCareerApps.jsx'
import { firstAllowedTab } from '../lib/adminAccess.js'

export default function Admin() {
  const { logout, heroSlides, saveHeroSlides, canAccess, currentUser } = useApp()
  const { t, lang, setLang } = useLang()
  const navigate = useNavigate()
  const [tab, setTab] = useState('profile')
  const [fileKey, setFileKey] = useState(0)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [slides, setSlides] = useState(heroSlides)
  const [heroFiles, setHeroFiles] = useState({})
  const [heroSaved, setHeroSaved] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)

  useEffect(() => {
    setSlides(heroSlides)
  }, [heroSlides])

  useEffect(() => {
    if (!currentUser) return
    setTab((prev) => (canAccess(prev) ? prev : firstAllowedTab(currentUser)))
  }, [canAccess, currentUser])

  useEffect(() => {
    if (currentUser) setTab(firstAllowedTab(currentUser))
  }, [currentUser?.id])

  async function saveHero(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setHeroSaved(false)
    try {
      await saveHeroSlides(slides, heroFiles)
      setHeroFiles({})
      setFileKey((k) => k + 1)
      setHeroSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save slider.')
    } finally {
      setBusy(false)
    }
  }

  const title =
    tab === 'menus'
      ? t('admin.menus')
    : tab === 'slider'
      ? t('admin.homeSlider')
      : tab === 'footer'
        ? t('admin.gallery')
        : tab === 'blog'
          ? t('admin.blogPosts')
          : tab === 'about'
            ? t('admin.aboutPage')
            : tab === 'messages'
              ? t('admin.messages')
              : tab === 'subscribers'
                ? t('admin.subscribers')
              : tab === 'socials'
                ? t('admin.socials')
              : tab === 'admissions'
                ? t('admin.admissions')
              : tab === 'contact'
                ? t('admin.contactPage')
                : tab === 'users'
                  ? t('admin.users')
                : tab === 'career'
                  ? t('admin.career')
                : tab === 'careerApps'
                  ? t('admin.careerApps')
                : t('admin.profile')

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <div className="admin-shell">
      {menuOpen && <button type="button" className="admin-backdrop" aria-label="Close" onClick={() => setMenuOpen(false)} />}
      <AdminSidebar
        tab={tab}
        onTab={setTab}
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
      />
      <div className="admin-main">
        <header className="admin-topbar">
          <button type="button" className="admin-menu-btn" onClick={() => setMenuOpen(true)} aria-label={t('nav.menu')}>
            <IconMenu />
          </button>
          <div className="admin-topbar-title">
            <p className="eyebrow">{t('admin.dashboard')}</p>
            <h1>{title}</h1>
            {tab === 'menus' ? <p>{t('admin.menusLead')}</p> : null}
          </div>
          <div className="admin-topbar-tools">
            <div className="lang-switch" role="group" aria-label="Language">
              <button type="button" className={lang === 'bn' ? 'is-active' : ''} onClick={() => setLang('bn')}>
                BN
              </button>
              <button type="button" className={lang === 'en' ? 'is-active' : ''} onClick={() => setLang('en')}>
                EN
              </button>
            </div>
            <Link to="/" className="admin-view-site">
              <IconHouse />
              {t('admin.viewSite')}
            </Link>
            <button type="button" className="admin-logout" onClick={handleLogout}>
              <IconLogout />
              {t('admin.logout')}
            </button>
          </div>
        </header>

        {error && (
          <div className="admin-page">
            <p className="form-error">{error}</p>
          </div>
        )}

      {!currentUser ? null : !canAccess(tab) ? (
        <p className="container lead">{t('admin.noAccess')}</p>
      ) : tab === 'menus' ? (
        <AdminMenus />
      ) : tab === 'slider' ? (
        <form className="container hero-admin" onSubmit={saveHero}>
          <p className="lead">
            Each slide has its own photo and text. Upload a new image to replace the current one, then click Save slider.
          </p>
          {slides.map((slide, i) => {
            const preview = heroFiles[i] ? URL.createObjectURL(heroFiles[i]) : slide.image
            return (
              <article key={slide.id || i} className="hero-admin-card quote-form">
                <div className="hero-admin-head">
                  <h3>Slide {i + 1}</h3>
                  {slides.length > 1 && (
                    <button
                      type="button"
                      className="btn-del"
                      onClick={() => {
                        setSlides(slides.filter((_, idx) => idx !== i))
                        setHeroFiles((prev) => {
                          const next = {}
                          Object.entries(prev).forEach(([key, file]) => {
                            const n = Number(key)
                            if (n < i) next[n] = file
                            if (n > i) next[n - 1] = file
                          })
                          return next
                        })
                      }}
                    >
                      Remove slide
                    </button>
                  )}
                </div>
                {preview ? <img src={preview} alt="" className="admin-preview" /> : <div className="admin-preview empty-preview">No image yet</div>}
                <label className="full">
                  Replace photo
                  <input
                    key={`hero-${fileKey}-${i}`}
                    type="file"
                    accept="image/*"
                    onChange={(e) => setHeroFiles({ ...heroFiles, [i]: e.target.files?.[0] || null })}
                  />
                </label>
                <label>
                  Small label
                  <span>
                    <input
                      value={slide.eyebrow}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, eyebrow: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
                <label className="full">
                  Heading
                  <span>
                    <input
                      required
                      value={slide.title}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, title: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
                <label className="full">
                  Text on this photo
                  <span>
                    <textarea
                      required
                      rows="3"
                      value={slide.text}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, text: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
              </article>
            )
          })}
          {slides.length < 8 && (
            <button
              type="button"
              className="btn-outline"
              onClick={() =>
                setSlides([
                  ...slides,
                  {
                    id: `slide-${Date.now()}`,
                    image: '',
                    eyebrow: 'SAFA PROPERTIS',
                    title: '',
                    text: '',
                  },
                ])
              }
            >
              Add another slide
            </button>
          )}
          <button className="btn-orange" type="submit" disabled={busy}>
            {busy ? 'SAVING...' : heroSaved ? 'SLIDER SAVED' : 'SAVE SLIDER'}
          </button>
        </form>
      ) : tab === 'footer' ? (
        <AdminGallery />
      ) : tab === 'blog' ? (
        <AdminBlog />
      ) : tab === 'about' ? (
        <AdminPages section="about" />
      ) : tab === 'messages' ? (
        <AdminMessages />
      ) : tab === 'admissions' ? (
        <AdminAdmissions />
      ) : tab === 'subscribers' ? (
        <AdminSubscribers />
      ) : tab === 'socials' ? (
        <AdminSocial />
      ) : tab === 'contact' ? (
        <AdminPages section="contact" />
      ) : tab === 'users' ? (
        canAccess('users') ? <AdminUsers /> : <p className="container lead">{t('admin.noAccess')}</p>
      ) : tab === 'career' ? (
        <AdminCareer />
      ) : tab === 'careerApps' ? (
        <AdminCareerApps />
      ) : (
        <AdminProfile />
      )}
      </div>
    </div>
  )
}
