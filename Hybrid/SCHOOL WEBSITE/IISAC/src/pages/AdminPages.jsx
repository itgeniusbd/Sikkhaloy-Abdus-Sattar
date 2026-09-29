import { useEffect, useState } from 'react'
import { DEFAULT_PAGES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'

export default function AdminPages({ section }) {
  const { pages, savePages } = useApp()
  const [about, setAbout] = useState(pages.about || DEFAULT_PAGES.about)
  const [contact, setContact] = useState(pages.contact || DEFAULT_PAGES.contact)
  const [imageFile, setImageFile] = useState(null)
  const [fileKey, setFileKey] = useState(0)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setAbout({ ...DEFAULT_PAGES.about, ...pages.about })
    setContact({
      ...DEFAULT_PAGES.contact,
      ...pages.contact,
      })
  }, [pages])

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved(false)
    try {
      const { featuresText, ...aboutRest } = about
      await savePages(
        {
          about: {
            ...aboutRest,
            features: String(featuresText ?? (about.features || []).join('\n'))
              .split('\n')
              .map((item) => item.trim())
              .filter(Boolean),
          },
          contact,
        },
        imageFile,
      )
      setImageFile(null)
      setFileKey((k) => k + 1)
      setSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save page.')
    } finally {
      setBusy(false)
    }
  }

  const preview = imageFile ? URL.createObjectURL(imageFile) : about.image
  const featuresText = about.featuresText ?? (about.features || []).join('\n')

  return (
    <form className="container hero-admin quote-form" onSubmit={submit}>
      {section === 'about' ? (
        <>
          <p className="lead">Change the About page photo and text. This also updates the About block on the home page.</p>
          {preview && <img className="admin-preview" src={preview} alt="" />}
          <label className="full">
            About photo
            <input key={`about-${fileKey}`} type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
          </label>
          <label>
            Small label
            <span>
              <input value={about.eyebrow || ''} onChange={(e) => setAbout({ ...about, eyebrow: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Heading
            <span>
              <input required value={about.title || ''} onChange={(e) => setAbout({ ...about, title: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Short intro
            <span>
              <textarea required rows="3" value={about.lead || ''} onChange={(e) => setAbout({ ...about, lead: e.target.value })} />
            </span>
          </label>
          <label className="full">
            About details
            <span>
              <textarea required rows="5" value={about.body || ''} onChange={(e) => setAbout({ ...about, body: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Feature list (one line each)
            <span>
              <textarea
                rows="5"
                value={featuresText}
                onChange={(e) => setAbout({ ...about, featuresText: e.target.value })}
              />
            </span>
          </label>
        </>
      ) : (
        <>
          <p className="lead">Contact page text, phone, WhatsApp, email, and office address. Site name and dialog are in Site settings.</p>
          <label>
            Small label
            <span>
              <input value={contact.eyebrow || ''} onChange={(e) => setContact({ ...contact, eyebrow: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Heading
            <span>
              <input required value={contact.title || ''} onChange={(e) => setContact({ ...contact, title: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Intro text
            <span>
              <textarea required rows="3" value={contact.lead || ''} onChange={(e) => setContact({ ...contact, lead: e.target.value })} />
            </span>
          </label>
          <label>
            Phone
            <span>
              <input required value={contact.phone || ''} onChange={(e) => setContact({ ...contact, phone: e.target.value })} />
            </span>
          </label>
          <label>
            WhatsApp
            <span>
              <input required value={contact.whatsapp || ''} onChange={(e) => setContact({ ...contact, whatsapp: e.target.value })} placeholder="8801XXXXXXXXX" />
            </span>
          </label>
          <label>
            Email
            <span>
              <input required value={contact.email || ''} onChange={(e) => setContact({ ...contact, email: e.target.value })} />
            </span>
          </label>
          <label className="full">
            Office address
            <span>
              <input required value={contact.address || ''} onChange={(e) => setContact({ ...contact, address: e.target.value })} />
            </span>
          </label>
        </>
      )}
      {error && <p className="form-error">{error}</p>}
      <button className="btn-orange" type="submit" disabled={busy}>
        {busy ? 'SAVING...' : saved ? 'PAGE SAVED' : section === 'about' ? 'SAVE ABOUT PAGE' : 'SAVE CONTACT PAGE'}
      </button>
    </form>
  )
}
