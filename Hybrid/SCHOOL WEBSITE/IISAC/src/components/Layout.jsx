import { Outlet, useLocation } from 'react-router-dom'
import { useEffect, useState } from 'react'
import Header from './Header.jsx'
import Footer from './Footer.jsx'
import NewsTicker from './NewsTicker.jsx'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Layout() {
  const { pathname } = useLocation()
  const { dbReady, dbError, loading, settings } = useApp()
  const { t } = useLang()
  const dark = String(settings.theme_header_mode || 'light').toLowerCase() === 'dark'
  const [showTop, setShowTop] = useState(false)

  useEffect(() => {
    const onScroll = () => setShowTop(window.scrollY > 400)
    onScroll()
    window.addEventListener('scroll', onScroll)
    return () => window.removeEventListener('scroll', onScroll)
  }, [pathname])

  return (
    <>
      <div className="site-top">
        <Header dark={dark} />
        <NewsTicker />
      </div>
      {!loading && !dbReady && (
        <div className="db-banner">{dbError ? `${t('common.dbBanner')} (${dbError})` : t('common.dbBanner')}</div>
      )}
      <main>
        <Outlet />
      </main>
      <Footer />
      <button
        className={`to-top ${showTop ? 'is-visible' : ''}`}
        aria-label={t('common.backToTop')}
        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
      >
        ↑
      </button>
    </>
  )
}
