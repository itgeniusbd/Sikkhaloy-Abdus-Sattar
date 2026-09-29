import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import Sidebar from '../components/Sidebar.jsx'
import {
  IconBath,
  IconBed,
  IconCheck,
  IconPhone,
  IconRoom,
  IconSize,
  IconSpace,
  IconType,
  IconWhatsApp,
} from '../components/Icons.jsx'
import { displayPhone, telHref, waHref } from '../lib/phone.js'

export default function ProjectDetail() {
  const { slug } = useParams()
  const { projects, site } = useApp()
  const { t, projectText } = useLang()
  const project = projects.find((p) => p.slug === slug)
  const [active, setActive] = useState(0)

  if (!project) {
    return (
      <section className="page-pad container">
        <h1>{t('common.notFoundProject')}</h1>
        <Link to="/project">{t('common.backProjects')}</Link>
      </section>
    )
  }

  const item = projectText(project)
  const gallery = project.gallery?.length ? project.gallery : [project.image]
  const phone = project.phone || site.phone
  const whatsapp = project.whatsapp || site.whatsapp
  const specs = [
    { icon: IconRoom, label: t('common.room'), value: t('common.rooms', { n: project.rooms || 0 }) },
    { icon: IconBed, label: t('common.bed'), value: t('common.beds', { n: project.beds || 0 }) },
    { icon: IconBath, label: t('common.bath'), value: t('common.baths', { n: project.baths || 0 }) },
    { icon: IconSpace, label: t('common.space'), value: t('common.spaces', { n: project.parking || 0 }) },
    { icon: IconSize, label: t('common.size'), value: t('common.sqft', { n: project.size || 0 }) },
    { icon: IconType, label: t('common.propertyType'), value: item.type },
  ]

  return (
    <section className="page-pad detail-page">
      <div className="container detail-grid">
        <div>
          <img className="detail-hero" src={gallery[active]} alt={item.title} />
          {gallery.length > 1 && (
            <div className="thumbs">
              {gallery.map((src, i) => (
                <button key={src + i} className={i === active ? 'is-active' : ''} onClick={() => setActive(i)}>
                  <img src={src} alt="" />
                </button>
              ))}
            </div>
          )}
          <h1 className="page-title">{item.title}</h1>
          <p className="lead">{item.description}</p>
          <p className="detail-price">{Number(project.price).toLocaleString()} BDT</p>

          <h2 className="subhead">{t('common.preview')}</h2>
          <div className="preview-grid">
            {specs.map((s) => (
              <div key={s.label} className="preview-item">
                <span className="preview-icon">
                  <s.icon />
                </span>
                <div>
                  <small>{s.label}</small>
                  <strong>{s.value}</strong>
                </div>
              </div>
            ))}
          </div>

          <h2 className="subhead">{t('common.features')}</h2>
          <div className="feature-grid">
            {item.features.map((f) => (
              <div key={f} className="check-item">
                <span className="check">
                  <IconCheck />
                </span>
                {f}
              </div>
            ))}
          </div>

          <h2 className="subhead">{t('common.address')}</h2>
          <p>{item.location}</p>

          <div className="project-contact">
            <h2 className="subhead">{t('common.contact')}</h2>
            <p className="contact-number">
              <IconPhone />
              <a href={telHref(phone)}>{displayPhone(phone)}</a>
            </p>
            <div className="contact-actions">
              <a className="btn-orange" href={telHref(phone)}>
                <IconPhone /> {t('common.callNow')}
              </a>
              <a
                className="btn-whatsapp"
                href={waHref(whatsapp, t('common.waProject', { title: item.title }))}
                target="_blank"
                rel="noreferrer"
              >
                <IconWhatsApp /> {t('common.whatsapp')}
              </a>
            </div>
          </div>
        </div>
        <Sidebar />
      </div>
    </section>
  )
}
