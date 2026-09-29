import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { DEFAULT_POSTS, DEFAULT_PROJECTS } from '../data/projects'
import { DEFAULT_PAGES, HERO_SLIDES, SITE } from '../data/site.js'
import { DEFAULT_CMS_PAGES, DEFAULT_MENU, isAdminBodyHint, normalizeMenus } from '../data/menu.js'
import { canAccess as pageAllowed, isSuperAdmin } from '../lib/adminAccess.js'
import { api, mediaUrl, setToken } from '../lib/api.js'
import { applyTheme, themeFromSettings } from '../lib/theme.js'

function withMedia(item) {
  if (!item || typeof item !== 'object') return item
  const next = { ...item }
  if (typeof next.image === 'string') next.image = mediaUrl(next.image)
  if (typeof next.photoUrl === 'string') next.photoUrl = mediaUrl(next.photoUrl)
  if (typeof next.logoUrl === 'string') next.logoUrl = mediaUrl(next.logoUrl)
  if (Array.isArray(next.gallery)) next.gallery = next.gallery.map((src) => (typeof src === 'string' ? mediaUrl(src) : src))
  if (Array.isArray(next.members)) next.members = next.members.map(withMedia)
  return next
}

const EMPTY_INSTITUTE = {
  schoolID: 0,
  schoolName: SITE.name,
  dialog: SITE.about,
  established: '',
  principal: '',
  address: SITE.address,
  phone: SITE.phone,
  email: SITE.email,
  website: '',
  logoUrl: '',
}

const EMPTY_STATS = { students: 0, teachers: 0, staff: 0, educationYear: '', classes: [] }

const AppContext = createContext(null)

