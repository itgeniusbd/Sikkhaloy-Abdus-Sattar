import { useEffect, useState } from 'react'
import ProjectCard from './ProjectCard.jsx'
import { useLang } from '../context/LanguageContext.jsx'

function chunk(list, size) {
  const pages = []
  for (let i = 0; i < list.length; i += size) pages.push(list.slice(i, i + size))
  return pages
}

export default function ProjectSlider({ projects, resetKey }) {
  const { t } = useLang()
  const [perPage, setPerPage] = useState(3)
  const [page, setPage] = useState(0)

  useEffect(() => {
    const update = () => {
      if (window.innerWidth < 720) setPerPage(1)
      else if (window.innerWidth < 980) setPerPage(2)
      else setPerPage(3)
    }
    update()
    window.addEventListener('resize', update)
    return () => window.removeEventListener('resize', update)
  }, [])

  useEffect(() => {
    setPage(0)
  }, [resetKey, perPage])

  if (!projects.length) {
    return <p className="empty">{t('common.noMatch')}</p>
  }

  const pages = chunk(projects, perPage)
  const last = pages.length - 1

  return (
    <div className="project-slider">
      <button
        type="button"
        className="slider-nav"
        aria-label="Previous projects"
        disabled={page <= 0}
        onClick={() => setPage((p) => Math.max(0, p - 1))}
      >
        ‹
      </button>
      <div className="project-slider-window">
        <div className="project-slider-track" style={{ transform: `translateX(-${page * 100}%)` }}>
          {pages.map((group, i) => (
            <div key={i} className="project-slider-page">
              {group.map((project) => (
                <ProjectCard key={project.id} project={project} />
              ))}
            </div>
          ))}
        </div>
      </div>
      <button
        type="button"
        className="slider-nav"
        aria-label="Next projects"
        disabled={page >= last}
        onClick={() => setPage((p) => Math.min(last, p + 1))}
      >
        ›
      </button>
    </div>
  )
}
