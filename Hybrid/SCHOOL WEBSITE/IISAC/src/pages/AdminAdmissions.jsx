import { useEffect, useMemo, useState } from 'react'
import { api } from '../lib/api.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

const PRINT_KEY = 'sikkhaloy-receipt-print'
const LEGACY_PRINT_KEY = 'receipt-printing'
const PRINT_SIZES = ['3', '3.5', '4', '4.5', '5', '5.5', '6']

function formatTime(value) {
  if (!value) return ''
  const date = new Date(String(value).replace(' ', 'T'))
  if (Number.isNaN(date.getTime())) return String(value)
  return date.toLocaleString()
}

function formatTokenDate(value) {
  if (!value) return ''
  const date = new Date(String(value).replace(' ', 'T'))
  if (Number.isNaN(date.getTime())) return String(value)
  const dd = String(date.getDate()).padStart(2, '0')
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const yyyy = date.getFullYear()
  let hours = date.getHours()
  const minutes = String(date.getMinutes()).padStart(2, '0')
  const ampm = hours >= 12 ? 'PM' : 'AM'
  hours = hours % 12 || 12
  return `${dd}/${mm}/${yyyy} ${hours}:${minutes} ${ampm}`
}

function line(parts) {
  return parts.filter(Boolean).join(' · ')
}

function uniqueAddress(text, maxParts = 5) {
  const parts = String(text || '')
    .split(',')
    .map((part) => part.trim())
    .filter(Boolean)
  const kept = []
  const seen = new Set()
  for (const part of parts) {
    if (/^\d{3,}$/.test(part) && kept.some((item) => item.includes(part))) continue
    const key = part
      .split(/[\s./-]+/)
      .find((word) => word.replace(/[^a-z\u0980-\u09FF]/gi, '').length > 3)
      ?.replace(/[^a-z\u0980-\u09FF]/gi, '')
      .toLowerCase()
    if (key && seen.has(key)) continue
    if (key) seen.add(key)
    kept.push(part)
    if (kept.length >= maxParts) break
  }
  return kept.join(', ')
}

function schoolInfoLine(address, phone, inches = 4) {
  const maxParts = inches <= 3 ? 3 : inches <= 3.5 ? 3 : inches <= 4 ? 4 : 5
  const tel = String(phone || '').replace(/^(phone|ফোন)\s*:\s*/i, '').trim()
  return [uniqueAddress(address, maxParts), tel].filter(Boolean).join(' · ')
}

function fitLinePx(inches, text, minPx, maxPx, charRatio) {
  const usable = Math.max(120, inches * 96 - 14)
  const chars = Math.max(String(text || '').length, 8)
  return Math.max(minPx, Math.min(maxPx, Math.floor(usable / (chars * charRatio))))
}

function matchesSearch(item, query) {
  if (!query) return true
  const hay = [
    item.studentsName,
    item.fathersName,
    item.mothersName,
    item.smsPhone,
    item.fatherPhone,
    item.motherPhone,
    item.className,
    item.educationYear,
    item.rollNo,
    item.status,
    item.assignedId,
    item.permanentAddress,
    item.localAddress,
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase()
  return hay.includes(query)
}

function parsePrintSize(value) {
  if (value === 'half') return 4
  if (value === 'quarter') return 3
  if (value === 'full') return 6
  const inches = Number(value)
  if (!Number.isFinite(inches)) return 4
  return Math.min(6, Math.max(3, inches))
}

function readPrintOptions() {
  let data = {}
  try {
    data = {
      ...JSON.parse(localStorage.getItem(LEGACY_PRINT_KEY) || '{}'),
      ...JSON.parse(localStorage.getItem(PRINT_KEY) || '{}'),
    }
  } catch {
    data = {}
  }
  return {
    size: parsePrintSize(data.size),
    isInstitutionName: Boolean(data.hideIns ?? data.isInstitutionName),
    topSpace: Number(data.topSpace) || 0,
    fontSize: Math.min(20, Math.max(10, Number(data.fontSize) || 11)),
  }
}

function savePrintOptions(opts) {
  const payload = {
    size: String(opts.size),
    topSpace: opts.topSpace,
    fontSize: opts.fontSize,
    hideIns: opts.isInstitutionName,
    isInstitutionName: opts.isInstitutionName,
  }
  localStorage.setItem(PRINT_KEY, JSON.stringify(payload))
  localStorage.setItem(LEGACY_PRINT_KEY, JSON.stringify(payload))
}

function money(value) {
  const n = Number(value || 0)
  return n.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 2 })
}

