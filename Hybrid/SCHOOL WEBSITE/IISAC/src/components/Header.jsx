import { useRef, useState } from 'react'
import { NavLink, Link, useLocation } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { menuLabel, menuPath, visibleMenu } from '../data/menu.js'
import { payOnlineHref } from '../lib/socials.js'

function isAuthMenu(item) {
  const id = String(item?.id || item?.slug || item?.type || '').toLowerCase()
  return id === 'login' || id === 'admin'
}

export default function Header({ dark }) {
  const [open, setOpen] = useState(false)
  const [openId, setOpenId] = useState('')
  const leaveTimer = useRef(0)
  const { authed, institute, menus, settings } = useApp()
  const { t, lang, setLang } = useLang()
  const { pathname } = useLocation()
  const name = institute.schoolName || settings.site_name || 'Demo'
  const dialog = String(settings.site_dialog || institute.dialog || '').trim()
  const items = visibleMenu(menus).filter((item) => !isAuthMenu(item))
  const payHref = payOnlineHref(settings)

  function closeAll() {
    window.clearTimeout(leaveTimer.current)
    setOpen(false)
    setOpenId('')
  }

  function openMenu(id) {
    window.clearTimeout(leaveTimer.current)
    setOpenId(id)
  }

  function scheduleClose() {
    window.clearTimeout(leaveTimer.current)
    leaveTimer.current = window.setTimeout(() => setOpenId(''), 200)
  }

  function itemActive(item) {
    const path = menuPath(item)
    if (item.type === 'home') return pathname === '/'
    if (item.type === 'group') {
      return (item.children || []).some((child) => itemActive(child))
    }
    return pathname === path || pathname.startsWith(`${path}/`)
  }

  return (
    <header className={`site-header ${dark ? 'is-dark' : ''}`}>
      <div className="container header-inner">
        <Link to="/" className="brand" onClick={closeAll}>
          <img src={institute.logoUrl || '/logo.svg'} alt={name} width="52" height="52" />
          <span className="brand-text">
            <strong className="brand-name">{name}</strong>
            {dialog ? <small className="brand-dialog">{dialog}</small> : null}
          </span>
        </Link>

        <div className="header-tools">
          {payHref ? (
            <a className="header-pay" href={payHref} target="_blank" rel="noreferrer">
              {t('nav.payOnline')}
            </a>
          ) : null}
          <Link className="header-login" to={authed ? '/admin' : '/login'} onClick={closeAll}>
            {authed ? t('nav.admin') : t('nav.login')}
          </Link>
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

      <div className={`nav-bar ${open ? 'is-open' : ''}`}>
        <div className="container nav-bar-inner">
          <nav>
            {items.map((item) => {
              const children = item.children || []
              const hasKids = children.length > 0
              const path = menuPath(item)
              return (
                <div
                  key={item.id}
                  className={`nav-item ${hasKids ? 'has-kids' : ''} ${openId === item.id ? 'is-open' : ''}`}
                  onMouseEnter={() => {
                    if (hasKids && window.matchMedia('(min-width: 721px)').matches) openMenu(item.id)
                  }}
                  onMouseLeave={() => {
                    if (window.matchMedia('(min-width: 721px)').matches) scheduleClose()
                  }}
                >
                  {hasKids ? (
                    <button
                      type="button"
                      className={itemActive(item) ? 'is-current' : ''}
                      onClick={() => {
                        if (window.matchMedia('(min-width: 721px)').matches) openMenu(item.id)
                        else setOpenId((id) => (id === item.id ? '' : item.id))
                      }}
                    >
                      {menuLabel(item, lang, name)}
                    </button>
                  ) : (
                    <NavLink
                      to={path}
                      end={item.type === 'home'}
                      className={({ isActive }) => (isActive || itemActive(item) ? 'active' : '')}
                      onClick={closeAll}
                    >
                      {menuLabel(item, lang, name)}
                    </NavLink>
                  )}
                  {hasKids ? (
                    <div className="nav-drop">
                      {children.map((child) => (
                        <Link
                          key={child.id}
                          to={menuPath(child)}
                          className={itemActive(child) ? 'active' : ''}
                          onClick={closeAll}
                        >
                          {menuLabel(child, lang, name)}
                        </Link>
                      ))}
                    </div>
                  ) : null}
                </div>
              )
            })}
          </nav>
        </div>
      </div>
    </header>
  )
}
