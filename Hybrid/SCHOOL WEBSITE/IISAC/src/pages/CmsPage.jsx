import { Link, Navigate, useParams } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { DEFAULT_CMS_PAGES, findMenuItem, isAdminBodyHint, menuLabel } from '../data/menu.js'
import PeopleGrid from '../components/PeopleGrid.jsx'
import ApplyOnline from './ApplyOnline.jsx'

export default function CmsPage() {
  const { slug } = useParams()
  const { menus, cmsPages } = useApp()
  const { lang, t } = useLang()
  if (slug === 'events') return <Navigate to="/events" replace />
  if (slug === 'video-gallery' || slug === 'videos') return <Navigate to="/videos" replace />
  if (['teachers', 'teacher-day', 'teacher-morning', 'office-staff'].includes(slug)) {
    return <Navigate to="/teachers" replace />
  }
  if (slug === 'apply-online') return <ApplyOnline />
  if (slug === 'career') return <Navigate to="/career" replace />
  const item = findMenuItem(menus, slug)
  const stored = cmsPages[slug] || cmsPages[item?.id] || DEFAULT_CMS_PAGES[slug] || DEFAULT_CMS_PAGES[item?.id] || {}
  const title = lang === 'bn' ? stored.titleBn || stored.title || menuLabel(item, lang) : stored.title || menuLabel(item, lang)
  const eyebrow = lang === 'bn' ? stored.eyebrowBn || stored.eyebrow || '' : stored.eyebrow || ''
  const rawLead = lang === 'bn' ? stored.leadBn || stored.lead || '' : stored.lead || ''
  const rawBody = lang === 'bn' ? stored.bodyBn || stored.body || '' : stored.body || ''
  const lead = isAdminBodyHint(rawLead) ? '' : rawLead
  const body = isAdminBodyHint(rawBody) ? '' : rawBody
  const members = (stored.members || []).map((member) => ({
    id: member.id,
    name: lang === 'bn' ? member.nameBn || member.name : member.name,
    title: lang === 'bn' ? member.titleBn || member.title : member.title,
    qualification: lang === 'bn' ? member.qualificationBn || member.qualification : member.qualification,
    photoUrl: member.image,
  }))

  if (!item && !stored.title && !stored.body) {
    return (
      <section className="page-pad">
        <div className="container">
          <h1 className="page-title">{t('pages.notFound')}</h1>
          <Link to="/" className="btn-outline">
            {t('nav.home')}
          </Link>
        </div>
      </section>
    )
  }

  return (
    <section className="page-pad">
      <div className="container cms-page">
        {eyebrow ? <p className="eyebrow">{eyebrow}</p> : null}
        <h1 className="page-title">{title}</h1>
        {lead ? <p className="lead">{lead}</p> : null}
        {stored.image ? <img className="cms-photo" src={stored.image} alt={title} /> : null}
        {body ? (
          <div className="cms-body">
            {String(body)
              .split(/\n{2,}/)
              .filter(Boolean)
              .map((block, i) => (
                <p key={i}>{block}</p>
              ))}
          </div>
        ) : null}
        {members.length ? <PeopleGrid people={members} variant="official" /> : null}
      </div>
    </section>
  )
}
