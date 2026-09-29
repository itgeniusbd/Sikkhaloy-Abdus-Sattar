import { useEffect, useMemo, useRef, useState } from 'react'
import { api } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

const BLOOD = ['Unknown', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-']
const RELIGION = ['Islam', 'Hinduism', 'Buddhism', 'Christianity']

const emptyForm = {
  smsPhone: '',
  educationYearId: '',
  studentsName: '',
  gender: 'Male',
  fathersName: '',
  studentEmail: '',
  dateOfBirth: '',
  legalIdentity: '',
  bloodGroup: 'Unknown',
  religion: 'Islam',
  permanentAddress: '',
  localAddress: '',
  fatherPhone: '',
  fatherOccupation: '',
  mothersName: '',
  motherPhone: '',
  motherOccupation: '',
  guardianName: '',
  guardianRelation: '',
  guardianPhone: '',
  prevSchool: '',
  prevClass: '',
  prevExamYear: '',
  prevGrade: '',
  classId: '',
  groupId: '',
  sectionId: '',
  shiftId: '',
  rollNo: '',
}

function labelOf(list, id) {
  const found = (list || []).find((item) => String(item.id) === String(id))
  return found?.name || ''
}

function validDob(value) {
  const text = String(value || '').trim()
  const slash = /^(\d{1,2})[/\-.](\d{1,2})[/\-.](\d{4})$/.exec(text)
  const iso = /^(\d{4})-(\d{1,2})-(\d{1,2})$/.exec(text)
  let day
  let month
  let year
  if (slash) {
    day = Number(slash[1])
    month = Number(slash[2])
    year = Number(slash[3])
  } else if (iso) {
    year = Number(iso[1])
    month = Number(iso[2])
    day = Number(iso[3])
  } else {
    return false
  }
  const date = new Date(year, month - 1, day)
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day
}

function Field({ label, required, children, className = '' }) {
  return (
    <label className={`adm-field ${className}`}>
      <span className="adm-label">
        {label}
        {required ? <em>*</em> : null}
      </span>
      {children}
    </label>
  )
}

function ReviewItem({ label, value }) {
  const text = String(value || '').trim()
  if (!text || /^\[.+\]$/.test(text)) return null
  return (
    <span className="adm-review-item">
      <em>{label}</em>
      <strong>{text}</strong>
    </span>
  )
}

export default function ApplyOnline() {
  const { t } = useLang()
  const [open, setOpen] = useState({ extra: true, parents: true, prev: false, academic: true })
  const [form, setForm] = useState(emptyForm)
  const [photo, setPhoto] = useState(null)
  const [preview, setPreview] = useState('')
  const [sessions, setSessions] = useState([])
  const [classes, setClasses] = useState([])
  const [lastId, setLastId] = useState('')
  const [lastRoll, setLastRoll] = useState('')
  const [groups, setGroups] = useState([])
  const [sections, setSections] = useState([])
  const [shifts, setShifts] = useState([])
  const [subjects, setSubjects] = useState([])
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [sent, setSent] = useState(false)
  const [reviewOpen, setReviewOpen] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)
  const formTopRef = useRef(null)

  function scrollToFormTop() {
    window.setTimeout(() => {
      const el = formTopRef.current
      const header = document.querySelector('.site-top')
      const offset = header ? header.getBoundingClientRect().height + 8 : 160
      const top = el ? el.getBoundingClientRect().top + window.pageYOffset - offset : 0
      window.scrollTo({ top: Math.max(0, top), behavior: 'smooth' })
    }, 80)
  }

  function editFromReview() {
    setConfirmOpen(false)
    setReviewOpen(false)
    scrollToFormTop()
  }

  function setField(key, value) {
    setForm((prev) => ({ ...prev, [key]: value }))
  }

  useEffect(() => {
    if (sent || error) scrollToFormTop()
  }, [sent, error])

  useEffect(() => {
    let live = true
    api
      .admissionForm()
      .then((data) => {
        if (!live) return
        setSessions(data.sessions || [])
        setClasses(data.classes || [])
        setLastId(data.lastStudentId || '')
        if (data.activeEducationYearId) {
          setForm((prev) => ({ ...prev, educationYearId: String(data.activeEducationYearId) }))
        }
      })
      .catch((err) => {
        if (live) setError(err.message || t('admission.loadError'))
      })
    return () => {
      live = false
    }
  }, [t])

  useEffect(() => {
    if (!form.classId) {
      setGroups([])
      setSections([])
      setShifts([])
      setSubjects([])
      setLastRoll('')
      return
    }
    let live = true
    api
      .admissionOptions({
        classId: form.classId,
        educationYearId: form.educationYearId || 0,
        groupId: form.groupId || 0,
        sectionId: form.sectionId || 0,
        shiftId: form.shiftId || 0,
      })
      .then((data) => {
        if (!live) return
        setGroups(data.groups || [])
        setSections(data.sections || [])
        setShifts(data.shifts || [])
        setSubjects(
          (data.subjects || []).map((row) => ({
            ...row,
            selected: Boolean(row.selected),
            subjectType: row.subjectType || 'Compulsory',
          })),
        )
        setLastRoll(data.lastRollNo || '')
      })
      .catch(() => {
        if (live) {
          setGroups([])
          setSections([])
          setShifts([])
          setSubjects([])
        }
      })
    return () => {
      live = false
    }
  }, [form.classId, form.educationYearId, form.groupId, form.sectionId, form.shiftId])

  const selectedSubjects = useMemo(
    () =>
      subjects
        .filter((row) => row.selected)
        .map((row) => ({ subjectID: row.subjectID, subjectType: row.subjectType })),
    [subjects],
  )

  function validate() {
    if (!/^01[3-9]\d{8}$/.test(form.smsPhone)) {
      setError(t('admission.phoneInvalid'))
      return false
    }
    if (!validDob(form.dateOfBirth)) {
      setError(t('admission.dobInvalid'))
      return false
    }
    if (!form.permanentAddress.trim()) {
      setError(t('admission.addressRequired'))
      return false
    }
    if (!/^01[3-9]\d{8}$/.test(form.fatherPhone) || !/^01[3-9]\d{8}$/.test(form.motherPhone)) {
      setError(t('admission.parentPhoneInvalid'))
      return false
    }
    return true
  }

  function openReview(e) {
    e.preventDefault()
    setError('')
    setSent(false)
    if (!validate()) return
    setReviewOpen(true)
  }

  async function sendApplication() {
    setError('')
    setBusy(true)
    try {
      const body = new FormData()
      Object.entries({
        ...form,
        educationYear: labelOf(sessions, form.educationYearId),
        className: labelOf(classes, form.classId),
        groupName: labelOf(groups, form.groupId),
        sectionName: labelOf(sections, form.sectionId),
        shiftName: labelOf(shifts, form.shiftId),
        subjects: JSON.stringify(selectedSubjects),
      }).forEach(([key, value]) => body.append(key, value == null ? '' : String(value)))
      if (photo) body.append('studentPhoto', photo)
      await api.submitAdmission(body)
      setSent(true)
      setConfirmOpen(false)
      setReviewOpen(false)
      setForm((prev) => ({ ...emptyForm, educationYearId: prev.educationYearId }))
      setPhoto(null)
      setPreview('')
      setSubjects([])
      scrollToFormTop()
    } catch (err) {
      setConfirmOpen(false)
      setReviewOpen(false)
      setError(err.message || t('admission.submitError'))
      scrollToFormTop()
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="page-pad admission-page">
      <div className="container admission-wrap" id="admission-form-top" ref={formTopRef}>
        <header className="admission-hero">
          <h1>{t('admission.title')}</h1>
          <p>{t('admission.lead')}</p>
        </header>

        <div className="admission-steps" aria-hidden="true">
          <div className={reviewOpen || sent ? '' : 'is-active'}>
            <strong>1</strong>
            <span>{t('admission.stepBasic')}</span>
          </div>
          <div>
            <strong>2</strong>
            <span>{t('admission.stepParents')}</span>
          </div>
          <div>
            <strong>3</strong>
            <span>{t('admission.stepAcademic')}</span>
          </div>
          <div className={reviewOpen || sent ? 'is-active' : ''}>
            <strong>4</strong>
            <span>{t('admission.stepDone')}</span>
          </div>
        </div>

        {error ? <p className="form-error">{error}</p> : null}
        {sent ? <p className="admission-ok">{t('admission.submitted')}</p> : null}

        <form className="admission-form" onSubmit={openReview}>
          <section className="adm-card">
            <button type="button" className="adm-head is-required" onClick={() => setOpen((o) => ({ ...o }))}>
              <h2>{t('admission.requiredTitle')}</h2>
              <small>{t('admission.required')}</small>
            </button>
            <div className="adm-body is-open">
              {lastId ? (
                <p className="adm-badge">
                  {t('admission.lastId')}: <strong>{lastId}</strong>
                </p>
              ) : null}
              <p className="adm-note">{t('admission.idNote')}</p>
              <div className="adm-grid">
                <Field label={t('admission.smsPhone')} required>
                  <input
                    required
                    inputMode="numeric"
                    maxLength={11}
                    placeholder="01XXXXXXXXX"
                    value={form.smsPhone}
                    onChange={(e) => setField('smsPhone', e.target.value.replace(/\D/g, '').slice(0, 11))}
                  />
                </Field>
                <Field label={t('admission.session')} required>
                  <select required value={form.educationYearId} onChange={(e) => setField('educationYearId', e.target.value)}>
                    <option value="">{t('admission.selectSession')}</option>
                    {sessions.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.name}
                      </option>
                    ))}
                  </select>
                </Field>
                <Field label={t('admission.studentName')} required className="is-wide">
                  <input required value={form.studentsName} onChange={(e) => setField('studentsName', e.target.value)} />
                </Field>
                <Field label={t('admission.gender')} required className="is-wide">
                  <div className="adm-radios">
                    {['Male', 'Female'].map((value) => (
                      <label key={value}>
                        <input
                          type="radio"
                          name="gender"
                          checked={form.gender === value}
                          onChange={() => setField('gender', value)}
                        />
                        {t(`admission.${value.toLowerCase()}`)}
                      </label>
                    ))}
                  </div>
                </Field>
                <Field label={t('admission.fatherName')} required className="is-wide">
                  <input required value={form.fathersName} onChange={(e) => setField('fathersName', e.target.value)} />
                </Field>
              </div>
            </div>
          </section>

          <section className="adm-card">
            <button
              type="button"
              className={`adm-head is-required ${open.extra ? '' : 'is-collapsed'}`}
              onClick={() => setOpen((o) => ({ ...o, extra: !o.extra }))}
            >
              <h2>{t('admission.extraTitle')}</h2>
              <small>{t('admission.required')}</small>
            </button>
            {open.extra ? (
              <div className="adm-body is-open">
                <div className="adm-grid">
                  <Field label={t('admission.email')}>
                    <input type="email" value={form.studentEmail} onChange={(e) => setField('studentEmail', e.target.value)} />
                  </Field>
                  <Field label={t('admission.dob')} required>
                    <input
                      required
                      placeholder="dd/mm/yyyy"
                      value={form.dateOfBirth}
                      onChange={(e) => setField('dateOfBirth', e.target.value)}
                    />
                  </Field>
                  <Field label={t('admission.nid')}>
                    <input value={form.legalIdentity} onChange={(e) => setField('legalIdentity', e.target.value)} />
                  </Field>
                  <Field label={t('admission.blood')}>
                    <select value={form.bloodGroup} onChange={(e) => setField('bloodGroup', e.target.value)}>
                      {BLOOD.map((item) => (
                        <option key={item} value={item}>
                          {item === 'Unknown' ? t('admission.select') : item}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label={t('admission.religion')}>
                    <select value={form.religion} onChange={(e) => setField('religion', e.target.value)}>
                      {RELIGION.map((item) => (
                        <option key={item} value={item}>
                          {item}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label={t('admission.permanent')} required className="is-half">
                    <textarea required rows="3" value={form.permanentAddress} onChange={(e) => setField('permanentAddress', e.target.value)} />
                  </Field>
                  <Field label={t('admission.local')} className="is-half">
                    <textarea rows="3" value={form.localAddress} onChange={(e) => setField('localAddress', e.target.value)} />
                    <button
                      type="button"
                      className="adm-link"
                      onClick={() => setField('localAddress', form.permanentAddress)}
                    >
                      {t('admission.sameAddress')}
                    </button>
                  </Field>
                  <Field label={t('admission.photo')} className="is-wide">
                    <input
                      type="file"
                      accept=".png,.jpg,.jpeg"
                      onChange={(e) => {
                        const file = e.target.files?.[0] || null
                        setPhoto(file)
                        setPreview(file ? URL.createObjectURL(file) : '')
                      }}
                    />
                    {preview ? <img src={preview} alt="" className="adm-photo" /> : null}
                    <small className="adm-hint">{t('admission.photoHint')}</small>
                  </Field>
                </div>
              </div>
            ) : null}
          </section>

          <section className="adm-card">
            <button
              type="button"
              className={`adm-head is-required ${open.parents ? '' : 'is-collapsed'}`}
              onClick={() => setOpen((o) => ({ ...o, parents: !o.parents }))}
            >
              <h2>{t('admission.parentsTitle')}</h2>
              <small>{t('admission.required')}</small>
            </button>
            {open.parents ? (
              <div className="adm-body is-open">
                <h3>{t('admission.fatherInfo')}</h3>
                <div className="adm-grid">
                  <Field label={t('admission.fatherPhone')} required>
                    <input
                      required
                      inputMode="numeric"
                      maxLength={11}
                      placeholder="01XXXXXXXXX"
                      value={form.fatherPhone}
                      onChange={(e) => setField('fatherPhone', e.target.value.replace(/\D/g, '').slice(0, 11))}
                    />
                  </Field>
                  <Field label={t('admission.fatherJob')}>
                    <input value={form.fatherOccupation} onChange={(e) => setField('fatherOccupation', e.target.value)} />
                  </Field>
                </div>
                <h3>{t('admission.motherInfo')}</h3>
                <div className="adm-grid">
                  <Field label={t('admission.motherName')}>
                    <input value={form.mothersName} onChange={(e) => setField('mothersName', e.target.value)} />
                  </Field>
                  <Field label={t('admission.motherPhone')} required>
                    <input
                      required
                      inputMode="numeric"
                      maxLength={11}
                      placeholder="01XXXXXXXXX"
                      value={form.motherPhone}
                      onChange={(e) => setField('motherPhone', e.target.value.replace(/\D/g, '').slice(0, 11))}
                    />
                  </Field>
                  <Field label={t('admission.motherJob')}>
                    <input value={form.motherOccupation} onChange={(e) => setField('motherOccupation', e.target.value)} />
                  </Field>
                </div>
                <h3>{t('admission.guardianInfo')}</h3>
                <div className="adm-grid">
                  <Field label={t('admission.guardianName')}>
                    <input value={form.guardianName} onChange={(e) => setField('guardianName', e.target.value)} />
                  </Field>
                  <Field label={t('admission.guardianRelation')}>
                    <input value={form.guardianRelation} onChange={(e) => setField('guardianRelation', e.target.value)} />
                  </Field>
                  <Field label={t('admission.guardianPhone')}>
                    <input
                      inputMode="numeric"
                      maxLength={11}
                      placeholder="01XXXXXXXXX"
                      value={form.guardianPhone}
                      onChange={(e) => setField('guardianPhone', e.target.value.replace(/\D/g, '').slice(0, 11))}
                    />
                  </Field>
                </div>
              </div>
            ) : null}
          </section>

          <section className="adm-card">
            <button
              type="button"
              className={`adm-head is-optional ${open.prev ? '' : 'is-collapsed'}`}
              onClick={() => setOpen((o) => ({ ...o, prev: !o.prev }))}
            >
              <h2>{t('admission.prevTitle')}</h2>
              <small>{t('admission.optional')}</small>
            </button>
            {open.prev ? (
              <div className="adm-body is-open">
                <div className="adm-grid">
                  <Field label={t('admission.prevSchool')} className="is-wide">
                    <input value={form.prevSchool} onChange={(e) => setField('prevSchool', e.target.value)} />
                  </Field>
                  <Field label={t('admission.prevClass')}>
                    <input value={form.prevClass} onChange={(e) => setField('prevClass', e.target.value)} />
                  </Field>
                  <Field label={t('admission.prevYear')}>
                    <input
                      inputMode="numeric"
                      value={form.prevExamYear}
                      onChange={(e) => setField('prevExamYear', e.target.value.replace(/\D/g, '').slice(0, 4))}
                    />
                  </Field>
                  <Field label={t('admission.prevGrade')}>
                    <input value={form.prevGrade} onChange={(e) => setField('prevGrade', e.target.value)} />
                  </Field>
                </div>
              </div>
            ) : null}
          </section>

          <section className="adm-card">
            <button
              type="button"
              className={`adm-head is-required ${open.academic ? '' : 'is-collapsed'}`}
              onClick={() => setOpen((o) => ({ ...o, academic: !o.academic }))}
            >
              <h2>{t('admission.academicTitle')}</h2>
              <small>{t('admission.required')}</small>
            </button>
            {open.academic ? (
              <div className="adm-body is-open">
                {lastRoll ? (
                  <p className="adm-badge">
                    {t('admission.lastRoll')}: <strong>{lastRoll}</strong>
                  </p>
                ) : null}
                <div className="adm-grid">
                  <Field label={t('admission.class')} required>
                    <select
                      required
                      value={form.classId}
                      onChange={(e) =>
                        setForm((prev) => ({
                          ...prev,
                          classId: e.target.value,
                          groupId: '',
                          sectionId: '',
                          shiftId: '',
                        }))
                      }
                    >
                      <option value="">{t('admission.selectClass')}</option>
                      {classes.map((item) => (
                        <option key={item.id} value={item.id}>
                          {item.name}
                        </option>
                      ))}
                    </select>
                  </Field>
                  {groups.length ? (
                    <Field label={t('admission.group')}>
                      <select value={form.groupId} onChange={(e) => setField('groupId', e.target.value)}>
                        <option value="">{t('admission.select')}</option>
                        {groups.map((item) => (
                          <option key={item.id} value={item.id}>
                            {item.name}
                          </option>
                        ))}
                      </select>
                    </Field>
                  ) : null}
                  {sections.length ? (
                    <Field label={t('admission.section')}>
                      <select value={form.sectionId} onChange={(e) => setField('sectionId', e.target.value)}>
                        <option value="">{t('admission.select')}</option>
                        {sections.map((item) => (
                          <option key={item.id} value={item.id}>
                            {item.name}
                          </option>
                        ))}
                      </select>
                    </Field>
                  ) : null}
                  {shifts.length ? (
                    <Field label={t('admission.shift')}>
                      <select value={form.shiftId} onChange={(e) => setField('shiftId', e.target.value)}>
                        <option value="">{t('admission.select')}</option>
                        {shifts.map((item) => (
                          <option key={item.id} value={item.id}>
                            {item.name}
                          </option>
                        ))}
                      </select>
                    </Field>
                  ) : null}
                  <Field label={t('admission.roll')}>
                    <input value={form.rollNo} onChange={(e) => setField('rollNo', e.target.value)} />
                  </Field>
                </div>
                {subjects.length ? (
                  <>
                    <h3>{t('admission.subjects')}</h3>
                    <div className="adm-table-wrap">
                      <table className="adm-table">
                        <thead>
                          <tr>
                            <th>{t('admission.subjectName')}</th>
                            <th>{t('admission.selectCol')}</th>
                            <th>{t('admission.subjectType')}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {subjects.map((row, i) => (
                            <tr key={row.subjectID}>
                              <td>{row.subjectName}</td>
                              <td>
                                <input
                                  type="checkbox"
                                  checked={row.selected}
                                  onChange={(e) => {
                                    const next = [...subjects]
                                    next[i] = { ...row, selected: e.target.checked }
                                    setSubjects(next)
                                  }}
                                />
                              </td>
                              <td>
                                <div className="adm-radios">
                                  {['Compulsory', 'Optional'].map((type) => (
                                    <label key={type}>
                                      <input
                                        type="radio"
                                        name={`sub-${row.subjectID}`}
                                        checked={row.subjectType === type}
                                        onChange={() => {
                                          const next = [...subjects]
                                          next[i] = { ...row, subjectType: type }
                                          setSubjects(next)
                                        }}
                                      />
                                      {t(`admission.${type.toLowerCase()}`)}
                                    </label>
                                  ))}
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </>
                ) : null}
              </div>
            ) : null}
          </section>

          <button className="btn-orange admission-submit" type="submit" disabled={busy}>
            {t('admission.review')}
          </button>
        </form>
      </div>

      {reviewOpen ? (
        <div className="adm-modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="admission-review-title">
          <div className="adm-modal adm-review-modal">
            <div className="adm-modal-head">
              <div>
                <h2 id="admission-review-title">{t('admission.reviewTitle')}</h2>
                <p className="adm-review-lead">{t('admission.reviewLead')}</p>
              </div>
              <button type="button" className="btn-ghost" onClick={editFromReview}>
                {t('admission.edit')}
              </button>
            </div>

            <div className="adm-review-hero">
              {preview ? <img src={preview} alt="" className="adm-review-photo" /> : null}
              <div>
                <strong>{form.studentsName}</strong>
                <div className="adm-review-line">
                  <ReviewItem label={t('admission.gender')} value={t(`admission.${form.gender.toLowerCase()}`)} />
                  <ReviewItem label={t('admission.shortPhone')} value={form.smsPhone} />
                  <ReviewItem label={t('admission.session')} value={labelOf(sessions, form.educationYearId)} />
                </div>
              </div>
            </div>

            <div className="adm-review-cols">
              <section className="adm-review-card is-student">
                <h3>{t('admission.reviewStudent')}</h3>
                <div className="adm-review-line">
                  <ReviewItem label={t('admission.fatherName')} value={form.fathersName} />
                  <ReviewItem label={t('admission.shortDob')} value={form.dateOfBirth} />
                  <ReviewItem label={t('admission.shortBlood')} value={form.bloodGroup === 'Unknown' ? '' : form.bloodGroup} />
                  <ReviewItem label={t('admission.shortReligion')} value={form.religion} />
                  <ReviewItem label={t('admission.shortEmail')} value={form.studentEmail} />
                  <ReviewItem label={t('admission.shortNid')} value={form.legalIdentity} />
                  <ReviewItem label={t('admission.shortAddr')} value={form.permanentAddress} />
                  <ReviewItem label={t('admission.shortLocal')} value={form.localAddress} />
                </div>
              </section>

              <section className="adm-review-card is-parents">
                <h3>{t('admission.reviewParents')}</h3>
                <div className="adm-review-line">
                  <ReviewItem
                    label={t('admission.shortFather')}
                    value={[form.fathersName, form.fatherPhone, form.fatherOccupation].filter(Boolean).join(' · ')}
                  />
                  <ReviewItem
                    label={t('admission.shortMother')}
                    value={[form.mothersName, form.motherPhone, form.motherOccupation].filter(Boolean).join(' · ')}
                  />
                  <ReviewItem
                    label={t('admission.shortGuardian')}
                    value={[form.guardianName, form.guardianRelation, form.guardianPhone].filter(Boolean).join(' · ')}
                  />
                </div>
              </section>

              <section className="adm-review-card is-academic">
                <h3>{t('admission.reviewAcademic')}</h3>
                <div className="adm-review-line">
                  <ReviewItem label={t('admission.class')} value={labelOf(classes, form.classId)} />
                  <ReviewItem label={t('admission.group')} value={labelOf(groups, form.groupId)} />
                  <ReviewItem label={t('admission.section')} value={labelOf(sections, form.sectionId)} />
                  <ReviewItem label={t('admission.shift')} value={labelOf(shifts, form.shiftId)} />
                  <ReviewItem label={t('admission.roll')} value={form.rollNo} />
                  <ReviewItem label={t('admission.prevSchool')} value={form.prevSchool} />
                  <ReviewItem label={t('admission.prevClass')} value={form.prevClass} />
                  <ReviewItem label={t('admission.prevYear')} value={form.prevExamYear} />
                  <ReviewItem label={t('admission.prevGrade')} value={form.prevGrade} />
                </div>
                {subjects.some((row) => row.selected) ? (
                  <div className="adm-review-chips">
                    {subjects
                      .filter((row) => row.selected)
                      .map((row) => (
                        <span key={row.subjectID} className="adm-review-chip">
                          {row.subjectName}
                        </span>
                      ))}
                  </div>
                ) : null}
              </section>
            </div>

            <div className="adm-review-actions">
              <button type="button" className="btn-ghost" onClick={editFromReview}>
                {t('admission.edit')}
              </button>
              <button type="button" className="btn-orange" onClick={() => setConfirmOpen(true)}>
                {t('admission.submit')}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {confirmOpen ? (
        <div className="adm-modal-backdrop" style={{ zIndex: 50 }} role="dialog" aria-modal="true" aria-labelledby="admission-confirm-title">
          <div className="adm-confirm-modal">
            <h2 id="admission-confirm-title">{t('admission.confirmTitle')}</h2>
            <p>{t('admission.confirmLead')}</p>
            <div className="adm-confirm-actions">
              <button type="button" className="btn-ghost" disabled={busy} onClick={() => setConfirmOpen(false)}>
                {t('admission.confirmNo')}
              </button>
              <button type="button" className="btn-orange" disabled={busy} onClick={sendApplication}>
                {busy ? t('admission.sending') : t('admission.confirmYes')}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </section>
  )
}
