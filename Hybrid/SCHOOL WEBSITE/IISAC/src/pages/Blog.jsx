import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

function sameCategory(value, wanted) {
  return String(value || '').trim().toLowerCase() === String(wanted || '').trim().toLowerCase()
}

export default function Blog({ category, eyebrowKey = 'pages.blogEyebrow', titleKey = 'pages.blogTitle', emptyKey = 'common.noPosts' }) {
  const { posts } = useApp()
  const { t, postText } = useLang()
  const list = category ? posts.filter((post) => sameCategory(post.category, category)) : posts
  const base = category && sameCategory(category, 'Events') ? '/events' : '/notice'
  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t(eyebrowKey)}</p>
        <h1 className="page-title">{t(titleKey)}</h1>
        <div className="project-grid">
          {!list.length && <p className="lead">{t(emptyKey)}</p>}
          {list.map((post) => {
            const item = postText(post)
            const href = `${base}/${post.id}`
            return (
              <article key={post.id} className="project-card">
                <Link to={href} className="card-media">
                  <img src={post.image} alt={item.title} />
                </Link>
                <div className="card-body">
                  <p className="card-loc">{item.category}</p>
                  <h3>
                    <Link to={href}>{item.title}</Link>
                  </h3>
                  <p>{item.excerpt}</p>
                  <Link to={href} className="card-cta">
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
