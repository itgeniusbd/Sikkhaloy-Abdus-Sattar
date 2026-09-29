import { Link } from 'react-router-dom'
import { CATEGORIES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Sidebar() {
  const { projects, posts } = useApp()
  const { t, label, postText } = useLang()

  return (
    <aside className="sidebar">
      <div className="side-box">
        <h3>{t('common.category')}</h3>
        <ul className="cat-list">
          {CATEGORIES.map((cat) => {
            const count = projects.filter((p) => p.category === cat).length
            return (
              <li key={cat}>
                <Link to={`/project?category=${encodeURIComponent(cat)}`}>
                  <span>→ {label('cat', cat)}</span>
                  <em>({count})</em>
                </Link>
              </li>
            )
          })}
        </ul>
      </div>

      <div className="side-box">
        <h3>{t('common.recentPost')}</h3>
        <ul className="recent-list">
          {posts.map((post) => {
            const item = postText(post)
            return (
              <li key={post.id}>
                <Link to={`/blog/${post.id}`}>
                  <img src={post.image} alt="" />
                  <div>
                    <small>{item.category}</small>
                    <strong>{item.title}</strong>
                  </div>
                </Link>
              </li>
            )
          })}
        </ul>
      </div>
    </aside>
  )
}
