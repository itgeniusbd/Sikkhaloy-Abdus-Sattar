import { useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import { LOCATIONS, PROPERTY_TYPES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import ProjectCard from '../components/ProjectCard.jsx'

const STATUSES = ['all', 'completed', 'ongoing', 'upcoming']

export default function Projects() {
  const { projects } = useApp()
  const { t, label } = useLang()
  const [params, setParams] = useSearchParams()
  const status = params.get('status') || 'all'
  const city = params.get('city') || ''
  const type = params.get('type') || ''
  const category = params.get('category') || ''
  const q = params.get('q') || ''

  const filtered = useMemo(() => {
    return projects.filter((p) => {
      if (status !== 'all' && p.status !== status) return false
      if (city && p.city !== city) return false
      if (type && p.type !== type) return false
      if (category && p.category !== category) return false
      if (q && !`${p.title} ${p.location} ${p.category}`.toLowerCase().includes(q.toLowerCase())) return false
      return true
    })
  }, [projects, status, city, type, category, q])

  function sync(next) {
    const nextParams = new URLSearchParams()
    if (next.status && next.status !== 'all') nextParams.set('status', next.status)
    if (next.city) nextParams.set('city', next.city)
    if (next.type) nextParams.set('type', next.type)
    if (next.category) nextParams.set('category', next.category)
    if (next.q) nextParams.set('q', next.q)
    setParams(nextParams)
  }

  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('pages.projectsEyebrow')}</p>
        <h1 className="page-title">{t('pages.projectsTitle')}</h1>
        {category && (
          <p className="lead">
            {t('common.category')}: {label('cat', category)}
          </p>
        )}

        <div className="status-tabs">
          {STATUSES.map((item) => (
            <button
              key={item}
              className={status === item ? 'is-active' : ''}
              onClick={() => sync({ status: item, city, type, category, q })}
            >
              {t(`status.${item}`)}
            </button>
          ))}
        </div>

        <div className="search-bar inline">
          <label>
            {t('common.location')}
            <select value={city} onChange={(e) => sync({ status, city: e.target.value, type, category, q })}>
              <option value="">{t('common.allLocations')}</option>
              {LOCATIONS.map((loc) => (
                <option key={loc} value={loc}>
                  {label('loc', loc)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('common.propertyType')}
            <select value={type} onChange={(e) => sync({ status, city, type: e.target.value, category, q })}>
              <option value="">{t('common.allTypes')}</option>
              {PROPERTY_TYPES.map((item) => (
                <option key={item} value={item}>
                  {label('type', item)}
                </option>
              ))}
            </select>
          </label>
          <label>
            {t('common.searchLabel')}
            <input
              value={q}
              onChange={(e) => sync({ status, city, type, category, q: e.target.value })}
              placeholder={t('common.searchPh')}
            />
          </label>
        </div>

        <div className="project-grid">
          {filtered.length ? (
            filtered.map((p) => <ProjectCard key={p.id} project={p} />)
          ) : (
            <p className="empty">{t('common.noProjects')}</p>
          )}
        </div>
      </div>
    </section>
  )
}
