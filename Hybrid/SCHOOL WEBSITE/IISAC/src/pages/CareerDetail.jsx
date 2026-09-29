import { useEffect, useState } from 'react'
import { createPortal } from 'react-dom'
import { Link, useParams } from 'react-router-dom'
import { api, mediaUrl } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'
import { IconClose } from '../components/Icons.jsx'

const empty = { name: '', phone: '', email: '', address: '', education: '', experience: '', coverLetter: '' }

export default function CareerDetail() {
  const { id } = useParams()
  const { t, lang } = useLang()
  const [job, setJob] = useState(null)
  const [form, setForm] = useState(empty)
  const [cv, setCv] = useState(null)
  const [open, setOpen] = useState(false)
  const [busy, setBusy] = useState(false)
  const [sent, setSent] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setSent(false)
    setForm(empty)
    setCv(null)
    setOpen(false)
    setError('')
    api
      .job(id)
      .then((data) => setJob(data.job || null))
      .catch((err) => setError(err.message || t('career.loadError')))
  }, [id, t])

  useEffect(() => {
    if (!open) return undefined
    const prev = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    function onKey(e) {
      if (e.key === 'Escape') setOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => {
      document.body.style.overflow = prev
      window.removeEventListener('keydown', onKey)
    }
  }, [open])

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    try {
      const body = new FormData()
      Object.entries(form).forEach(([key, value]) => body.append(key, value))
      if (cv) body.append('jobCv', cv)
      await api.applyJob(id, body)
      setSent(true)
      setForm(empty)
      setCv(null)
    } catch (err) {
      setError(err.message || t('career.applyError'))
    } finally {
      setBusy(false)
    }
  }

  if (!job && !error) {
    return (
      <section className="page-pad">
        <div className="container">
          <p className="lead">{t('career.loading')}</p>
        </div>
      </section>
    )
  }
  if (!job) {
    return (
      <section className="page-pad">
        <div className="container">
          <p className="form-error">{error}</p>
          <Link to="/career">{t('career.back')}</Link>
        </div>
      </section>
    )
  }

  const title = lang === 'bn' ? job.titleBn || job.title : job.title
  const description = lang === 'bn' ? job.descriptionBn || job.description : job.description
  const cover = job.image ? mediaUrl(job.image) : ''
  const facts = [
    job.department ? { key: 'department', label: t('career.department'), value: job.department } : null,
    job.location ? { key: 'location', label: t('career.location'), value: job.location } : null,
    job.jobType ? { key: 'type', label: t('career.jobType'), value: job.jobType } : null,
    job.deadline ? { key: 'deadline', label: t('career.deadline'), value: job.deadline } : null,
  ].filter(Boolean)

  const modal = open
    ? createPortal(
        <div
          className="career-apply-overlay"
          role="presentation"
          onClick={(e) => {
            if (e.target === e.currentTarget) setOpen(false)
          }}
        >
          <div className="career-apply-modal" role="dialog" aria-modal="true" aria-labelledby="career-apply-title">
            <div className="career-apply-head">
              <div>
                <strong id="career-apply-title">{t('career.applyTitle')}</strong>
                <small>{title}</small>
              </div>
              <button type="button" className="career-apply-close" onClick={() => setOpen(false)} aria-label="Close">
                <IconClose />
              </button>
            </div>
            {sent ? <p className="admission-ok">{t('career.applyOk')}</p> : null}
            {error ? <p className="form-error">{error}</p> : null}
            {sent ? (
              <button type="button" className="btn-orange" onClick={() => setOpen(false)}>
                {t('career.back')}
              </button>
            ) : (
              <form className="quote-form career-apply-form" onSubmit={submit}>
                <label>
                  {t('career.name')}
                  <span>
                    <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
                  </span>
                </label>
                <label>
                  {t('career.phone')}
                  <span>
                    <input required value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
                  </span>
                </label>
                <label>
                  {t('career.email')}
                  <span>
                    <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
                  </span>
                </label>
                <label>
                  {t('career.address')}
                  <span>
                    <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
                  </span>
                </label>
                <label>
                  {t('career.education')}
                  <span>
                    <textarea rows="2" value={form.education} onChange={(e) => setForm({ ...form, education: e.target.value })} />
                  </span>
                </label>
                <label>
                  {t('career.experience')}
                  <span>
                    <textarea rows="2" value={form.experience} onChange={(e) => setForm({ ...form, experience: e.target.value })} />
                  </span>
                </label>
                <label className="full">
                  {t('career.cover')}
                  <span>
                    <textarea rows="2" value={form.coverLetter} onChange={(e) => setForm({ ...form, coverLetter: e.target.value })} />
                  </span>
                </label>
                <label className="full">
                  {t('career.cv')}
                  <input type="file" accept=".pdf,.doc,.docx,application/pdf" required onChange={(e) => setCv(e.target.files?.[0] || null)} />
                  <small className="adm-hint">{t('career.cvHint')}</small>
                </label>
                <button className="btn-orange" type="submit" disabled={busy}>
                  {busy ? t('career.sending') : t('career.apply')}
                </button>
              </form>
            )}
          </div>
        </div>,
        document.body,
      )
    : null

  return (
    <section className="page-pad">
      <div className="container career-detail">
        {cover ? <img className="career-hero" src={cover} alt="" /> : null}
        <p className="eyebrow">{t('career.eyebrow')}</p>
        <div className="career-detail-head">
          <h1 className="page-title">{title}</h1>
          <button type="button" className="career-apply-now" onClick={() => setOpen(true)}>
            {t('career.applyNow')}
          </button>
        </div>
        {facts.length ? (
          <ul className="career-facts">
            {facts.map((fact) => (
              <li key={fact.key} className="career-fact">
                <small>{fact.label}</small>
                <strong>{fact.value}</strong>
              </li>
            ))}
          </ul>
        ) : null}
        {description ? <div className="career-body">{description}</div> : null}
        <p>
          <Link to="/career">{t('career.back')}</Link>
        </p>
        {modal}
      </div>
    </section>
  )
}