function num(value) {
  const n = Number(value)
  return Number.isFinite(n) ? n : 0
}

function mapFeeRow(row) {
  const prescribed = num(row.prescribed ?? row.amount ?? row.Amount)
  const discount = num(row.discount ?? row.Discount)
  const payable = row.payable != null && row.payable !== ''
    ? num(row.payable)
    : Math.max(0, prescribed - discount)
  return {
    roleID: Number(row.roleID || row.RoleID || 0),
    role: row.role || row.Role || '',
    prescribed: prescribed || '',
    discount: discount || '',
    payable: payable || '',
  }
}

function emptyFee() {
  return { roleID: 0, role: '', prescribed: '', discount: '', payable: '' }
}

function blankRows() {
  return [emptyFee(), emptyFee(), emptyFee()]
}

export default function AdminAdmissions() {
  const { t } = useLang()
  const { institute, adminProfile } = useApp()
  const [items, setItems] = useState([])
  const [query, setQuery] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [openId, setOpenId] = useState(0)
  const [working, setWorking] = useState(0)
  const [feeRoles, setFeeRoles] = useState([])
  const [payments, setPayments] = useState([])
  const [sendSms, setSendSms] = useState(true)
  const [printOpen, setPrintOpen] = useState(false)
  const [printOptions, setPrintOptions] = useState(readPrintOptions)
  const filtered = items.filter((item) => matchesSearch(item, query.trim().toLowerCase()))
  const openItem = items.find((item) => item.id === openId) || null
  const totalDue = useMemo(
    () => payments.reduce((sum, row) => sum + num(row.payable), 0),
    [payments],
  )

  async function load() {
    setBusy(true)
    setError('')
    try {
      const data = await api.admissions()
      setItems(Array.isArray(data.admissions) ? data.admissions : [])
    } catch (err) {
      setError(err.message || t('admin.admissionsLoadError'))
    } finally {
      setBusy(false)
    }
  }

  useEffect(() => {
    load()
    api.admissionForm()
      .then((data) => setFeeRoles(Array.isArray(data.feeRoles) ? data.feeRoles : []))
      .catch(() => {})
  }, [])

  useEffect(() => {
    if (!openItem) return
    setSendSms(true)
    setPrintOpen(false)
    setPayments(openItem.payments?.length ? openItem.payments.map(mapFeeRow) : blankRows())
  }, [openId])

  function updatePrint(next) {
    const merged = { ...printOptions, ...next }
    setPrintOptions(merged)
    savePrintOptions(merged)
  }

  function changePayment(index, patch) {
    setPayments((prev) =>
      prev.map((row, i) => {
        if (i !== index) return row
        const next = { ...row, ...patch }
        const prescribed = num(next.prescribed)
        const discount = num(next.discount)
        if (Object.prototype.hasOwnProperty.call(patch, 'prescribed')) {
          next.payable = next.prescribed === '' && next.discount === '' ? '' : String(Math.max(0, prescribed - discount))
        } else if (Object.prototype.hasOwnProperty.call(patch, 'discount')) {
          next.payable = next.prescribed === '' && next.discount === '' ? '' : String(Math.max(0, prescribed - discount))
        } else if (Object.prototype.hasOwnProperty.call(patch, 'payable') && next.prescribed !== '') {
          next.discount = String(Math.max(0, prescribed - num(next.payable)))
        }
        return next
      }),
    )
  }

  async function confirm(item) {
    const already = item.status === 'confirmed'
    if (!already && !window.confirm(t('admin.confirmPush', { name: item.studentsName }))) return
    setWorking(item.id)
    setError('')
    setNotice('')
    try {
      const data = await api.confirmAdmission(item.id, {
        sendSms,
        payments: payments
          .filter((row) => Number(row.roleID) > 0 && num(row.prescribed || row.payable) > 0)
          .map((row) => {
            const role = feeRoles.find((item) => Number(item.roleID) === Number(row.roleID))?.role || row.role
            const prescribed = num(row.prescribed || row.payable)
            const payable = num(row.payable || prescribed)
            return {
              roleID: Number(row.roleID),
              role,
              payFor: role,
              amount: prescribed,
              discount: Math.max(0, prescribed - payable),
            }
          }),
      })
      const next = data.admission
      setItems((prev) => prev.map((row) => (row.id === item.id ? next : row)))
      if (next.payments?.length) setPayments(next.payments.map(mapFeeRow))
      const result = data.result || {}
      if (!already) {
        if (result.smsSent || result.SmsSent) {
          setNotice(result.smsLocal || result.SmsLocal ? t('admin.smsLocal') : t('admin.smsSent'))
        } else if (sendSms) {
          setNotice(result.smsError || result.SmsError || t('admin.smsSkipped'))
        }
      }
      setPrintOpen(true)
    } catch (err) {
      setError(err.message || t('admin.admissionsPushError'))
      load()
    } finally {
      setWorking(0)
    }
  }

  async function remove(id) {
    if (!window.confirm(t('admin.deleteAdmissionConfirm'))) return
    try {
      await api.deleteAdmission(id)
      setItems((prev) => prev.filter((item) => item.id !== id))
      if (openId === id) setOpenId(0)
    } catch (err) {
      setError(err.message || t('admin.admissionsLoadError'))
    }
  }

  const canEditFees = !openItem?.payments?.length
  const printInches = parsePrintSize(printOptions.size)
  const printNarrow = printInches <= 3.5
  const printFont = printOptions.fontSize
  const headerLine = schoolInfoLine(institute.address, institute.phone, printInches)
  const schoolPx = fitLinePx(printInches, institute.schoolName, printInches <= 3 ? 9 : 10, printInches <= 4 ? 15 : 20, 0.56)
  const infoPx = fitLinePx(printInches, headerLine, printInches <= 3 ? 6 : 7, printInches <= 4 ? 9 : 11, 0.5)
  const tablePx = printNarrow ? (printInches <= 3 ? 8 : 9) : printFont
  const titlePx = printNarrow ? (printInches <= 3 ? 10 : 11) : 14
  const logoPx = printInches <= 3 ? 28 : printInches <= 3.5 ? 32 : printInches <= 4 ? 40 : 52
  const sideMm = Math.max(4, (210 - printInches * 25.4) / 2)
  const printWidth = `${printInches}in`

  const printStyle = `
    @page { size: A4 portrait; margin: ${printOptions.topSpace}px ${sideMm}mm 8mm ${sideMm}mm !important; }
    @media print {
      html, body { background: #fff !important; }
      body * { visibility: hidden; }
      .adm-print-layer, .adm-print-layer .adm-token, .adm-print-layer .adm-token * { visibility: visible; }
      .adm-print-card, .no-print { display: none !important; visibility: hidden !important; }
      .adm-print-layer {
        position: static !important; inset: auto !important; background: #fff !important;
        padding: 0 !important; overflow: visible !important;
      }
      .adm-token {
        width: ${printWidth} !important; max-width: ${printWidth} !important;
        margin: 0 auto !important; padding: ${printNarrow ? '4px 2px' : '8px'} !important;
        box-shadow: none !important;
      }
      .adm-token-logo {
        width: ${logoPx}px !important; height: ${logoPx}px !important;
        max-height: ${logoPx}px !important; margin: 0 !important; flex-shrink: 0 !important;
      }
      .InsInfo {
        display: flex !important; flex-direction: row !important; align-items: center !important;
        justify-content: center !important; width: 100% !important;
        gap: ${printNarrow ? '4px' : '8px'} !important; margin-bottom: 4px !important;
      }
      .InsInfo > div { flex: 0 1 auto !important; width: auto !important; min-width: 0 !important; }
      #InstitutionName {
        font-size: ${schoolPx}px !important; line-height: 1.15 !important;
        white-space: nowrap !important; letter-spacing: ${printNarrow ? '-0.02em' : '0'} !important;
        text-align: left !important;
      }
      .SInfo, .InsInfo p {
        font-size: ${infoPx}px !important; line-height: 1.2 !important;
        white-space: nowrap !important; margin: 2px 0 0 !important;
      }
      .payment-receipt-header { font-size: ${titlePx}px !important; letter-spacing: ${printNarrow ? '0' : '1px'} !important; }
      .receipt-info-container p {
        font-size: ${infoPx}px !important; white-space: nowrap !important;
        overflow: hidden !important;
      }
      .adm-token-table { width: 100% !important; max-width: 100% !important; table-layout: fixed !important; }
      .adm-token-table th, .adm-token-table td {
        font-size: ${tablePx}px !important; padding: ${printNarrow ? '2px 1px' : '5px 4px'} !important;
        text-align: center !important; overflow-wrap: anywhere !important; word-break: break-word !important;
      }
      .adm-token-note, .RecvBy, .adm-token-powered { font-size: ${infoPx}px !important; }
      .RecvBy { text-align: right !important; margin-top: 10px !important; }
      .adm-token-powered { text-align: center !important; margin-top: 14px !important; }
    }
  `

  return (
    <div className="container admissions-admin">
      <style media="print" id="print-content">{printStyle}</style>
      <p className="lead">{t('admin.admissionsLead')}</p>
      <label className="adm-search">
        <span>{t('admin.searchAdmissions')}</span>
        <input
          type="search"
          value={query}
          placeholder={t('admin.searchAdmissionsPh')}
          onChange={(e) => setQuery(e.target.value)}
        />
      </label>
      {error ? <p className="form-error">{error}</p> : null}
      {notice ? <p className="adm-notice">{notice}</p> : null}
      {busy && items.length === 0 ? <p className="lead">{t('admin.admissionsLoading')}</p> : null}
      {!busy && items.length === 0 ? <p className="lead">{t('admin.noAdmissions')}</p> : null}
      {!busy && items.length > 0 && filtered.length === 0 ? <p className="lead">{t('admin.noAdmissionMatch')}</p> : null}

      <div className="adm-admin-table-wrap">
        <table className="adm-admin-table">
          <thead>
            <tr>
              <th>{t('admin.colDate')}</th>
              <th>{t('admin.colName')}</th>
              <th>{t('admin.colPhone')}</th>
              <th>{t('admin.colClass')}</th>
              <th>{t('admin.colStatus')}</th>
              <th>{t('admin.colActions')}</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((item) => (
              <tr key={item.id} className={item.status === 'confirmed' ? 'is-done' : ''}>
                <td>{formatTime(item.createdAt)}</td>
                <td>
                  <strong>{item.studentsName}</strong>
                  <small>{item.fathersName}</small>
                </td>
                <td>
                  <a href={`tel:${item.smsPhone}`}>{item.smsPhone}</a>
                </td>
                <td>{line([item.className, item.educationYear, item.rollNo && `${t('home.roll')} ${item.rollNo}`])}</td>
                <td>
                  <span className={`adm-status is-${item.status}`}>{t(`admin.status_${item.status}`)}</span>
                  {item.assignedId ? (
                    <small>
                      {t('admin.assignedId')}: {item.assignedId}
                    </small>
                  ) : null}
                  {item.confirmError ? <small className="form-error">{item.confirmError}</small> : null}
                </td>
                <td>
                  <div className="adm-actions">
                    <button type="button" className="btn-outline" onClick={() => setOpenId(item.id)}>
                      {t('admin.viewDetails')}
                    </button>
                    <button type="button" className="btn-del" onClick={() => remove(item.id)}>
                      {t('admin.deleteAdmission')}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {openItem ? (
        <div className="adm-modal-backdrop no-print" onClick={() => setOpenId(0)}>
          <div className="adm-modal" onClick={(e) => e.stopPropagation()}>
            <div className="adm-modal-head">
              <div>
                <h2>{openItem.studentsName}</h2>
                <small>{line([openItem.className, openItem.educationYear, openItem.assignedId])}</small>
              </div>
              <button type="button" className="btn-outline" onClick={() => setOpenId(0)}>
                {t('admin.closeModal')}
              </button>
            </div>

            <div className="adm-modal-grid">
              <div className="adm-detail">
                {openItem.photo ? <img src={openItem.photo} alt="" className="adm-photo" /> : null}
                <p>
                  <strong>{t('admission.studentName')}:</strong> {openItem.studentsName}
                </p>
                <p>
                  <strong>{t('admission.gender')}:</strong> {openItem.gender}
                </p>
                <p>
                  <strong>{t('admission.fatherName')}:</strong> {line([openItem.fathersName, openItem.fatherPhone, openItem.fatherOccupation])}
                </p>
                <p>
                  <strong>{t('admission.motherName')}:</strong> {line([openItem.mothersName, openItem.motherPhone, openItem.motherOccupation])}
                </p>
                <p>
                  <strong>{t('admission.email')}:</strong> {openItem.studentEmail || '—'}
                </p>
                <p>
                  <strong>{t('admission.dob')}:</strong> {openItem.dateOfBirth || '—'}
                </p>
                <p>
                  <strong>{t('admission.permanent')}:</strong> {openItem.permanentAddress || '—'}
                </p>
                <p>
                  <strong>{t('admission.local')}:</strong> {openItem.localAddress || '—'}
                </p>
                <p>
                  <strong>{t('admission.prevSchool')}:</strong>{' '}
                  {line([openItem.prevSchool, openItem.prevClass, openItem.prevExamYear, openItem.prevGrade])}
                </p>
                <p>
                  <strong>{t('admission.class')}:</strong>{' '}
                  {line([openItem.className, openItem.groupName, openItem.sectionName, openItem.shiftName, openItem.rollNo])}
                </p>
                {openItem.subjects?.length ? (
                  <p>
                    <strong>{t('admission.subjects')}:</strong>{' '}
                    {openItem.subjects.map((row) => `${row.subjectName || row.subjectID} (${row.subjectType})`).join(', ')}
                  </p>
                ) : null}
              </div>

              <div className="adm-paybox">
                <h3>{t('admin.paymentSetup')}</h3>
                <p className="lead">{t('admin.paymentSetupHint')}</p>
                <div className="adm-pay-table-wrap">
                  <table className="adm-pay-table">
                    <thead>
                      <tr>
                        <th>{t('admin.feeRole')}</th>
                        <th>{t('admin.prescribed')}</th>
                        <th>{t('admin.concession')}</th>
                        <th>{t('admin.payable')}</th>
                        <th />
                      </tr>
                    </thead>
                    <tbody>
                      {payments.map((row, index) => (
                        <tr key={`${row.roleID}-${index}`}>
                          <td>
                            <select
                              value={row.roleID || ''}
                              disabled={!canEditFees}
                              onChange={(e) => {
                                const roleID = Number(e.target.value || 0)
                                const role = feeRoles.find((item) => Number(item.roleID) === roleID)
                                changePayment(index, { roleID, role: role?.role || '' })
                              }}
                            >
                              <option value="">{t('admin.feeRole')}</option>
                              {feeRoles.map((role) => (
                                <option key={role.roleID} value={role.roleID}>
                                  {role.role}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td>
                            <input
                              type="number"
                              min="0"
                              value={row.prescribed}
                              disabled={!canEditFees}
                              onChange={(e) => changePayment(index, { prescribed: e.target.value })}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min="0"
                              value={row.discount}
                              disabled={!canEditFees}
                              onChange={(e) => changePayment(index, { discount: e.target.value })}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min="0"
                              value={row.payable}
                              disabled={!canEditFees}
                              onChange={(e) => changePayment(index, { payable: e.target.value })}
                            />
                          </td>
                          <td>
                            {canEditFees ? (
                              <button type="button" className="btn-del" onClick={() => setPayments((prev) => prev.filter((_, i) => i !== index))}>
                                {t('admin.removeFee')}
                              </button>
                            ) : null}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {canEditFees ? (
                  <button type="button" className="btn-outline" onClick={() => setPayments((prev) => [...prev, emptyFee()])}>
                    {t('admin.addFee')}
                  </button>
                ) : null}
                <p className="adm-total">
                  <strong>{t('admin.totalDue')}:</strong> {money(totalDue)} TK
                </p>
                {openItem.status !== 'confirmed' ? (
                  <label className="adm-sms">
                    <input type="checkbox" checked={sendSms} onChange={(e) => setSendSms(e.target.checked)} />
                    <span>{t('admin.sendAdmissionSms')}</span>
                  </label>
                ) : null}
                <div className="adm-actions">
                  {openItem.status !== 'confirmed' ? (
                    <button
                      type="button"
                      className="btn-orange"
                      disabled={working === openItem.id}
                      onClick={() => confirm(openItem)}
                    >
                      {working === openItem.id ? t('admin.pushing') : t('admin.confirmPushBtn')}
                    </button>
                  ) : canEditFees ? (
                    <button
                      type="button"
                      className="btn-orange"
                      disabled={working === openItem.id}
                      onClick={() => confirm(openItem)}
                    >
                      {working === openItem.id ? t('admin.pushing') : t('admin.saveFees')}
                    </button>
                  ) : (
                    <button type="button" className="btn-orange" onClick={() => setPrintOpen(true)}>
                      {t('admin.printToken')}
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : null}

      {openItem && (printOpen || openItem.status === 'confirmed') && printOpen ? (
        <div className="adm-print-layer">
          <div className="adm-print-card no-print">
            <h3>{t('admin.printOptions')}</h3>
            <p className="lead">{t('admin.printSizeHint')}</p>
            <label>
              <input
                type="checkbox"
                checked={printOptions.isInstitutionName}
                onChange={(e) => updatePrint({ isInstitutionName: e.target.checked })}
              />
              {t('admin.hideInstitution')}
            </label>
            <div className="adm-print-opts">
              <label>
                {t('admin.printSize')}
                <select
                  value={String(printOptions.size)}
                  onChange={(e) => updatePrint({ size: parsePrintSize(e.target.value) })}
                >
                  {PRINT_SIZES.map((size) => (
                    <option key={size} value={size}>
                      {t('admin.inch', { n: size })}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                {t('admin.topSpace')}
                <input
                  type="number"
                  min="0"
                  value={printOptions.topSpace}
                  onChange={(e) => updatePrint({ topSpace: Number(e.target.value || 0) })}
                />
              </label>
              <label>
                {t('admin.fontSize')}
                <input
                  type="number"
                  min="10"
                  max="20"
                  value={printOptions.fontSize}
                  onChange={(e) => updatePrint({ fontSize: Number(e.target.value || 11) })}
                />
              </label>
            </div>
            <div className="adm-actions">
              <button type="button" className="btn-orange" onClick={() => window.print()}>
                {t('admin.printNow')}
              </button>
              <button type="button" className="btn-outline" onClick={() => setPrintOpen(false)}>
                {t('admin.closeModal')}
              </button>
            </div>
          </div>

          <article
            className={`adm-token ${printNarrow ? 'is-narrow' : ''}`}
            style={{ width: printWidth, maxWidth: printWidth, overflow: 'hidden' }}
          >
            <div className={`InsInfo bg-main ${printOptions.isInstitutionName ? 'hide-inst' : ''}`}>
              {institute.logoUrl ? (
                <img src={institute.logoUrl} alt="" className="adm-token-logo" style={{ width: logoPx, height: logoPx }} />
              ) : null}
              <div>
                <h1 id="InstitutionName" style={{ fontSize: `${schoolPx}px` }}>{institute.schoolName}</h1>
                {headerLine ? <p className="SInfo" style={{ fontSize: `${infoPx}px` }}>{headerLine}</p> : null}
              </div>
            </div>
            <div className="payment-receipt-header">{t('admin.tokenTitle')}</div>
            <div className="receipt-info-container">
              <div className="receipt-info-left">
                <p>
                  <strong>(ID: {openItem.assignedId || '—'})</strong> {openItem.studentsName}
                </p>
                <p>
                  <strong>{t('admission.class')}:</strong> {openItem.className}
                  {openItem.rollNo ? `, ${t('home.roll')}: ${openItem.rollNo}` : ''}
                </p>
                <p>
                  <strong>{t('admin.tokenNo')}:</strong> ADM-{openItem.id}
                </p>
              </div>
              <div className="receipt-info-right">
                <p>
                  <strong>{t('admin.colDate')}:</strong> {formatTokenDate(openItem.confirmedAt || openItem.createdAt)}
                </p>
                <p>
                  <strong>{t('admin.colPhone')}:</strong> {openItem.smsPhone}
                </p>
                <p>
                  <strong>{t('admin.colStatus')}:</strong> {t('admin.status_confirmed')}
                </p>
              </div>
            </div>
            <table className="adm-token-table" style={{ fontSize: `${tablePx}px` }}>
              <colgroup>
                <col style={{ width: '34%' }} />
                <col style={{ width: '22%' }} />
                <col style={{ width: '22%' }} />
                <col style={{ width: '22%' }} />
              </colgroup>
              <thead>
                <tr>
                  <th>{t('admin.feeRole')}</th>
                  <th>{t('admin.prescribed')}</th>
                  <th>{t('admin.concession')}</th>
                  <th>{t('admin.payable')}</th>
                </tr>
              </thead>
              <tbody>
                {payments.filter((row) => row.roleID).map((row, index) => (
                  <tr key={`${row.roleID}-${index}`}>
                    <td>{row.role}</td>
                    <td>{money(row.prescribed)}৳</td>
                    <td>{money(row.discount)}৳</td>
                    <td>{money(row.payable)}৳</td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr>
                  <td colSpan="3">{t('admin.totalDue')}</td>
                  <td>{money(totalDue)}৳</td>
                </tr>
              </tfoot>
            </table>
            <p className="adm-token-note">{t('admin.tokenNote')}</p>
            <p className="RecvBy">
              {t('admin.approvedBy')}: {adminProfile?.name || '—'}
              {adminProfile?.designation ? `, ${adminProfile.designation}` : ''}
            </p>
            <p className="adm-token-powered">({t('admin.poweredBy')} Sikkhaloy.com)</p>
          </article>
        </div>
      ) : null}
    </div>
  )
}
