import { useState } from 'react'
import { BLOG_CATEGORIES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'

const emptyPost = {
  title: '',
  category: BLOG_CATEGORIES[0],
  imageUrl: '',
  excerpt: '',
  body: '',
}

function slugify(text) {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '')
}

export default function AdminBlog() {
  const { posts, savePost, deletePost } = useApp()
  const [form, setForm] = useState(emptyPost)
  const [editingId, setEditingId] = useState(null)
  const [imageFile, setImageFile] = useState(null)
  const [fileKey, setFileKey] = useState(0)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')

  function reset() {
    setEditingId(null)
    setForm(emptyPost)
    setImageFile(null)
    setFileKey((k) => k + 1)
  }

  function startEdit(post) {
    setEditingId(post.id)
    setForm({
      title: post.title,
      category: post.category || BLOG_CATEGORIES[0],
      imageUrl: post.image || '',
      excerpt: post.excerpt || '',
      body: post.body || '',
    })
    setImageFile(null)
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
      await savePost(
        {
          id: editingId || slugify(form.title) || `post-${Date.now()}`,
          title: form.title,
          category: form.category,
          image: form.imageUrl,
          excerpt: form.excerpt,
          body: form.body,
        },
        imageFile,
      )
      reset()
      setSaved(true)
    } catch (err) {
      setError(err.message || 'Could not save post.')
    } finally {
      setBusy(false)
    }
  }

  const preview = imageFile ? URL.createObjectURL(imageFile) : form.imageUrl

  return (
    <div className="container admin-layout">
      <form className="quote-form admin-form" onSubmit={submit}>
        {editingId && (
          <div className="edit-banner">
            <span>Editing {form.title || 'post'}</span>
            <button type="button" onClick={reset}>
              Cancel edit
            </button>
          </div>
        )}
        <label className="full">
          Post title
          <span>
            <input required value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
          </span>
        </label>
        <label>
          Category
          <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {BLOG_CATEGORIES.map((c) => (
              <option key={c}>{c}</option>
            ))}
          </select>
        </label>
        {preview && <img className="admin-preview" src={preview} alt="" />}
        <label className="full">
          Cover photo
          <input key={`post-${fileKey}`} type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
        </label>
        <label className="full">
          Or image URL
          <span>
            <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
          </span>
        </label>
        <label className="full">
          Short excerpt (card text)
          <span>
            <textarea required rows="3" value={form.excerpt} onChange={(e) => setForm({ ...form, excerpt: e.target.value })} />
          </span>
        </label>
        <label className="full">
          Full post
          <span>
            <textarea required rows="8" value={form.body} onChange={(e) => setForm({ ...form, body: e.target.value })} />
          </span>
        </label>
        {error && <p className="form-error">{error}</p>}
        <button className="btn-orange full" type="submit" disabled={busy}>
          {busy ? 'SAVING...' : editingId ? 'UPDATE POST' : saved ? 'POST SAVED — ADD ANOTHER' : 'SAVE POST'}
        </button>
      </form>

      <div className="admin-list">
        <h3>All posts ({posts.length})</h3>
        <p className="lead">Click Edit to change a post. New posts appear on the Blog page.</p>
        {posts.map((post) => (
          <div key={post.id} className="admin-row">
            <img src={post.image} alt="" />
            <div>
              <strong>{post.title}</strong>
              <small>{post.category}</small>
            </div>
            <button type="button" className="btn-edit" onClick={() => startEdit(post)}>
              Edit
            </button>
            <button
              type="button"
              className="btn-del"
              onClick={async () => {
                if (!window.confirm(`Delete ${post.title}?`)) return
                try {
                  await deletePost(post.id)
                  if (editingId === post.id) reset()
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
  )
}
