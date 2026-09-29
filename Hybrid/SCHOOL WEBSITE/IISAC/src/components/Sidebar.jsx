import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Sidebar() {
  const { posts } = useApp()
  const { t, postText } = useLang()

  return (
    <aside className="sidebar">
      <div className="side-box">
        <h3>{t('common.recentPost')}</h3>
        <ul className="recent-list">
          {posts.map((post) => {
            const item = postText(post)
            return (
              <li key={post.id}>
                <Link to={`/notice/${post.id}`}>
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
