const TOKEN_KEY = 'safa-propertis-token'

const APP_ROUTES = ['/admin', '/login', '/project', '/blog', '/about', '/contact']

export function appBase() {
  if (typeof window === 'undefined') return ''
  if (typeof window.SAFA_BASE === 'string' && window.SAFA_BASE.trim()) {
    return window.SAFA_BASE.replace(/\/$/, '')
  }
  const script = document.querySelector('script[src*="/assets/"], script[src*="assets/"]')
  if (script) {
    try {
      const pathname = new URL(script.getAttribute('src'), window.location.href).pathname
      const cut = pathname.replace(/\/assets\/[^/]+$/, '').replace(/\/$/, '')
      if (cut) return cut
    } catch {
      /* ignore */
    }
  }
  let path = (window.location.pathname || '/').replace(/\/index\.php$/i, '').replace(/\/+$/, '')
  for (const route of APP_ROUTES) {
    if (path === route || path.endsWith(route)) {
      return path.slice(0, -route.length).replace(/\/$/, '')
    }
    const marker = `${route}/`
    const idx = path.indexOf(marker)
    if (idx !== -1) return path.slice(0, idx).replace(/\/$/, '')
  }
  return path === '/' ? '' : path
}

export function mediaUrl(src) {
  if (!src || /^https?:\/\//i.test(src) || src.startsWith('data:')) return src
  const base = appBase()
  if (!base) return src
  if (src === base || src.startsWith(`${base}/`)) return src
  if (src.startsWith('/')) return `${base}${src}`
  return src
}

function apiUrl(path) {
  const normalized = path.startsWith('/') ? path : `/${path}`
  if (import.meta.env.DEV) {
    return normalized
  }
  const route = normalized.replace(/^\/api\/?/, '')
  return `${appBase()}/api/index.php?route=${encodeURIComponent(route)}`
}

export function getToken() {
  return localStorage.getItem(TOKEN_KEY) || ''
}

export function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

async function request(path, options = {}) {
  const headers = { ...(options.headers || {}) }
  const token = getToken()
  if (token) {
    headers.Authorization = `Bearer ${token}`
    headers['X-Auth-Token'] = token
  }
  if (options.body && !(options.body instanceof FormData) && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json'
  }

  const res = await fetch(path, { ...options, headers })
  const data = await res.json().catch(() => ({}))
  if (!res.ok) {
    throw new Error(data.error || 'Request failed')
  }
  return data
}

export const api = {
  projects: () => request(apiUrl('/api/projects')),
  project: (slug) => request(apiUrl(`/api/projects/${slug}`)),
  createProject: (body) => request(apiUrl('/api/projects'), { method: 'POST', body }),
  deleteProject: (id) => request(apiUrl(`/api/projects/${id}`), { method: 'DELETE' }),
  login: (username, password) =>
    request(apiUrl('/api/auth/login'), {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),
  me: () => request(apiUrl('/api/auth/me')),
  logout: () => request(apiUrl('/api/auth/logout'), { method: 'POST' }),
  sendMessage: (payload) =>
    request(apiUrl('/api/messages'), {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  messages: () => request(apiUrl('/api/messages')),
  deleteMessage: (id) => request(apiUrl(`/api/messages/${id}`), { method: 'DELETE' }),
  subscribe: (email) =>
    request(apiUrl('/api/subscribers'), {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),
  subscribers: () => request(apiUrl('/api/subscribers')),
  deleteSubscriber: (id) => request(apiUrl(`/api/subscribers/${id}`), { method: 'DELETE' }),
  settings: () => request(apiUrl('/api/settings')),
  saveProfile: (body) => request(apiUrl('/api/profile'), { method: 'POST', body }),
  saveSocials: (links) =>
    request(apiUrl('/api/socials'), {
      method: 'POST',
      body: JSON.stringify(links),
    }),
  hero: () => request(apiUrl('/api/hero')),
  saveHero: (body) => request(apiUrl('/api/hero'), { method: 'POST', body }),
  footerGallery: () => request(apiUrl('/api/footer-gallery')),
  saveFooterGallery: (body) => request(apiUrl('/api/footer-gallery'), { method: 'POST', body }),
  posts: () => request(apiUrl('/api/posts')),
  savePost: (body) => request(apiUrl('/api/posts'), { method: 'POST', body }),
  deletePost: (id) => request(apiUrl(`/api/posts/${id}`), { method: 'DELETE' }),
  pages: () => request(apiUrl('/api/pages')),
  savePages: (body) => request(apiUrl('/api/pages'), { method: 'POST', body }),
}
