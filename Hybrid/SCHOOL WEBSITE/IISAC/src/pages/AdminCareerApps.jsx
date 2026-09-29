import { Fragment, useEffect, useState } from 'react'
import { api } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

const STATUSES = ['pending', 'shortlisted', 'rejected', 'confirmed']

export default function AdminCareerApps() {
  const { t } = useLang()
  const [jobs, setJobs] = useState([])
  const [apps, setApps] = useState([])
  const [jobFilter, setJobFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [query, setQuery] = useState('')
  const [openId, setOpenId] = useState(0)
  const [error, setError] = useState('')

  async function loadJobs() {
    const data = await api.manageJobs()
    setJobs(data.jobs || [])
  }

  async function loadApps() {
    const data = await api.jobApplications({
      jobId: jobFilter,
      status: statusFilter,
    })
    setApps(data.applications || [])
  }

  useEffect(() => {
    loadJobs().catch((err) => setError(err.message || t('admin.careerError')))
  }, [t])

  useEffect(() => {
    loadApps().catch((err) => setError(err.message || t('admin.careerError')))
  }, [jobFilter, statusFilter])

  async function setStatus(app, status) {
    setError('')
    try {
      await api.setJobApplicationStatus(app.id, status)
      await loadApps()
    } catch (err) {
      setError(err.message || t('admin.careerError'))
    }
  }

  async function downloadCv(app) {
    setError('')
    try {
      await api.downloadJobCv(app.id)
    } catch (err) {
      setError(err.message || t('admin.careerError'))
    }
  }

  const filtered = apps.filter((app) => {
    const q = query.trim().toLowerCase()
    if (!q) return true
    return [app.name, app.phone, app.email, app.jobTitle].some((value) => String(value || '').toLowerCase().includes(q))
  })

  return (
    <div className="container admin-career">
      <p className="lead">{t('admin.careerAppsLead')}</p>
      {error ? <p className="form-error">{error}</p> : null}

      <div className="adm-search career-filters">
        <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder={t('admin.careerSearch')} />
        <select value={jobFilter} onChange={(e) => setJobFilter(e.target.value)}>
          <option value="">{t('admin.careerAllJobs')}</option>
          {jobs.map((job) => (
            <option key={job.id} value={job.id}>{job.title}</option>
          ))}
        </select>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">{t('admin.careerAllStatus')}</option>
          {STATUSES.map((status) => (
            <option key={status} value={status}>{t(`admin.careerStatus_${status}`)}</option>
          ))}
        </select>
      </div>

      <div className="adm-admin-table-wrap">
        <table className="adm-admin-table">
          <thead>
            <tr>
              <th>{t('admin.colDate')}</th>
              <th>{t('admin.careerJob')}</th>
              <th>{t('career.name')}</th>
              <th>{t('career.phone')}</th>
              <th>{t('admin.colStatus')}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {filtered.map((app) => (
              <Fragment key={app.id}>
                <tr>
                  <td>{String(app.createdAt || '').slice(0, 16)}</td>
                  <td>{app.jobTitle}</td>
                  <td>
                    <strong>{app.name}</strong>
                    <small>{app.email}</small>
                  </td>
                  <td>{app.phone}</td>
                  <td>
                    <span className={`adm-status is-${app.status || 'pending'}`}>
                      {t(`admin.careerStatus_${app.status || 'pending'}`)}
                    </span>
                  </td>
                  <td className="career-app-actions">
                    <button type="button" className="btn-ghost" onClick={() => setOpenId(openId === app.id ? 0 : app.id)}>
                      {openId === app.id ? t('admin.hideDetails') : t('admin.viewDetails')}
                    </button>
                    <button type="button" className="btn-ghost" onClick={() => downloadCv(app)}>{t('admin.careerCv')}</button>
                    {app.status !== 'shortlisted' ? (
                      <button type="button" className="btn-ghost" onClick={() => setStatus(app, 'shortlisted')}>{t('admin.careerShortlist')}</button>
                    ) : null}
                    {app.status !== 'confirmed' ? (
                      <button type="button" className="btn-orange" onClick={() => setStatus(app, 'confirmed')}>{t('admin.careerConfirm')}</button>
                    ) : null}
                    {app.status !== 'rejected' ? (
                      <button type="button" className="btn-del" onClick={() => setStatus(app, 'rejected')}>{t('admin.careerReject')}</button>
                    ) : null}
                  </td>
                </tr>
                {openId === app.id ? (
                  <tr>
                    <td colSpan="6">
                      <div className="career-app-more">
                        {app.address ? <p>{t('career.address')}: {app.address}</p> : null}
                        {app.education ? <p>{t('career.education')}: {app.education}</p> : null}
                        {app.experience ? <p>{t('career.experience')}: {app.experience}</p> : null}
                        {app.coverLetter ? <p>{t('career.cover')}: {app.coverLetter}</p> : null}
                      </div>
                    </td>
                  </tr>
                ) : null}
              </Fragment>
            ))}
            {!filtered.length ? (
              <tr><td colSpan="6">{t('admin.careerNoApps')}</td></tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </div>
  )
}
