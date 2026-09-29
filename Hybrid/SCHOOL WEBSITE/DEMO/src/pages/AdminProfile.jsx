import { useEffect, useState } from 'react'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

const emptyPass = { currentPassword: '', newPassword: '', confirmPassword: '' }

export default function AdminProfile() {
  const { adminProfile, saveProfile } = useApp()
  const { t } = useLang()
  const [form, setForm] = useState({ name: '', address: '', photo: '', ...emptyPass })
  const [file, setFile] = useState(null)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setForm((prev) => ({
      ...prev,
      name: adminProfile?.name || '',
      address: adminProfile?.address || '',
      photo: adminProfile?.photo || '',
    }))
  }, [adminProfile])

  const preview = file ? URL.createObjectURL(file) : form.photo

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved(false)
    try {
      await saveProfile(form, file)
      setFile(null)
      setForm((prev) => ({ ...prev, ...emptyPass }))
      setSaved(true)
    } catch (err) {
      setError(err.message || t('admin.profileError'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="container quote-form admin-form" onSubmit={submit}>
      {preview ? (
        <div className="admin-avatar admin-avatar-lg">
          <span className="admin-avatar-orb is-left" aria-hidden="true" />
          <span className="admin-avatar-orb is-right" aria-hidden="true" />
          <span className="admin-avatar-ring" aria-hidden="true" />
          <img src={preview} alt="" />
        </div>
      ) : null}
      <label className="full">
        {t('admin.profilePhoto')}
        <input type="file" accept="image/*" onChange={(e) => setFile(e.target.files?.[0] || null)} />
      </label>
      <label className="full">
        {t('admin.profileName')}
        <span>
          <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </span>
      </label>
      <label className="full">
        {t('admin.profileAddress')}
        <span>
          <textarea rows="3" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
        </span>
      </label>
      <p className="lead full">{t('admin.passwordLead')}</p>
      <label className="full">
        {t('admin.currentPassword')}
        <span>
          <input type="password" value={form.currentPassword} onChange={(e) => setForm({ ...form, currentPassword: e.target.value })} autoComplete="current-password" />
        </span>
      </label>
      <label>
        {t('admin.newPassword')}
        <span>
          <input type="password" value={form.newPassword} onChange={(e) => setForm({ ...form, newPassword: e.target.value })} autoComplete="new-password" />
        </span>
      </label>
      <label>
        {t('admin.confirmPassword')}
        <span>
          <input type="password" value={form.confirmPassword} onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })} autoComplete="new-password" />
        </span>
      </label>
      {error && <p className="form-error">{error}</p>}
      <button className="btn-orange full" type="submit" disabled={busy}>
        {busy ? t('admin.profileSaving') : saved ? t('admin.profileSaved') : t('admin.profileSave')}
      </button>
    </form>
  )
}
