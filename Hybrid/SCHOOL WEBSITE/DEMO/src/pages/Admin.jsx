import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { CATEGORIES, LOCATIONS, PROPERTY_TYPES, SITE } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import AdminSidebar from '../components/AdminSidebar.jsx'
import { IconMenu } from '../components/Icons.jsx'
import AdminBlog from './AdminBlog.jsx'
import AdminMessages from './AdminMessages.jsx'
import AdminSubscribers from './AdminSubscribers.jsx'
import AdminSocial from './AdminSocial.jsx'
import AdminPages from './AdminPages.jsx'
import AdminProfile from './AdminProfile.jsx'

const empty = {
  title: '',
  status: 'ongoing',
  badge: 'SALES',
  category: CATEGORIES[0],
  type: 'Apartment',
  location: '',
  city: LOCATIONS[0],
  price: '',
  beds: 3,
  baths: 2,
  rooms: 4,
  parking: 1,
  size: 1200,
  imageUrl: '',
  description: '',
  featuresText: '24/7 Security, Reserved Parking, Backup Generator',
  phone: SITE.phone,
  whatsapp: SITE.whatsapp,
}

function slugify(text) {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '')
}

function fromProject(p) {
  return {
    title: p.title || '',
    status: p.status || 'ongoing',
    badge: p.badge || 'SALES',
    category: p.category || CATEGORIES[0],
    type: p.type || 'Apartment',
    location: p.location || '',
    city: p.city || LOCATIONS[0],
    price: p.price ?? '',
    beds: p.beds ?? 0,
    baths: p.baths ?? 0,
    rooms: p.rooms ?? 0,
    parking: p.parking ?? 0,
    size: p.size ?? 0,
    imageUrl: p.image || '',
    description: p.description || '',
    featuresText: (p.features || []).join(', '),
    phone: p.phone || SITE.phone,
    whatsapp: p.whatsapp || SITE.whatsapp,
  }
}

