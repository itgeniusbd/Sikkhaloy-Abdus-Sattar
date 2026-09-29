import { useEffect, useState } from 'react'
import { api, mediaUrl } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

const emptyJob = {
  title: '',
  titleBn: '',
  department: '',
  location: '',
  jobType: 'Full-time',
  deadline: '',
  description: '',
  descriptionBn: '',
  image: '',
  published: true,
}

export default function AdminCareer() {
  const { t } = useLang()
  const [jobs, setJobs] = useState([])
  const [form, setForm] = useState(emptyJob)
  const [editingId, setEditingId] = useState(0)
  const [modalOpen, setModalOpen] = useState(false)
  const [coverFile, setCoverFile] = useState(null)
  const [query, setQuery] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  async function loadJobs() {
    const data = await api.manageJobs()
    setJobs(data.jobs || [])
  }

  useEffect(() => {
    loadJobs().catch((err) => setError(err.message || t('admin.careerError')))
  }, [t])

  function openAdd() {
    setEditingId(0)
    setForm(emptyJob)
    setCoverFile(null)
    setError('')
    setNotice('')
    setModalOpen(true)
  }

  function startEdit(job) {
    setEditingId(job.id)
    setForm({
      title: job.title || '',
      titleBn: job.titleBn || '',
      department: job.department || '',
      location: job.location || '',
      jobType: job.jobType || 'Full-time',
      deadline: job.deadline || '',
      description: job.description || '',
      descriptionBn: job.descriptionBn || '',
      image: job.image || '',
      published: job.published !== false,
    })
    setCoverFile(null)
    setError('')
    setNotice('')
    setModalOpen(true)
  }

  function closeModal() {
    setModalOpen(false)
    setEditingId(0)
    setForm(emptyJob)
    setCoverFile(null)
  }

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setNotice('')
    try {
      const body = new FormData()
      body.append('title', form.title)
      body.append('titleBn', form.titleBn)
      body.append('department', form.department)
      body.append('location', form.location)
      body.append('jobType', form.jobType)
      body.append('deadline', form.deadline)
      body.append('description', form.description)
      body.append('descriptionBn', form.descriptionBn)
      body.append('published', form.published ? '1' : '0')
      if (editingId) body.append('id', String(editingId))
      if (coverFile) body.append('jobCover', coverFile)
      await api.saveJob(body)
      setNotice(t('admin.careerSaved'))
      closeModal()
      await loadJobs()
    } catch (err) {
      setError(err.message || t('admin.careerError'))
    } finally {
      setBusy(false)
    }
  }

  async function remove(job) {
    if (!window.confirm(t('admin.careerDeleteConfirm'))) return
    setError('')
    try {
      await api.deleteJob(job.id)
      if (editingId === job.id) closeModal()
      await loadJobs()
    } catch (err) {
      setError(err.message || t('admin.careerError'))
    }
  }

  const filtered = jobs.filter((job) => {
    const q = query.trim().toLowerCase()
    if (!q) return true
    return [job.title, job.titleBn, job.department, job.location, job.jobType].some((value) =>
      String(value || '').toLowerCase().includes(q),
    )
  })

  return (
    <div className="container admin-career">
      <p className="lead">{t('admin.careerLead')}</p>
      {!modalOpen && error ? <p className="form-error">{error}</p> : null}
      {notice ? <p className="admission-ok">{notice}</p> : null}

      <div className="career-toolbar">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={t('admin.careerSearchJobs')}
        />
        <button type="button" className="btn-orange" onClick={openAdd}>
          {t('admin.careerAdd')}
        </button>
      </div>

      <div className="adm-admin-table-wrap">
        <table className="adm-admin-table">
          <thead>
            <tr>
              <th>{t('admin.careerTitle')}</th>
              <th>{t('admin.careerDept')}</th>
              <th>{t('admin.careerLocation')}</th>
              <th>{t('career.deadline')}</th>
              <th>{t('admin.colStatus')}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {filtered.map((job) => (
              <tr key={job.id}>
                <td>
                  <strong>{job.title}</strong>
                  {job.titleBn ? <small>{job.titleBn}</small> : null}
                </td>
                <td>{job.department || '—'}</td>
                <td>{job.location || '—'}</td>
                <td>{job.deadline || '—'}</td>
                <td>
                  <span className={`adm-status ${job.published ? 'is-confirmed' : 'is-pending'}`}>
                    {job.published ? t('admin.careerPublished') : t('admin.careerDraft')}
                  </span>
                </td>
                <td className="career-app-actions">
                  <button type="button" className="btn-ghost" onClick={() => startEdit(job)}>
                    {t('admission.edit')}
                  </button>
                  <button type="button" className="btn-del" onClick={() => remove(job)}>
                    {t('admin.deleteMenu')}
                  </button>
                </td>
              </tr>
            ))}
            {!filtered.length ? (
              <tr>
                <td colSpan="6">{t('admin.careerNoPosts')}</td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>

      {modalOpen ? (
        <div className="adm-modal-backdrop" onClick={closeModal} role="dialog" aria-modal="true">
          <div className="adm-modal career-job-modal" onClick={(e) => e.stopPropagation()}>
            <div className="adm-modal-head">
              <h2>{editingId ? t('admin.careerEdit') : t('admin.careerAdd')}</h2>
              <button type="button" className="btn-outline" onClick={closeModal}>
                {t('admin.closeModal')}
              </button>
            </div>
            {error ? <p className="form-error">{error}</p> : null}
            <form className="quote-form admin-form" onSubmit={submit}>
              <label>
                {t('admin.careerTitle')}
                <span>
                  <input required value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
                </span>
              </label>
              <label>
                {t('admin.careerTitleBn')}
                <span>
                  <input value={form.titleBn} onChange={(e) => setForm({ ...form, titleBn: e.target.value })} />
                </span>
              </label>
              <label>
                {t('admin.careerDept')}
                <span>
                  <input value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} />
                </span>
              </label>
              <label>
                {t('admin.careerLocation')}
                <span>
                  <input value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
                </span>
              </label>
              <label>
                {t('admin.careerType')}
                <select value={form.jobType} onChange={(e) => setForm({ ...form, jobType: e.target.value })}>
                  <option>Full-time</option>
                  <option>Part-time</option>
                  <option>Contract</option>
                </select>
              </label>
              <label>
                {t('career.deadline')}
                <span>
                  <input type="date" value={form.deadline} onChange={(e) => setForm({ ...form, deadline: e.target.value })} />
                </span>
              </label>
              <label className="full">
                {t('admin.careerCover')}
                {(coverFile || form.image) ? (
                  <img
                    className="career-cover-preview"
                    src={coverFile ? URL.createObjectURL(coverFile) : mediaUrl(form.image)}
                    alt=""
                  />
                ) : null}
                <input
                  type="file"
                  accept="image/*"
                  onChange={(e) => setCoverFile(e.target.files?.[0] || null)}
                />
                <small className="adm-hint">{t('admin.careerCoverHint')}</small>
              </label>
              <label className="full">
                {t('admin.careerBody')}
                <span>
                  <textarea rows="4" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
                </span>
              </label>
              <label className="full">
                {t('admin.careerBodyBn')}
                <span>
                  <textarea rows="4" value={form.descriptionBn} onChange={(e) => setForm({ ...form, descriptionBn: e.target.value })} />
                </span>
              </label>
              <label className="admin-user-active">
                <input
                  type="checkbox"
                  checked={form.published}
                  onChange={(e) => setForm({ ...form, published: e.target.checked })}
                />
                {t('admin.careerPublished')}
              </label>
              <div className="admin-user-actions">
                <button className="btn-orange" type="submit" disabled={busy}>
                  {busy ? t('admin.profileSaving') : t('admin.careerSave')}
                </button>
                <button type="button" className="btn-ghost" onClick={closeModal}>
                  {t('admin.usersCancel')}
                </button>
              </div>
            </form>
          </div>
        </div>
      ) : null}
    </div>
  )
}