export function AppProvider({ children }) {
  const [projects, setProjects] = useState(() => DEFAULT_PROJECTS.map(withMedia))
  const [heroSlides, setHeroSlides] = useState(() => HERO_SLIDES.map(withMedia))
  const [footerGallery, setFooterGallery] = useState([])
  const [videos, setVideos] = useState([])
  const [posts, setPosts] = useState(() => DEFAULT_POSTS.map(withMedia))
  const [pages, setPages] = useState(() => ({
    about: withMedia(DEFAULT_PAGES.about),
    contact: DEFAULT_PAGES.contact,
  }))
  const [settings, setSettings] = useState({})
  const [menus, setMenus] = useState(DEFAULT_MENU)
  const [cmsPages, setCmsPages] = useState(DEFAULT_CMS_PAGES)
  const [institute, setInstitute] = useState(EMPTY_INSTITUTE)
  const [eduStats, setEduStats] = useState(EMPTY_STATS)
  const [teachers, setTeachers] = useState([])
  const [staff, setStaff] = useState([])
  const [students, setStudents] = useState([])
  const [birthdays, setBirthdays] = useState([])
  const [notices, setNotices] = useState([])
  const [authed, setAuthed] = useState(false)
  const [currentUser, setCurrentUser] = useState(null)
  const [dbReady, setDbReady] = useState(false)
  const [dbError, setDbError] = useState('')
  const [loading, setLoading] = useState(true)

  async function refresh() {
    try {
      const [data, hero, footer, videoRes, blog, sitePages, settingsRes, edu, menuRes] = await Promise.all([
        api.projects().catch((err) => ({ error: err.message })),
        api.hero().catch(() => null),
        api.footerGallery().catch(() => null),
        api.videos().catch(() => null),
        api.posts().catch(() => null),
        api.pages().catch(() => null),
        api.settings().catch(() => null),
        api.eduHome().catch(() => null),
        api.menus().catch(() => null),
      ])
      if (edu?.school) {
        setInstitute(withMedia({ ...EMPTY_INSTITUTE, ...edu.school }))
        setEduStats({
          students: edu.stats?.students || 0,
          teachers: edu.stats?.teachers || 0,
          staff: edu.stats?.staff || 0,
          educationYear: edu.stats?.educationYear || '',
          classes: Array.isArray(edu.stats?.classes) ? edu.stats.classes : [],
        })
        setTeachers((edu.teachers || []).map(withMedia))
        setStaff((edu.staff || []).map(withMedia))
        setStudents((edu.students || []).map(withMedia))
        setBirthdays((edu.birthdays || []).map(withMedia))
        setNotices(Array.isArray(edu.notices) ? edu.notices.filter((item) => item?.title) : [])
      } else {
        // Education API offline/unavailable: fall back to Admin-edited site settings
        const saved = settingsRes?.settings || {}
        setInstitute({
          ...EMPTY_INSTITUTE,
          schoolName: saved.site_name || SITE.name,
          dialog: saved.site_dialog || SITE.about,
          phone: saved.phone || EMPTY_INSTITUTE.phone,
          email: saved.email || EMPTY_INSTITUTE.email,
          address: saved.address || EMPTY_INSTITUTE.address,
          logoUrl: '',
        })
        setEduStats(EMPTY_STATS)
        setTeachers([])
        setStaff([])
        setStudents([])
        setBirthdays([])
        setNotices([])
      }
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
      if (Array.isArray(videoRes?.videos)) {
        setVideos(videoRes.videos)
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
      if (Array.isArray(menuRes?.menus) && menuRes.menus.length) {
        setMenus(normalizeMenus(menuRes.menus))
      }
      if (menuRes?.pages && typeof menuRes.pages === 'object') {
        const nextPages = { ...DEFAULT_CMS_PAGES }
        Object.entries(menuRes.pages).forEach(([key, stored]) => {
          const base = DEFAULT_CMS_PAGES[key] || {}
          nextPages[key] = {
            ...base,
            ...stored,
            members: Array.isArray(stored?.members) ? stored.members : base.members || [],
            body: isAdminBodyHint(stored?.body) ? '' : stored?.body ?? base.body,
            bodyBn: isAdminBodyHint(stored?.bodyBn) ? '' : stored?.bodyBn ?? base.bodyBn,
            lead: isAdminBodyHint(stored?.lead) ? '' : stored?.lead ?? base.lead,
            leadBn: isAdminBodyHint(stored?.leadBn) ? '' : stored?.leadBn ?? base.leadBn,
          }
        })
        Object.keys(nextPages).forEach((key) => {
          nextPages[key] = withMedia(nextPages[key])
        })
        setCmsPages(nextPages)
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
    applyTheme(themeFromSettings(settings))
  }, [
    settings.theme_primary,
    settings.theme_button,
    settings.theme_menu_hover,
    settings.theme_sidebar,
    settings.theme_menu_bg,
    settings.theme_nav_bg,
  ])

  useEffect(() => {
    refresh()
    api
      .me()
      .then((data) => {
        setAuthed(true)
        setCurrentUser(data.user || { role: 'admin', displayName: 'Admin', permissions: [] })
      })
      .catch(() => {
        setAuthed(false)
        setCurrentUser(null)
      })
  }, [])

  const value = useMemo(
    () => ({
      projects,
      heroSlides,
      footerGallery,
      videos,
      posts,
      pages,
      settings,
      menus,
      cmsPages,
      institute,
      eduStats,
      teachers,
      staff,
      students,
      birthdays,
      notices,
      currentUser,
      isSuperAdmin: isSuperAdmin(currentUser),
      canAccess: (page) => pageAllowed(currentUser, page),
      adminProfile: {
        name: currentUser?.displayName || settings.admin_name || 'Admin',
        address: settings.admin_address || settings.address || pages.contact?.address || SITE.address,
        photo: mediaUrl(
          currentUser?.photo ||
            (currentUser?.role === 'admin' ? settings.admin_photo : '') ||
            '/admin-avatar.png',
        ),
        designation: currentUser?.designation || '',
        role: currentUser?.role || '',
        username: currentUser?.username || '',
      },
      site: {
        ...SITE,
        ...pages.contact,
        name: institute.schoolName || settings.site_name || SITE.name,
        phone: institute.phone || settings.phone || pages.contact?.phone || SITE.phone,
        whatsapp: settings.whatsapp || pages.contact?.whatsapp || SITE.whatsapp,
        email: institute.email || settings.email || pages.contact?.email || SITE.email,
        address: institute.address || settings.address || pages.contact?.address || SITE.address,
        about: (() => {
          const raw = settings.site_dialog || institute.dialog || pages.about?.lead || SITE.about || ''
          const school = institute.schoolName || settings.site_name || SITE.name || 'Demo'
          return String(raw).replace(/\bIISAC\b/gi, school).replace(/\bDemo\b/g, school)
        })(),
      },
      authed,
      dbReady,
      dbError,
      loading,
      async login(user, pass) {
        const data = await api.login(user, pass)
        setToken(data.token)
        setCurrentUser(data.user || null)
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
        setCurrentUser(null)
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
      async saveVideos(nextVideos) {
        await api.saveVideos(nextVideos)
        await refresh()
      },
      async saveMenus(nextMenus, nextCmsPages, image, memberFiles = {}) {
        const form = new FormData()
        form.append('menus', JSON.stringify(nextMenus))
        form.append('pages', JSON.stringify(nextCmsPages))
        const slug = image?.slug || ''
        if (slug) form.append('pageSlug', slug)
        if (image?.slug && image.file) {
          form.append('pageImage', image.file)
        }
        Object.entries(memberFiles || {}).forEach(([id, file]) => {
          if (file) form.append(`member_${id}`, file)
        })
        await api.saveMenus(form)
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
        form.append('designation', profile.designation || '')
        form.append('admin_address', profile.address || '')
        const existingPhoto = String(profile.photo || '')
        if (existingPhoto && !existingPhoto.includes('admin-avatar.png')) {
          form.append('admin_photo', existingPhoto)
        }
        if (photoFile) form.append('adminPhoto', photoFile)
        if (profile.currentPassword || profile.newPassword || profile.confirmPassword) {
          form.append('current_password', profile.currentPassword || '')
          form.append('new_password', profile.newPassword || '')
          form.append('confirm_password', profile.confirmPassword || '')
        }
        const saved = await api.saveProfile(form)
        if (saved?.user) setCurrentUser(saved.user)
        else {
          const me = await api.me().catch(() => null)
          if (me?.user) setCurrentUser(me.user)
        }
        await refresh()
      },
      async saveSocials(links) {
        await api.saveSocials(links)
        await refresh()
      },
      async saveTheme(theme) {
        const saved = await api.saveTheme(theme)
        if (saved?.settings) setSettings(saved.settings)
        else await refresh()
        applyTheme(themeFromSettings(saved?.settings || theme))
      },
    }),
    [projects, heroSlides, footerGallery, videos, posts, pages, settings, menus, cmsPages, institute, eduStats, teachers, staff, students, birthdays, notices, authed, currentUser, dbReady, dbError, loading],
  )

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

export function useApp() {
  const ctx = useContext(AppContext)
  if (!ctx) throw new Error('useApp must be used inside AppProvider')
  return ctx
}
