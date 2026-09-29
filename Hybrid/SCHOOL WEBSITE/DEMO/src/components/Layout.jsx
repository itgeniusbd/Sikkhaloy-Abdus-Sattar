import { Outlet, useLocation } from 'react-router-dom'
import { useEffect, useState } from 'react'
import Header from './Header.jsx'
import Footer from './Footer.jsx'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Layout() {
  const { pathname } = useLocation()
  const { dbReady, dbError, loading } = useApp()
  const { t } = useLang()
  const dark = pathname.startsWith('/project/') || pathname.startsWith('/blog/')
  const [showTop, setShowTop] = useState(false)

  useEffect(() => {
    const onScroll = () => setShowTop(window.scrollY > 400)
    onScroll()
    window.addEventListener('scroll', onScroll)
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  return (
    <>
      <Header dark={dark} />
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
