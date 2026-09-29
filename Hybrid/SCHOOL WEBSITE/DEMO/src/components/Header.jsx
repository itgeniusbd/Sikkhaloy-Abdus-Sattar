import { useState } from 'react'
import { NavLink, Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Header({ dark }) {
  const [open, setOpen] = useState(false)
  const { authed } = useApp()
  const { t, lang, setLang } = useLang()
  const links = [
    { to: '/', label: t('nav.home') },
    { to: '/project', label: t('nav.project') },
    { to: '/blog', label: t('nav.blog') },
    { to: '/about', label: t('nav.about') },
    { to: '/contact', label: t('nav.contact') },
  ]

  return (
    <header className={`site-header ${dark ? 'is-dark' : ''}`}>
      <div className="container header-inner">
        <Link to="/" className="brand" onClick={() => setOpen(false)}>
          <img src="/logo.svg" alt="SAFA PROPERTIS" width="52" height="52" />
          <span>
            <strong>SAFA</strong>
            <small>PROPERTIS</small>
          </span>
        </Link>

        <nav className={open ? 'is-open' : ''}>
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} end={link.to === '/'} onClick={() => setOpen(false)}>
              {link.label}
            </NavLink>
          ))}
          <NavLink to={authed ? '/admin' : '/login'} onClick={() => setOpen(false)}>
            {authed ? t('nav.admin') : t('nav.login')}
          </NavLink>
        </nav>

        <div className="header-tools">
          <div className="lang-switch" role="group" aria-label="Language">
            <button type="button" className={lang === 'bn' ? 'is-active' : ''} onClick={() => setLang('bn')}>
              BN
            </button>
            <button type="button" className={lang === 'en' ? 'is-active' : ''} onClick={() => setLang('en')}>
              EN
            </button>
          </div>
          <button className="nav-toggle" aria-label={t('nav.menu')} onClick={() => setOpen((v) => !v)}>
            <span />
            <span />
            <span />
          </button>
        </div>
      </div>
    </header>
  )
}
