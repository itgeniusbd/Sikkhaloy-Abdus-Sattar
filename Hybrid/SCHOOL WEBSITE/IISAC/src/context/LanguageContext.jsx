import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { useApp } from './AppContext.jsx'
import { DEFAULT_PAGES, HERO_SLIDES } from '../data/site.js'
import {
  LANG_KEY,
  featureBn,
  heroBn,
  mapLabel,
  postBn,
  preferBn,
  projectBn,
  serviceBn,
  translate,
} from '../i18n/index.js'

const LanguageContext = createContext(null)

export function LanguageProvider({ children }) {
  const [lang, setLangState] = useState(() => localStorage.getItem(LANG_KEY) || 'en')
  const { institute, settings, site } = useApp()
  const schoolDisplayName =
    institute?.schoolName ||
    settings?.site_name ||
    site?.name ||
    'Demo'

  function setLang(next) {
    setLangState(next)
    localStorage.setItem(LANG_KEY, next)
  }

  useEffect(() => {
    document.documentElement.lang = lang === 'bn' ? 'bn' : 'en'
  }, [lang, schoolDisplayName])

  useEffect(() => {
    const base = schoolDisplayName || 'Institution Website'
    document.title = `${base} | Institution Website`
  }, [schoolDisplayName])

  const value = useMemo(() => {
    const t = (key, vars) => translate(lang, key, { name: schoolDisplayName, ...(vars || {}) })
    return {
      lang,
      setLang,
      t,
      label: (group, value) => mapLabel(lang, group, value),
      feature: (name) => (lang === 'bn' ? featureBn[name] || name : name),
      slideText: (slide) => {
        const pack = heroBn[slide.id]
        const original = HERO_SLIDES.find((item) => item.id === slide.id)
        if (!pack) return slide
        return {
          ...slide,
          eyebrow: preferBn(lang, slide.eyebrow, original?.eyebrow, pack.eyebrow),
          title: preferBn(lang, slide.title, original?.title, pack.title),
          text: preferBn(lang, slide.text, original?.text, pack.text),
        }
      },
      serviceText: (svc) => {
        const pack = serviceBn[svc.id]
        if (lang !== 'bn' || !pack) return svc
        return { ...svc, title: pack.title, desc: pack.desc }
      },
      pageText: (pages) => {
        const about = { ...DEFAULT_PAGES.about, ...pages?.about }
        const contact = { ...DEFAULT_PAGES.contact, ...pages?.contact }
        return {
          about: {
            ...about,
            eyebrow: preferBn(lang, about.eyebrow, DEFAULT_PAGES.about.eyebrow, t('pages.aboutEyebrow')),
            title: preferBn(lang, about.title, DEFAULT_PAGES.about.title, t('pages.aboutTitle')),
            lead: preferBn(lang, about.lead, DEFAULT_PAGES.about.lead, t('pages.aboutLead')),
            body: preferBn(lang, about.body, DEFAULT_PAGES.about.body, t('pages.aboutBody')),
            features: (about.features || []).map((name) => (lang === 'bn' ? featureBn[name] || name : name)),
          },
          contact: {
            ...contact,
            eyebrow: preferBn(lang, contact.eyebrow, DEFAULT_PAGES.contact.eyebrow, t('pages.contactEyebrow')),
            title: preferBn(lang, contact.title, DEFAULT_PAGES.contact.title, t('pages.contactTitle')),
            lead: preferBn(lang, contact.lead, DEFAULT_PAGES.contact.lead, t('pages.contactLead')),
          },
        }
      },
      projectText: (project) => {
        if (!project) return project
        const pack = lang === 'bn' ? projectBn[project.id] : null
        return {
          ...project,
          title: pack?.title || project.title,
          location: pack?.location || project.location,
          description: pack?.description || project.description,
          badge: mapLabel(lang, 'badge', project.badge) || project.badge,
          type: mapLabel(lang, 'type', project.type) || project.type,
          category: mapLabel(lang, 'cat', project.category) || project.category,
          features: (project.features || []).map((name) => (lang === 'bn' ? featureBn[name] || name : name)),
        }
      },
      postText: (post) => {
        if (!post) return post
        const pack = lang === 'bn' ? postBn[post.id] : null
        return {
          ...post,
          title: pack?.title || post.title,
          excerpt: pack?.excerpt || post.excerpt,
          body: pack?.body || post.body,
        }
      },
    }
  }, [lang])

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>
}

export function useLang() {
  const ctx = useContext(LanguageContext)
  if (!ctx) throw new Error('useLang must be used inside LanguageProvider')
  return ctx
}
