import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Blog() {
  const { posts } = useApp()
  const { t, postText } = useLang()
  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('pages.blogEyebrow')}</p>
        <h1 className="page-title">{t('pages.blogTitle')}</h1>
        <div className="project-grid">
          {!posts.length && <p className="lead">{t('common.noPosts')}</p>}
          {posts.map((post) => {
            const item = postText(post)
            return (
              <article key={post.id} className="project-card">
                <Link to={`/blog/${post.id}`} className="card-media">
                  <img src={post.image} alt={item.title} />
                </Link>
                <div className="card-body">
                  <p className="card-loc">{item.category}</p>
                  <h3>
                    <Link to={`/blog/${post.id}`}>{item.title}</Link>
                  </h3>
                  <p>{item.excerpt}</p>
                  <Link to={`/blog/${post.id}`} className="card-cta">
                    {t('common.readMore')}
                  </Link>
                </div>
              </article>
            )
          })}
        </div>
      </div>
    </section>
  )
}
