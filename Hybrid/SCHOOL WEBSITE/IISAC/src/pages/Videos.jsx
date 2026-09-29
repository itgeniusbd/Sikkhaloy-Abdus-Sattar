import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { youtubeEmbed, youtubeId } from '../lib/youtube.js'

export default function Videos() {
  const { videos } = useApp()
  const { t, lang } = useLang()
  const items = (videos || []).filter((item) => youtubeId(item.url))

  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('pages.videosEyebrow')}</p>
        <h1 className="page-title">{t('pages.videosTitle')}</h1>
        <div className="video-grid">
          {!items.length && <p className="lead">{t('pages.noVideos')}</p>}
          {items.map((item) => {
            const title = lang === 'bn' ? item.titleBn || item.title : item.title
            return (
              <article key={item.id || item.url} className="video-card">
                <div className="video-frame">
                  <iframe
                    src={youtubeEmbed(item.url)}
                    title={title || t('pages.videosTitle')}
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                    allowFullScreen
                  />
                </div>
                {title ? <h3>{title}</h3> : null}
              </article>
            )
          })}
        </div>
      </div>
    </section>
  )
}
