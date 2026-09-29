import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Gallery() {
  const { footerGallery } = useApp()
  const { t } = useLang()
  const items = (footerGallery || []).filter((item) => item.image)

  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('home.galleryEyebrow')}</p>
        <h1 className="page-title">{t('home.galleryTitle')}</h1>
        <div className="gallery-page-grid">
          {!items.length && <p className="lead">{t('home.noGallery')}</p>}
          {items.map((item, i) => (
            <figure key={`${item.image}-${i}`} className="gallery-page-card">
              <img src={item.image} alt={item.title || t('common.gallery')} />
              {item.title ? <figcaption>{item.title}</figcaption> : null}
            </figure>
          ))}
        </div>
      </div>
    </section>
  )
}
