import { useEffect, useState } from 'react'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { SOCIAL_NETWORKS, socialsFromSettings } from '../lib/socials.js'

export default function AdminSocial() {
  const { settings, saveSocials } = useApp()
  const { t } = useLang()
  const [form, setForm] = useState(socialsFromSettings(settings))
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setForm(socialsFromSettings(settings))
  }, [settings])

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved(false)
    try {
      await saveSocials(form)
      setSaved(true)
    } catch (err) {
      setError(err.message || t('admin.socialsError'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="container quote-form admin-form" onSubmit={submit}>
      <p className="lead full">{t('admin.socialsLead')}</p>
      {SOCIAL_NETWORKS.map((item) => (
        <label key={item.id} className="full">
          {t(item.labelKey)}
          <span>
            <input
              type="text"
              inputMode="url"
              placeholder={item.placeholder}
              value={form[item.id] || ''}
              onChange={(e) => setForm({ ...form, [item.id]: e.target.value })}
            />
          </span>
        </label>
      ))}
      <label className="full">
        {t('admin.payOnline')}
        <span>
          <input
            type="text"
            inputMode="url"
            placeholder={t('admin.payOnlinePlaceholder')}
            value={form.payOnline || ''}
            onChange={(e) => setForm({ ...form, payOnline: e.target.value })}
          />
        </span>
        <small className="adm-hint">{t('admin.payOnlineLead')}</small>
      </label>
      {error && <p className="form-error">{error}</p>}
      <button className="btn-orange full" type="submit" disabled={busy}>
        {busy ? t('admin.socialsSaving') : saved ? t('admin.socialsSaved') : t('admin.socialsSave')}
      </button>
    </form>
  )
}
