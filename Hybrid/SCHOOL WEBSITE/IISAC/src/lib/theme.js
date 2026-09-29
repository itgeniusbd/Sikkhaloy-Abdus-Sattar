export const DEFAULT_THEME = {
  primary: '#f58634',
  button: '#f58634',
  menuHover: '#f58634',
  sidebar: '#141414',
  menuBg: '#252525',
  navBg: '#fff4ea',
  brand: '#161616',
  headerBg: '#ffffff',
  headerMode: 'light',
}

export const THEME_PRESETS = [
  { id: 'orange', primary: '#f58634' },
  { id: 'green', primary: '#2e7d32' },
  { id: 'blue', primary: '#1b4f8a' },
  { id: 'teal', primary: '#0d7377' },
  { id: 'maroon', primary: '#8b1e3f' },
  { id: 'purple', primary: '#6b3fa0' },
]

function clamp(n) {
  return Math.max(0, Math.min(255, Math.round(n)))
}

export function normalizeHex(value, fallback = DEFAULT_THEME.primary) {
  const raw = String(value || '').trim()
  if (/^#[0-9a-f]{6}$/i.test(raw)) return raw.toLowerCase()
  if (/^#[0-9a-f]{3}$/i.test(raw)) {
    const [, a, b, c] = raw.toLowerCase()
    return `#${a}${a}${b}${b}${c}${c}`
  }
  return fallback
}

function rgb(hex) {
  const n = normalizeHex(hex).slice(1)
  return [parseInt(n.slice(0, 2), 16), parseInt(n.slice(2, 4), 16), parseInt(n.slice(4, 6), 16)]
}

function toHex(r, g, b) {
  return `#${[r, g, b].map((n) => clamp(n).toString(16).padStart(2, '0')).join('')}`
}

export function darken(hex, amount = 0.16) {
  const [r, g, b] = rgb(hex)
  return toHex(r * (1 - amount), g * (1 - amount), b * (1 - amount))
}

export function lighten(hex, amount = 0.2) {
  const [r, g, b] = rgb(hex)
  return toHex(r + (255 - r) * amount, g + (255 - g) * amount, b + (255 - b) * amount)
}

export function contrastText(hex) {
  const [r, g, b] = rgb(hex)
  return (r * 299 + g * 587 + b * 114) / 1000 > 148 ? '#161616' : '#f4f4f4'
}

export function themeFromSettings(settings = {}) {
  const primary = normalizeHex(settings.theme_primary, DEFAULT_THEME.primary)
  const headerMode = String(settings.theme_header_mode || DEFAULT_THEME.headerMode).toLowerCase() === 'dark' ? 'dark' : 'light'
  return {
    primary,
    button: normalizeHex(settings.theme_button, primary),
    menuHover: normalizeHex(settings.theme_menu_hover, primary),
    sidebar: normalizeHex(settings.theme_sidebar, DEFAULT_THEME.sidebar),
    menuBg: normalizeHex(settings.theme_menu_bg, DEFAULT_THEME.menuBg),
    navBg: normalizeHex(settings.theme_nav_bg, DEFAULT_THEME.navBg),
    brand: normalizeHex(settings.theme_brand, DEFAULT_THEME.brand),
    headerBg: normalizeHex(settings.theme_header_bg, DEFAULT_THEME.headerBg),
    headerMode,
    siteName: String(settings.site_name || '').trim(),
    siteDialog: String(settings.site_dialog || '').trim(),
  }
}

export function applyTheme(theme) {
  if (typeof document === 'undefined') return
  const next = {
    primary: normalizeHex(theme?.primary, DEFAULT_THEME.primary),
    button: normalizeHex(theme?.button, theme?.primary || DEFAULT_THEME.button),
    menuHover: normalizeHex(theme?.menuHover, theme?.primary || DEFAULT_THEME.menuHover),
    sidebar: normalizeHex(theme?.sidebar, DEFAULT_THEME.sidebar),
    menuBg: normalizeHex(theme?.menuBg, DEFAULT_THEME.menuBg),
    navBg: normalizeHex(theme?.navBg, DEFAULT_THEME.navBg),
    brand: normalizeHex(theme?.brand, DEFAULT_THEME.brand),
    headerBg: normalizeHex(theme?.headerBg, DEFAULT_THEME.headerBg),
    headerMode: String(theme?.headerMode || DEFAULT_THEME.headerMode).toLowerCase() === 'dark' ? 'dark' : 'light',
  }
  const root = document.documentElement
  root.style.setProperty('--orange', next.primary)
  root.style.setProperty('--orange-dark', darken(next.button, 0.16))
  root.style.setProperty('--orange-light', lighten(next.button, 0.22))
  root.style.setProperty('--theme-button', next.button)
  root.style.setProperty('--theme-menu-hover', next.menuHover)
  root.style.setProperty('--theme-sidebar', next.sidebar)
  root.style.setProperty('--theme-sidebar-text', contrastText(next.sidebar))
  root.style.setProperty('--theme-menu-bg', next.menuBg)
  root.style.setProperty('--theme-menu-text', contrastText(next.menuBg))
  root.style.setProperty('--theme-nav-bg', next.navBg)
  root.style.setProperty('--theme-nav-text', contrastText(next.navBg))
  root.style.setProperty('--theme-brand', next.brand)
  root.style.setProperty('--theme-header-bg', next.headerBg)
  root.style.setProperty('--theme-brand-dialog', next.primary)
  return next
}
