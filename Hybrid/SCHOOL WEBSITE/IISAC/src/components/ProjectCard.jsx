import { Link } from 'react-router-dom'
import { IconBath, IconBed, IconPin } from './Icons.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function ProjectCard({ project }) {
  const { t, projectText } = useLang()
  const item = projectText(project)
  return (
    <article className="project-card">
      <Link to={`/project/${project.slug}`} className="card-media">
        <img src={project.image} alt={item.title} />
        {item.badge && <span className="card-badge">{item.badge}</span>}
      </Link>
      <div className="card-body">
        <h3>
          <Link to={`/project/${project.slug}`}>{item.title}</Link>
        </h3>
        <div className="card-meta">
          {project.beds > 0 && (
            <span>
              <IconBed /> {t('common.beds', { n: project.beds })}
            </span>
          )}
          {project.baths > 0 && (
            <span>
              <IconBath /> {t('common.baths', { n: project.baths })}
            </span>
          )}
        </div>
        <div className="card-price">{Number(project.price).toLocaleString()}</div>
        <div className="card-foot">
          <p className="card-loc">
            <IconPin /> {item.location}
          </p>
          <Link to={`/project/${project.slug}`} className="card-cta">
            {t('common.seeDetails')}
          </Link>
        </div>
      </div>
    </article>
  )
}
