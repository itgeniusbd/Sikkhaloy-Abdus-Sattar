import { useState } from 'react'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { IconMail, IconPhone, IconPin, IconUser, IconWhatsApp } from '../components/Icons.jsx'
import { telHref, waHref } from '../lib/phone.js'

export default function Contact() {
  const { sendMessage, pages, site } = useApp()
  const { t, pageText } = useLang()
  const { contact } = pageText({ ...pages, contact: { ...pages.contact, ...site } })
  const [form, setForm] = useState({ name: '', phone: '', email: '', address: '', message: '' })
  const [sent, setSent] = useState(false)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  return (
    <section className="page-pad">
      <div className="container quote-grid">
        <div>
          <p className="eyebrow">{contact.eyebrow}</p>
          <h1 className="page-title">{contact.title}</h1>
          <p className="lead">{contact.lead}</p>
          <div className="help-box">
            <IconPhone />
            <div>
              <strong>{t('common.needHelp')}</strong>
              <a href={`tel:${String(contact.phone || '').replace(/\s/g, '')}`}>{contact.phone}</a>
              <p>{contact.address}</p>
              <p>{contact.email}</p>
              <div className="contact-actions" style={{ marginTop: 12 }}>
                <a className="btn-orange" href={telHref(contact.phone)}>
                  <IconPhone /> {t('common.call')}
                </a>
                <a className="btn-whatsapp" href={waHref(contact.whatsapp, t('common.waContact'))} target="_blank" rel="noreferrer">
                  <IconWhatsApp /> {t('common.whatsapp')}
                </a>
              </div>
            </div>
          </div>
        </div>
        <form
          className="quote-form"
          onSubmit={async (e) => {
            e.preventDefault()
            setBusy(true)
            setError('')
            setSent(false)
            try {
              await sendMessage(form)
              setSent(true)
              setForm({ name: '', phone: '', email: '', address: '', message: '' })
            } catch (err) {
              setError(err.message || t('common.messageFailed'))
            } finally {
              setBusy(false)
            }
          }}
        >
          <label>
            {t('common.yourName')}
            <span>
              <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              <IconUser />
            </span>
          </label>
          <label>
            {t('common.yourPhone')}
            <span>
              <input required value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              <IconPhone />
            </span>
          </label>
          <label>
            {t('common.yourEmail')}
            <span>
              <input type="email" required value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              <IconMail />
            </span>
          </label>
          <label>
            {t('common.yourAddress')}
            <span>
              <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
              <IconPin />
            </span>
          </label>
          <label className="full">
            {t('common.yourMessage')}
            <span>
              <textarea required rows="4" value={form.message} onChange={(e) => setForm({ ...form, message: e.target.value })} />
              <IconMail />
            </span>
          </label>
          {error && <p className="form-error">{error}</p>}
          <button className="btn-orange full" type="submit" disabled={busy}>
            {busy ? t('common.sending') : sent ? t('common.messageSent') : t('common.sendMessage')}
          </button>
        </form>
      </div>
    </section>
  )
}
