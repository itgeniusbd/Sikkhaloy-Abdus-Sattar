const TOKEN_KEY = 'iisac-site-token'

const APP_ROUTES = ['/admin', '/login', '/gallery', '/videos', '/blog', '/notices', '/notice', '/events', '/teachers', '/about', '/contact', '/career', '/project', '/p']

export function appBase() {
  if (typeof window === 'undefined') return ''
  const injected = window.SITE_BASE || window.SAFA_BASE
  if (typeof injected === 'string' && injected.trim()) {
    return injected.replace(/\/$/, '')
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
  users: () => request(apiUrl('/api/users')),
  createUser: (body) =>
    request(apiUrl('/api/users'), { method: 'POST', body: JSON.stringify(body) }),
  updateUser: (id, body) =>
    request(apiUrl(`/api/users/${id}`), { method: 'POST', body: JSON.stringify(body) }),
  deleteUser: (id) => request(apiUrl(`/api/users/${id}`), { method: 'DELETE' }),
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
  saveTheme: (theme) =>
    request(apiUrl('/api/theme'), {
      method: 'POST',
      body: JSON.stringify(theme),
    }),
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
  videos: () => request(apiUrl('/api/videos')),
  saveVideos: (videos) => request(apiUrl('/api/videos'), { method: 'POST', body: JSON.stringify({ videos }) }),
  posts: () => request(apiUrl('/api/posts')),
  savePost: (body) => request(apiUrl('/api/posts'), { method: 'POST', body }),
  deletePost: (id) => request(apiUrl(`/api/posts/${id}`), { method: 'DELETE' }),
  pages: () => request(apiUrl('/api/pages')),
  savePages: (body) => request(apiUrl('/api/pages'), { method: 'POST', body }),
  menus: () => request(apiUrl('/api/menus')),
  saveMenus: (body) => request(apiUrl('/api/menus'), { method: 'POST', body }),
  eduHome: () => request(apiUrl('/api/edu/home')),
  admissionForm: () => request(apiUrl('/api/admissions/form')),
  admissionOptions: (params = {}) => {
    const qs = new URLSearchParams()
    Object.entries(params).forEach(([key, value]) => {
      if (value != null && String(value) !== '') qs.set(key, String(value))
    })
    const suffix = qs.toString() ? `?${qs}` : ''
    return request(apiUrl(`/api/admissions/options${suffix}`))
  },
  submitAdmission: (body) => request(apiUrl('/api/admissions'), { method: 'POST', body }),
  admissions: () => request(apiUrl('/api/admissions')),
  confirmAdmission: (id, body = {}) =>
    request(apiUrl(`/api/admissions/${id}/confirm`), { method: 'POST', body: JSON.stringify(body) }),
  deleteAdmission: (id) => request(apiUrl(`/api/admissions/${id}`), { method: 'DELETE' }),
  jobs: () => request(apiUrl('/api/jobs')),
  job: (id) => request(apiUrl(`/api/jobs/${id}`)),
  applyJob: (id, body) => request(apiUrl(`/api/jobs/${id}/apply`), { method: 'POST', body }),
  manageJobs: () => request(apiUrl('/api/jobs/manage')),
  saveJob: (body) => request(apiUrl('/api/jobs'), { method: 'POST', body }),
  deleteJob: (id) => request(apiUrl(`/api/jobs/${id}`), { method: 'DELETE' }),
  jobApplications: (params = {}) => {
    const qs = new URLSearchParams()
    Object.entries(params).forEach(([key, value]) => {
      if (value != null && String(value) !== '') qs.set(key, String(value))
    })
    const suffix = qs.toString() ? `?${qs}` : ''
    return request(apiUrl(`/api/job-applications${suffix}`))
  },
  setJobApplicationStatus: (id, status) =>
    request(apiUrl(`/api/job-applications/${id}/status`), {
      method: 'POST',
      body: JSON.stringify({ status }),
    }),
  async downloadJobCv(id) {
    const headers = {}
    const token = getToken()
    if (token) {
      headers.Authorization = `Bearer ${token}`
      headers['X-Auth-Token'] = token
    }
    const res = await fetch(apiUrl(`/api/job-applications/${id}/cv`), { headers })
    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      throw new Error(data.error || 'Could not download CV.')
    }
    const blob = await res.blob()
    const name = (res.headers.get('content-disposition') || '').match(/filename="?([^"]+)"?/i)?.[1] || `cv-${id}`
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = decodeURIComponent(name)
    link.click()
    URL.revokeObjectURL(url)
  },
}
