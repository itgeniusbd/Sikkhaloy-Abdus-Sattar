import { useEffect, useState } from 'react'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { youtubeId, youtubeThumb } from '../lib/youtube.js'

export default function AdminGallery() {
  const { footerGallery, videos, saveFooterGallery, saveVideos } = useApp()
  const { t } = useLang()
  const [photos, setPhotos] = useState(footerGallery || [])
  const [photoFiles, setPhotoFiles] = useState({})
  const [clips, setClips] = useState(videos || [])
  const [fileKey, setFileKey] = useState(0)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    setPhotos(footerGallery?.length ? footerGallery : [])
  }, [footerGallery])

  useEffect(() => {
    setClips(videos?.length ? videos : [])
  }, [videos])

  async function savePhotos(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved('')
    try {
      await saveFooterGallery(photos, photoFiles)
      setPhotoFiles({})
      setFileKey((k) => k + 1)
      setSaved('photos')
    } catch (err) {
      setError(err.message || t('admin.galleryError'))
    } finally {
      setBusy(false)
    }
  }

  async function saveClips(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved('')
    try {
      await saveVideos(clips)
      setSaved('videos')
    } catch (err) {
      setError(err.message || t('admin.galleryError'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="container hero-admin">
      {error ? <p className="form-error">{error}</p> : null}

      <form onSubmit={savePhotos}>
        <h3>{t('admin.photoGallery')}</h3>
        <p className="lead">{t('admin.photoGalleryLead')}</p>
        <div className="footer-admin-grid">
          {photos.map((item, i) => {
            const preview = photoFiles[i] ? URL.createObjectURL(photoFiles[i]) : item.image
            return (
              <article key={`${item.image || 'new'}-${i}`} className="hero-admin-card quote-form">
                <div className="hero-admin-head">
                  <h3>
                    {t('admin.photoN')} {i + 1}
                  </h3>
                  <button
                    type="button"
                    className="btn-del"
                    onClick={() => {
                      setPhotos(photos.filter((_, idx) => idx !== i))
                      setPhotoFiles((prev) => {
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
                    {t('admin.deleteMember')}
                  </button>
                </div>
                {preview ? <img src={preview} alt="" className="admin-preview" /> : <div className="admin-preview empty-preview">{t('admin.memberPhoto')}</div>}
                <label className="full">
                  {t('admin.replacePhoto')}
                  <input
                    key={`photo-${fileKey}-${i}`}
                    type="file"
                    accept="image/*"
                    onChange={(e) => setPhotoFiles({ ...photoFiles, [i]: e.target.files?.[0] || null })}
                  />
                </label>
                <label className="full">
                  {t('admin.photoCaption')}
                  <span>
                    <input
                      value={item.title || ''}
                      onChange={(e) => {
                        const next = [...photos]
                        next[i] = { ...item, title: e.target.value, link: item.link || '/gallery' }
                        setPhotos(next)
                      }}
                    />
                  </span>
                </label>
              </article>
            )
          })}
        </div>
        {photos.length < 48 && (
          <button type="button" className="btn-outline" onClick={() => setPhotos([...photos, { image: '', title: '', link: '/gallery' }])}>
            + {t('admin.addPhoto')}
          </button>
        )}
        <button className="btn-orange" type="submit" disabled={busy}>
          {busy ? t('admin.profileSaving') : saved === 'photos' ? t('admin.profileSaved') : t('admin.savePhotos')}
        </button>
      </form>

      <form onSubmit={saveClips}>
        <h3>{t('admin.videoGallery')}</h3>
        <p className="lead">{t('admin.videoGalleryLead')}</p>
        <div className="footer-admin-grid">
          {clips.map((item, i) => {
            const thumb = youtubeThumb(item.url)
            return (
              <article key={item.id || i} className="hero-admin-card quote-form">
                <div className="hero-admin-head">
                  <h3>
                    {t('admin.videoN')} {i + 1}
                  </h3>
                  <button type="button" className="btn-del" onClick={() => setClips(clips.filter((_, idx) => idx !== i))}>
                    {t('admin.deleteMember')}
                  </button>
                </div>
                {thumb ? <img src={thumb} alt="" className="admin-preview" /> : <div className="admin-preview empty-preview">{t('admin.youtubeHint')}</div>}
                <label className="full">
                  {t('admin.youtubeUrl')}
                  <span>
                    <input
                      required
                      value={item.url || ''}
                      placeholder="https://www.youtube.com/watch?v=..."
                      onChange={(e) => {
                        const next = [...clips]
                        next[i] = { ...item, url: e.target.value }
                        setClips(next)
                      }}
                    />
                  </span>
                </label>
                {item.url && !youtubeId(item.url) ? <p className="form-error">{t('admin.youtubeInvalid')}</p> : null}
                <label className="full">
                  {t('admin.videoTitle')}
                  <span>
                    <input
                      value={item.title || ''}
                      onChange={(e) => {
                        const next = [...clips]
                        next[i] = { ...item, title: e.target.value }
                        setClips(next)
                      }}
                    />
                  </span>
                </label>
                <label className="full">
                  {t('admin.videoTitleBn')}
                  <span>
                    <input
                      value={item.titleBn || ''}
                      onChange={(e) => {
                        const next = [...clips]
                        next[i] = { ...item, titleBn: e.target.value }
                        setClips(next)
                      }}
                    />
                  </span>
                </label>
              </article>
            )
          })}
        </div>
        {clips.length < 24 && (
          <button
            type="button"
            className="btn-outline"
            onClick={() => setClips([...clips, { id: `v-${Date.now().toString(36)}`, url: '', title: '', titleBn: '' }])}
          >
            + {t('admin.addVideo')}
          </button>
        )}
        <button className="btn-orange" type="submit" disabled={busy}>
          {busy ? t('admin.profileSaving') : saved === 'videos' ? t('admin.profileSaved') : t('admin.saveVideos')}
        </button>
      </form>
    </div>
  )
}
