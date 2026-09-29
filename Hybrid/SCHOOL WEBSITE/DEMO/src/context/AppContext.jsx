import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { DEFAULT_POSTS, DEFAULT_PROJECTS } from '../data/projects'
import { DEFAULT_PAGES, HERO_SLIDES, SITE } from '../data/site.js'
import { api, mediaUrl, setToken } from '../lib/api.js'

function withMedia(item) {
  if (!item || typeof item !== 'object') return item
  const next = { ...item }
  if (typeof next.image === 'string') next.image = mediaUrl(next.image)
  if (Array.isArray(next.gallery)) next.gallery = next.gallery.map((src) => (typeof src === 'string' ? mediaUrl(src) : src))
  return next
}

const AppContext = createContext(null)

export function AppProvider({ children }) {
  const [projects, setProjects] = useState(() => DEFAULT_PROJECTS.map(withMedia))
  const [heroSlides, setHeroSlides] = useState(() => HERO_SLIDES.map(withMedia))
  const [footerGallery, setFooterGallery] = useState([])
  const [posts, setPosts] = useState(() => DEFAULT_POSTS.map(withMedia))
  const [pages, setPages] = useState(() => ({
    about: withMedia(DEFAULT_PAGES.about),
    contact: DEFAULT_PAGES.contact,
  }))
  const [settings, setSettings] = useState({})
  const [authed, setAuthed] = useState(false)
  const [dbReady, setDbReady] = useState(false)
  const [dbError, setDbError] = useState('')
  const [loading, setLoading] = useState(true)

  async function refresh() {
    try {
      const [data, hero, footer, blog, sitePages, settingsRes] = await Promise.all([
        api.projects(),
        api.hero().catch(() => null),
        api.footerGallery().catch(() => null),
        api.posts().catch(() => null),
        api.pages().catch(() => null),
        api.settings().catch(() => null),
      ])
      if (Array.isArray(data.projects)) {
        setProjects(data.projects.map(withMedia))
        setDbReady(true)
        setDbError('')
      } else {
        throw new Error(data.error || 'Projects API did not return data')
      }
      if (Array.isArray(hero?.slides) && hero.slides.length) {
        setHeroSlides(hero.slides.map(withMedia))
      }
      if (Array.isArray(footer?.items)) {
        setFooterGallery(footer.items.map(withMedia))
      }
      if (Array.isArray(blog?.posts)) {
        setPosts(blog.posts.map(withMedia))
      }
      if (sitePages?.pages) {
        setPages({
          about: withMedia({ ...DEFAULT_PAGES.about, ...sitePages.pages.about }),
          contact: { ...DEFAULT_PAGES.contact, ...sitePages.pages.contact },
        })
      }
      if (settingsRes?.settings) {
        setSettings(settingsRes.settings)
      }
    } catch (err) {
      setDbReady(false)
      setDbError(err.message || 'MySQL API is not connected')
      setProjects(DEFAULT_PROJECTS)
      setHeroSlides(HERO_SLIDES)
      setPosts(DEFAULT_POSTS)
      setPages(DEFAULT_PAGES)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    refresh()
    api
      .me()
      .then(() => setAuthed(true))
      .catch(() => setAuthed(false))
  }, [])

  const value = useMemo(
    () => ({
      projects,
      heroSlides,
      footerGallery,
      posts,
      pages,
      settings,
      adminProfile: {
        name: settings.admin_name || 'Admin',
        address: settings.admin_address || settings.address || pages.contact?.address || SITE.address,
        photo: mediaUrl(settings.admin_photo || '/admin-avatar.png'),
      },
      site: {
        ...SITE,
        ...pages.contact,
        name: settings.site_name || SITE.name,
        phone: settings.phone || pages.contact?.phone || SITE.phone,
        whatsapp: settings.whatsapp || pages.contact?.whatsapp || SITE.whatsapp,
        email: settings.email || pages.contact?.email || SITE.email,
        address: settings.address || pages.contact?.address || SITE.address,
        about: pages.about?.lead || SITE.about,
      },
      authed,
      dbReady,
      dbError,
      loading,
      async login(user, pass) {
        const data = await api.login(user, pass)
        setToken(data.token)
        setAuthed(true)
        await refresh()
        return true
      },
      async logout() {
        try {
          await api.logout()
        } catch {
          /* still clear local session */
        }
        setToken('')
        setAuthed(false)
      },
      async addProject(project, files = {}) {
        const form = new FormData()
        Object.entries(project).forEach(([key, val]) => {
          if (val == null) return
          form.append(key, typeof val === 'object' ? JSON.stringify(val) : String(val))
        })
        if (files.image) form.append('image', files.image)
        if (files.gallery) {
          ;[...files.gallery].forEach((file) => form.append('gallery', file))
        }
        await api.createProject(form)
        await refresh()
      },
      async deleteProject(id) {
        await api.deleteProject(id)
        await refresh()
      },
      async saveHeroSlides(slides, files = {}) {
        const form = new FormData()
        form.append('slides', JSON.stringify(slides))
        Object.entries(files).forEach(([index, file]) => {
          if (file) form.append(`hero${index}`, file)
        })
        await api.saveHero(form)
        await refresh()
      },
      async savePost(post, imageFile) {
        const form = new FormData()
        Object.entries(post).forEach(([key, val]) => {
          if (val == null) return
          form.append(key, String(val))
        })
        if (imageFile) form.append('postImage', imageFile)
        await api.savePost(form)
        await refresh()
      },
      async deletePost(id) {
        await api.deletePost(id)
        await refresh()
      },
      async saveFooterGallery(items, files = {}) {
        const form = new FormData()
        form.append('items', JSON.stringify(items))
        Object.entries(files).forEach(([index, file]) => {
          if (file) form.append(`foot${index}`, file)
        })
        await api.saveFooterGallery(form)
        await refresh()
      },
      async savePages(nextPages, aboutImage) {
        const form = new FormData()
        form.append('pages', JSON.stringify(nextPages))
        if (aboutImage) form.append('aboutImage', aboutImage)
        await api.savePages(form)
        await refresh()
      },
      async sendMessage(payload) {
        await api.sendMessage(payload)
      },
      async saveProfile(profile, photoFile) {
        const form = new FormData()
        form.append('admin_name', profile.name || '')
        form.append('admin_address', profile.address || '')
        if (profile.photo) form.append('admin_photo', profile.photo)
        if (photoFile) form.append('adminPhoto', photoFile)
        if (profile.currentPassword || profile.newPassword || profile.confirmPassword) {
          form.append('current_password', profile.currentPassword || '')
          form.append('new_password', profile.newPassword || '')
          form.append('confirm_password', profile.confirmPassword || '')
        }
        await api.saveProfile(form)
        await refresh()
      },
      async saveSocials(links) {
        await api.saveSocials(links)
        await refresh()
      },
    }),
    [projects, heroSlides, footerGallery, posts, pages, settings, authed, dbReady, dbError, loading],
  )

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

export function useApp() {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used inside AppProvider')
  return ctx
}
