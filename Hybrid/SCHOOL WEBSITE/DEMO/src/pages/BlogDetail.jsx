import { Link, useParams } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import Sidebar from '../components/Sidebar.jsx'

export default function BlogDetail() {
  const { id } = useParams()
  const { posts } = useApp()
  const { t, postText } = useLang()
  const post = posts.find((p) => p.id === id)

  if (!post) {
    return (
      <section className="page-pad container">
        <h1>{t('common.notFoundPost')}</h1>
        <Link to="/blog">{t('common.backBlog')}</Link>
      </section>
    )
  }

  const item = postText(post)

  return (
    <section className="page-pad detail-page">
      <div className="container detail-grid">
        <div>
          <img className="detail-hero" src={post.image} alt={item.title} />
          <p className="eyebrow">{item.category}</p>
          <h1 className="page-title">{item.title}</h1>
          <p className="lead">{item.body}</p>
        </div>
        <Sidebar />
      </div>
    </section>
  )
}
