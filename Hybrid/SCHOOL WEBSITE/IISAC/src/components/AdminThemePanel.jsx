import { useEffect, useState } from 'react'
import { createPortal } from 'react-dom'
import { applyTheme, DEFAULT_THEME, THEME_PRESETS, lighten, normalizeHex, themeFromSettings } from '../lib/theme.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { IconClose } from './Icons.jsx'

function ColorRow({ label, value, onChange }) {
  return (
    <label className="admin-theme-row">
      <span>{label}</span>
      <span className="admin-theme-pick">
        <input type="color" value={normalizeHex(value)} onChange={(e) => onChange(e.target.value)} />
        <input value={value} onChange={(e) => onChange(e.target.value)} maxLength={7} />
      </span>
    </label>
  )
}

export default function AdminThemePanel({ onClose }) {
  const { settings, saveTheme } = useApp()
  const { t } = useLang()
  const [theme, setTheme] = useState(() => themeFromSettings(settings))
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setTheme(themeFromSettings(settings))
  }, [
    settings.theme_primary,
    settings.theme_button,
    settings.theme_menu_hover,
    settings.theme_sidebar,
    settings.theme_menu_bg,
    settings.theme_nav_bg,
    settings.theme_brand,
    settings.theme_header_bg,
    settings.theme_header_mode,
    settings.site_name,
    settings.site_dialog,
  ])

  useEffect(() => {
    const prev = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    function onKey(e) {
      if (e.key === 'Escape') onClose?.()
    }
    window.addEventListener('keydown', onKey)
    return () => {
      document.body.style.overflow = prev
      window.removeEventListener('keydown', onKey)
    }
  }, [onClose])

  function change(next) {
    setTheme(next)
    applyTheme(next)
    setSaved(false)
  }

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    try {
      await saveTheme(theme)
      setSaved(true)
    } catch (err) {
      setError(err.message || t('admin.themeError'))
    } finally {
      setBusy(false)
    }
  }

  function reset() {
    change({
      ...DEFAULT_THEME,
      siteName: theme.siteName || '',
      siteDialog: theme.siteDialog || '',
    })
  }

  const panel = (
    <div
      className="admin-theme-overlay"
      role="presentation"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose?.()
      }}
    >
      <form className="admin-theme-panel admin-theme-panel-wide" onSubmit={submit} role="dialog" aria-labelledby="admin-theme-title">
        <div className="admin-theme-head">
          <strong id="admin-theme-title">{t('admin.theme')}</strong>
          <button type="button" className="admin-theme-close" onClick={onClose} aria-label="Close">
            <IconClose />
          </button>
        </div>
        <p>{t('admin.themeLead')}</p>

        <div className="admin-theme-section">
          <h4>{t('admin.siteIdentity')}</h4>
          <label className="admin-theme-field">
            <span>{t('admin.siteName')}</span>
            <input
              value={theme.siteName || ''}
              onChange={(e) => change({ ...theme, siteName: e.target.value })}
              placeholder="Demo"
              maxLength={120}
            />
          </label>
          <label className="admin-theme-field">
            <span>{t('admin.siteDialog')}</span>
            <input
              value={theme.siteDialog || ''}
              onChange={(e) => change({ ...theme, siteDialog: e.target.value })}
              placeholder={t('admin.siteDialogHint')}
              maxLength={160}
            />
          </label>
          <p className="admin-theme-hint">{t('admin.siteIdentityHint')}</p>
        </div>

        <div className="admin-theme-section">
          <h4>{t('admin.headerStyle')}</h4>
          <div className="admin-theme-mode">
            <button
              type="button"
              className={theme.headerMode !== 'dark' ? 'is-active' : ''}
              onClick={() =>
                change({
                  ...theme,
                  headerMode: 'light',
                  headerBg: '#ffffff',
                  brand: theme.headerMode === 'dark' ? '#161616' : theme.brand,
                })
              }
            >
              {t('admin.headerLight')}
            </button>
            <button
              type="button"
              className={theme.headerMode === 'dark' ? 'is-active' : ''}
              onClick={() =>
                change({
                  ...theme,
                  headerMode: 'dark',
                  headerBg: '#111111',
                  brand: '#ffffff',
                })
              }
            >
              {t('admin.headerDark')}
            </button>
          </div>
          <div className="admin-theme-grid">
            <ColorRow
              label={t('admin.themeHeaderBg')}
              value={theme.headerBg}
              onChange={(headerBg) => change({ ...theme, headerBg })}
            />
            <ColorRow
              label={t('admin.themeBrand')}
              value={theme.brand}
              onChange={(brand) => change({ ...theme, brand })}
            />
          </div>
        </div>

        <div className="admin-theme-section">
          <h4>{t('admin.themeColors')}</h4>
          <div className="admin-theme-presets">
            {THEME_PRESETS.map((item) => (
              <button
                key={item.id}
                type="button"
                className={theme.primary === item.primary ? 'is-active' : ''}
                style={{ background: item.primary }}
                aria-label={item.id}
                onClick={() =>
                  change({
                    ...theme,
                    primary: item.primary,
                    button: item.primary,
                    menuHover: item.primary,
                    navBg: lighten(item.primary, 0.9),
                  })
                }
              />
            ))}
          </div>
          <div className="admin-theme-grid">
            <ColorRow label={t('admin.themePrimary')} value={theme.primary} onChange={(primary) => change({ ...theme, primary })} />
            <ColorRow label={t('admin.themeButton')} value={theme.button} onChange={(button) => change({ ...theme, button })} />
            <ColorRow label={t('admin.themeMenuHover')} value={theme.menuHover} onChange={(menuHover) => change({ ...theme, menuHover })} />
            <ColorRow label={t('admin.themeSidebar')} value={theme.sidebar} onChange={(sidebar) => change({ ...theme, sidebar })} />
            <ColorRow label={t('admin.themeMenuBg')} value={theme.menuBg} onChange={(menuBg) => change({ ...theme, menuBg })} />
            <ColorRow label={t('admin.themeNavBg')} value={theme.navBg} onChange={(navBg) => change({ ...theme, navBg })} />
          </div>
        </div>

        {error ? <small className="form-error">{error}</small> : null}
        <div className="admin-theme-actions">
          <button className="btn-orange" type="submit" disabled={busy}>
            {busy ? t('admin.profileSaving') : saved ? t('admin.themeSaved') : t('admin.themeSave')}
          </button>
          <button type="button" className="btn-ghost" onClick={reset}>
            {t('admin.themeReset')}
          </button>
        </div>
      </form>
    </div>
  )

  return createPortal(panel, document.body)
}