export default function Admin() {
  const { projects, addProject, deleteProject, logout, heroSlides, saveHeroSlides, footerGallery, saveFooterGallery } = useApp()
  const { t } = useLang()
  const navigate = useNavigate()
  const [tab, setTab] = useState('projects')
  const [form, setForm] = useState(empty)
  const [editing, setEditing] = useState(null)
  const [imageFile, setImageFile] = useState(null)
  const [galleryFiles, setGalleryFiles] = useState([])
  const [existingGallery, setExistingGallery] = useState([])
  const [fileKey, setFileKey] = useState(0)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [slides, setSlides] = useState(heroSlides)
  const [heroFiles, setHeroFiles] = useState({})
  const [heroSaved, setHeroSaved] = useState(false)
  const [footerItems, setFooterItems] = useState(footerGallery)
  const [footerFiles, setFooterFiles] = useState({})
  const [footerSaved, setFooterSaved] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)

  useEffect(() => {
    setSlides(heroSlides)
  }, [heroSlides])

  useEffect(() => {
    setFooterItems(footerGallery?.length ? footerGallery : projects.slice(0, 6).map((p) => ({
      image: p.image,
      title: p.title,
      link: `/project/${p.slug}`,
    })))
  }, [footerGallery, projects])

  function resetProjectForm() {
    setEditing(null)
    setForm(empty)
    setImageFile(null)
    setGalleryFiles([])
    setExistingGallery([])
    setFileKey((k) => k + 1)
    setSaved(false)
  }

  function startEdit(project) {
    setTab('projects')
    setEditing({ id: project.id, slug: project.slug })
    setForm(fromProject(project))
    setExistingGallery(project.gallery?.length ? project.gallery : project.image ? [project.image] : [])
    setImageFile(null)
    setGalleryFiles([])
    setFileKey((k) => k + 1)
    setSaved(false)
    setError('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    try {
      const slug = editing?.slug || slugify(form.title) || `project-${Date.now()}`
      const id = editing?.id || slug
      await addProject(
        {
          id,
          slug,
          title: form.title,
          status: form.status,
          deal: 'buy',
          badge: form.badge,
          category: form.category,
          type: form.type,
          location: form.location,
          city: form.city,
          price: Number(form.price) || 0,
          beds: Number(form.beds) || 0,
          baths: Number(form.baths) || 0,
          rooms: Number(form.rooms) || 0,
          parking: Number(form.parking) || 0,
          size: Number(form.size) || 0,
          image: form.imageUrl,
          gallery: existingGallery,
          description: form.description,
          features: form.featuresText.split(',').map((s) => s.trim()).filter(Boolean),
          phone: form.phone,
          whatsapp: form.whatsapp,
        },
        { image: imageFile, gallery: galleryFiles },
      )
      resetProjectForm()
      setSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save project. Check MySQL connection.')
    } finally {
      setBusy(false)
    }
  }

  async function saveFooter(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setFooterSaved(false)
    try {
      await saveFooterGallery(footerItems, footerFiles)
      setFooterFiles({})
      setFileKey((k) => k + 1)
      setFooterSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save footer gallery.')
    } finally {
      setBusy(false)
    }
  }

  async function saveHero(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setHeroSaved(false)
    try {
      await saveHeroSlides(slides, heroFiles)
      setHeroFiles({})
      setFileKey((k) => k + 1)
      setHeroSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save slider.')
    } finally {
      setBusy(false)
    }
  }

  const previewImage = imageFile ? URL.createObjectURL(imageFile) : form.imageUrl
  const title =
    tab === 'slider'
      ? t('admin.homeSlider')
      : tab === 'footer'
        ? t('admin.footerGallery')
        : tab === 'blog'
          ? t('admin.blogPosts')
          : tab === 'about'
            ? t('admin.aboutPage')
            : tab === 'messages'
              ? t('admin.messages')
              : tab === 'subscribers'
                ? t('admin.subscribers')
              : tab === 'socials'
                ? t('admin.socials')
              : tab === 'contact'
                ? t('admin.contactPage')
                : tab === 'profile'
                  ? t('admin.profile')
                : editing
                  ? t('admin.editProject')
                  : t('admin.addProject')

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <div className="admin-shell">
      {menuOpen && <button type="button" className="admin-backdrop" aria-label="Close" onClick={() => setMenuOpen(false)} />}
      <AdminSidebar
        tab={tab}
        onTab={setTab}
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
        onLogout={handleLogout}
      />
      <div className="admin-main">
        <header className="admin-topbar">
          <button type="button" className="admin-menu-btn" onClick={() => setMenuOpen(true)} aria-label={t('nav.menu')}>
            <IconMenu />
          </button>
          <div>
            <p className="eyebrow">{t('admin.dashboard')}</p>
            <h1>{title}</h1>
          </div>
        </header>

        {error && (
          <div className="admin-page">
            <p className="form-error">{error}</p>
          </div>
        )}

      {tab === 'slider' ? (
        <form className="container hero-admin" onSubmit={saveHero}>
          <p className="lead">
            Each slide has its own photo and text. Upload a new image to replace the current one, then click Save slider.
          </p>
          {slides.map((slide, i) => {
            const preview = heroFiles[i] ? URL.createObjectURL(heroFiles[i]) : slide.image
            return (
              <article key={slide.id || i} className="hero-admin-card quote-form">
                <div className="hero-admin-head">
                  <h3>Slide {i + 1}</h3>
                  {slides.length > 1 && (
                    <button
                      type="button"
                      className="btn-del"
                      onClick={() => {
                        setSlides(slides.filter((_, idx) => idx !== i))
                        setHeroFiles((prev) => {
                          const next = {}
                          Object.entries(prev).forEach(([key, file]) => {
                            const n = Number(key)
                            if (n < i) next[n] = file
                            if (n > i) next[n - 1] = file
                          })
                          return next
                        })
                      }}
                    >
                      Remove slide
                    </button>
                  )}
                </div>
                {preview ? <img src={preview} alt="" className="admin-preview" /> : <div className="admin-preview empty-preview">No image yet</div>}
                <label className="full">
                  Replace photo
                  <input
                    key={`hero-${fileKey}-${i}`}
                    type="file"
                    accept="image/*"
                    onChange={(e) => setHeroFiles({ ...heroFiles, [i]: e.target.files?.[0] || null })}
                  />
                </label>
                <label>
                  Small label
                  <span>
                    <input
                      value={slide.eyebrow}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, eyebrow: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
                <label className="full">
                  Heading
                  <span>
                    <input
                      required
                      value={slide.title}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, title: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
                <label className="full">
                  Text on this photo
                  <span>
                    <textarea
                      required
                      rows="3"
                      value={slide.text}
                      onChange={(e) => {
                        const next = [...slides]
                        next[i] = { ...slide, text: e.target.value }
                        setSlides(next)
                      }}
                    />
                  </span>
                </label>
              </article>
            )
          })}
          {slides.length < 8 && (
            <button
              type="button"
              className="btn-outline"
              onClick={() =>
                setSlides([
                  ...slides,
                  {
                    id: `slide-${Date.now()}`,
                    image: '',
                    eyebrow: 'SAFA PROPERTIS',
                    title: '',
                    text: '',
                  },
                ])
              }
            >
              Add another slide
            </button>
          )}
          <button className="btn-orange" type="submit" disabled={busy}>
            {busy ? 'SAVING...' : heroSaved ? 'SLIDER SAVED' : 'SAVE SLIDER'}
          </button>
        </form>
      ) : tab === 'footer' ? (
        <form className="container hero-admin" onSubmit={saveFooter}>
          <p className="lead">
            These 6 photos appear in the footer under Our gallery. Replace any photo, then click Save gallery.
          </p>
          <div className="footer-admin-grid">
            {footerItems.map((item, i) => {
              const preview = footerFiles[i] ? URL.createObjectURL(footerFiles[i]) : item.image
              return (
                <article key={`${item.image}-${i}`} className="hero-admin-card quote-form">
                  <div className="hero-admin-head">
                    <h3>Photo {i + 1}</h3>
                    <button
                      type="button"
                      className="btn-del"
                      onClick={() => {
                        setFooterItems(footerItems.filter((_, idx) => idx !== i))
                        setFooterFiles((prev) => {
                          const next = {}
                          Object.entries(prev).forEach(([key, file]) => {
                            const n = Number(key)
                            if (n < i) next[n] = file
                            if (n > i) next[n - 1] = file
                          })
                          return next
                        })
                      }}
                    >
                      Remove
                    </button>
                  </div>
                  {preview ? <img src={preview} alt="" className="admin-preview" /> : <div className="admin-preview empty-preview">No image</div>}
                  <label className="full">
                    Replace photo
                    <input
                      key={`foot-${fileKey}-${i}`}
                      type="file"
                      accept="image/*"
                      onChange={(e) => setFooterFiles({ ...footerFiles, [i]: e.target.files?.[0] || null })}
                    />
                  </label>
                  <label className="full">
                    Opens this project
                    <select
                      value={item.link || '/project'}
                      onChange={(e) => {
                        const next = [...footerItems]
                        const picked = projects.find((p) => `/project/${p.slug}` === e.target.value)
                        next[i] = {
                          ...item,
                          link: e.target.value,
                          title: picked?.title || item.title,
                        }
                        setFooterItems(next)
                      }}
                    >
                      <option value="/project">All projects</option>
                      {projects.map((p) => (
                        <option key={p.id} value={`/project/${p.slug}`}>
                          {p.title}
                        </option>
                      ))}
                    </select>
                  </label>
                </article>
              )
            })}
          </div>
          {footerItems.length < 6 && (
            <button
              type="button"
              className="btn-outline"
              onClick={() => setFooterItems([...footerItems, { image: '', title: '', link: '/project' }])}
            >
              Add another photo
            </button>
          )}
          <button className="btn-orange" type="submit" disabled={busy}>
            {busy ? 'SAVING...' : footerSaved ? 'GALLERY SAVED' : 'SAVE GALLERY'}
          </button>
        </form>
      ) : tab === 'blog' ? (
        <AdminBlog />
      ) : tab === 'about' ? (
        <AdminPages section="about" />
      ) : tab === 'messages' ? (
        <AdminMessages />
      ) : tab === 'subscribers' ? (
        <AdminSubscribers />
      ) : tab === 'socials' ? (
        <AdminSocial />
      ) : tab === 'contact' ? (
        <AdminPages section="contact" />
      ) : tab === 'profile' ? (
        <AdminProfile />
      ) : (
        <div className="container admin-layout">
          <form className="quote-form admin-form" onSubmit={submit}>
            {editing && (
              <div className="edit-banner">
                <span>Editing {form.title || 'project'}</span>
                <button type="button" onClick={resetProjectForm}>
                  Cancel edit
                </button>
              </div>
            )}
            <label className="full">
              Project title
              <span>
                <input required value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
              </span>
            </label>
            <label>
              Status
              <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
                <option value="completed">Completed</option>
                <option value="ongoing">Ongoing</option>
                <option value="upcoming">Upcoming</option>
              </select>
            </label>
            <label>
              Badge
              <span>
                <input value={form.badge} onChange={(e) => setForm({ ...form, badge: e.target.value })} />
              </span>
            </label>
            <label>
              Category
              <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
                {CATEGORIES.map((c) => (
                  <option key={c}>{c}</option>
                ))}
              </select>
            </label>
            <label>
              Type
              <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })}>
                {PROPERTY_TYPES.map((t) => (
                  <option key={t}>{t}</option>
                ))}
              </select>
            </label>
            <label>
              City
              <select value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })}>
                {LOCATIONS.map((c) => (
                  <option key={c}>{c}</option>
                ))}
              </select>
            </label>
            <label className="full">
              Address / location
              <span>
                <input required value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
              </span>
            </label>
            <label>
              Price
              <span>
                <input type="number" required value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
              </span>
            </label>
            <label>
              Size (sqft)
              <span>
                <input type="number" value={form.size} onChange={(e) => setForm({ ...form, size: e.target.value })} />
              </span>
            </label>
            <label>
              Rooms
              <span>
                <input type="number" value={form.rooms} onChange={(e) => setForm({ ...form, rooms: e.target.value })} />
              </span>
            </label>
            <label>
              Beds
              <span>
                <input type="number" value={form.beds} onChange={(e) => setForm({ ...form, beds: e.target.value })} />
              </span>
            </label>
            <label>
              Baths
              <span>
                <input type="number" value={form.baths} onChange={(e) => setForm({ ...form, baths: e.target.value })} />
              </span>
            </label>
            <label>
              Parking
              <span>
                <input type="number" value={form.parking} onChange={(e) => setForm({ ...form, parking: e.target.value })} />
              </span>
            </label>
            <label>
              Contact phone
              <span>
                <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </span>
            </label>
            <label>
              WhatsApp number
              <span>
                <input value={form.whatsapp} onChange={(e) => setForm({ ...form, whatsapp: e.target.value })} placeholder="8801XXXXXXXXX" />
              </span>
            </label>
            {previewImage && <img className="admin-preview" src={previewImage} alt="" />}
            <label className="full">
              Main photo (upload to replace)
              <input key={`main-${fileKey}`} type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
            </label>
            <label className="full">
              Or image URL
              <span>
                <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
              </span>
            </label>
            <div className="full gallery-block">
              <p>Project gallery</p>
              <div className="gallery-manage">
                {existingGallery.map((src) => (
                  <div key={src} className="gallery-thumb">
                    <img src={src} alt="" />
                    <button type="button" aria-label="Remove photo" onClick={() => setExistingGallery(existingGallery.filter((item) => item !== src))}>
                      ×
                    </button>
                  </div>
                ))}
              </div>
              <label>
                Add more gallery photos
                <input
                  key={`gal-${fileKey}`}
                  type="file"
                  accept="image/*"
                  multiple
                  onChange={(e) => setGalleryFiles([...(e.target.files || [])])}
                />
              </label>
              {galleryFiles.length > 0 && <small>{galleryFiles.length} new photo(s) selected</small>}
            </div>
            <label className="full">
              Description
              <span>
                <textarea required rows="4" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
              </span>
            </label>
            <label className="full">
              Features (comma separated)
              <span>
                <input value={form.featuresText} onChange={(e) => setForm({ ...form, featuresText: e.target.value })} />
              </span>
            </label>
            <button className="btn-orange full" type="submit" disabled={busy}>
              {busy ? 'SAVING...' : editing ? 'UPDATE PROJECT' : saved ? 'PROJECT SAVED — ADD ANOTHER' : 'SAVE PROJECT'}
            </button>
          </form>

          <div className="admin-list">
            <h3>All projects ({projects.length})</h3>
            <p className="lead">Click Edit to change details, main photo, or gallery. Footer gallery uses these project photos.</p>
            {projects.map((p) => (
              <div key={p.id} className="admin-row">
                <img src={p.image} alt="" />
                <div>
                  <strong>{p.title}</strong>
                  <small>
                    {p.status} · {p.location}
                  </small>
                </div>
                <button type="button" className="btn-edit" onClick={() => startEdit(p)}>
                  Edit
                </button>
                <button
                  type="button"
                  className="btn-del"
                  onClick={async () => {
                    if (!window.confirm(`Delete ${p.title}?`)) return
                    try {
                      await deleteProject(p.id)
                      if (editing?.id === p.id) resetProjectForm()
                    } catch (err) {
                      setError(err.message)
                    }
                  }}
                >
                  Delete
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
      </div>
    </div>
  )
}
