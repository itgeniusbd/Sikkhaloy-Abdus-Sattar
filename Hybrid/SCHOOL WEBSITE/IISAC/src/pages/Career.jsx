import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, mediaUrl } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

export default function Career() {
  const { t, lang } = useLang()
  const [jobs, setJobs] = useState([])
  const [error, setError] = useState('')

  useEffect(() => {
    api
      .jobs()
      .then((data) => setJobs(data.jobs || []))
      .catch((err) => setError(err.message || t('career.loadError')))
  }, [t])

  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('career.eyebrow')}</p>
        <h1 className="page-title">{t('career.title')}</h1>
        <p className="lead">{t('career.lead')}</p>
        {error ? <p className="form-error">{error}</p> : null}
        <div className="career-grid">
          {!jobs.length && !error ? <p className="lead">{t('career.empty')}</p> : null}
          {jobs.map((job) => {
            const title = lang === 'bn' ? job.titleBn || job.title : job.title
            const cover = job.image ? mediaUrl(job.image) : ''
            return (
              <article key={job.id} className="career-card">
                <Link to={`/career/${job.id}`} className="career-card-media">
                  {cover ? (
                    <img src={cover} alt="" />
                  ) : (
                    <span className="career-card-fallback" aria-hidden="true">
                      {(title || 'J').slice(0, 1)}
                    </span>
                  )}
                  {job.jobType ? <span className="career-card-badge">{job.jobType}</span> : null}
                </Link>
                <div className="career-card-body">
                  {job.department ? <p className="career-card-dept">{job.department}</p> : null}
                  <h3>
                    <Link to={`/career/${job.id}`}>{title}</Link>
                  </h3>
                  <ul className="career-card-meta">
                    {job.location ? <li>{job.location}</li> : null}
                    {job.deadline ? <li>{t('career.deadline')}: {job.deadline}</li> : null}
                  </ul>
                  <Link to={`/career/${job.id}`} className="career-card-btn">
                    {t('career.view')}
                  </Link>
                </div>
              </article>
            )
          })}
        </div>
      </div>
    </section>
  )
}
