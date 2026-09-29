import cors from 'cors'
import crypto from 'node:crypto'
import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import bcrypt from 'bcryptjs'
import dotenv from 'dotenv'
import express from 'express'
import multer from 'multer'
import mysql from 'mysql2/promise'
import { DEFAULT_POSTS, DEFAULT_PROJECTS } from '../src/data/projects.js'
import { DEFAULT_PAGES, HERO_SLIDES, SITE } from '../src/data/site.js'

dotenv.config()

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const uploadDir = path.join(__dirname, '..', 'uploads', 'projects')
const heroDir = path.join(__dirname, '..', 'uploads', 'hero')
const footerDir = path.join(__dirname, '..', 'uploads', 'gallery')
const postsDir = path.join(__dirname, '..', 'uploads', 'posts')
const pagesDir = path.join(__dirname, '..', 'uploads', 'pages')
fs.mkdirSync(uploadDir, { recursive: true })
fs.mkdirSync(heroDir, { recursive: true })
fs.mkdirSync(footerDir, { recursive: true })
fs.mkdirSync(postsDir, { recursive: true })
fs.mkdirSync(pagesDir, { recursive: true })

function uploadFolder(fieldname) {
  if (String(fieldname).startsWith('hero')) return heroDir
  if (String(fieldname).startsWith('foot')) return footerDir
  if (String(fieldname).startsWith('post')) return postsDir
  if (String(fieldname).startsWith('about') || String(fieldname).startsWith('admin')) return pagesDir
  return uploadDir
}

function uploadPrefix(fieldname) {
  if (String(fieldname).startsWith('hero')) return 'h'
  if (String(fieldname).startsWith('foot')) return 'f'
  if (String(fieldname).startsWith('post')) return 'b'
  if (String(fieldname).startsWith('about') || String(fieldname).startsWith('admin')) return 'a'
  return 'p'
}

function defaultFooterGallery() {
  return DEFAULT_PROJECTS.slice(0, 6).map((p) => ({
    image: p.image,
    title: p.title,
    link: `/project/${p.slug}`,
  }))
}

const storage = multer.diskStorage({
  destination: (_req, file, cb) => cb(null, uploadFolder(file.fieldname)),
  filename: (_req, file, cb) => {
    const ext = path.extname(file.originalname || '.jpg') || '.jpg'
    cb(null, `${uploadPrefix(file.fieldname)}_${Date.now()}_${Math.random().toString(16).slice(2)}${ext}`)
  },
})
const upload = multer({ storage })

const app = express()
app.use(cors())
app.use(express.json({ limit: '8mb' }))
app.use('/uploads', express.static(path.join(__dirname, '..', 'uploads')))

const dbConfig = {
  host: process.env.DB_HOST || 'localhost',
  user: process.env.DB_USER || 'root',
  password: process.env.DB_PASS || '',
  multipleStatements: true,
}

let pool

function mapProject(row) {
  return {
    id: row.id,
    slug: row.slug,
    title: row.title,
    status: row.status,
    deal: row.deal,
    badge: row.badge,
    category: row.category,
    type: row.type,
    location: row.location,
    city: row.city,
    price: Number(row.price),
    beds: Number(row.beds),
    baths: Number(row.baths),
    rooms: Number(row.rooms),
    parking: Number(row.parking),
    size: Number(row.size),
    image: row.image,
    gallery: JSON.parse(row.gallery_json || '[]'),
    description: row.description,
    features: JSON.parse(row.features_json || '[]'),
    phone: row.phone,
    whatsapp: row.whatsapp,
  }
}

async function bearerUser(req) {
  const token = (req.headers.authorization || req.headers['x-auth-token'] || '').replace(/^Bearer\s+/i, '')
  if (!token) return null
  const [rows] = await pool.query(
    'SELECT user_id FROM tokens WHERE token = ? AND expires_at > NOW()',
    [token],
  )
  return rows[0] || null
}

function requireAuth(req, res, next) {
  bearerUser(req)
    .then((user) => {
      if (!user) return res.status(401).json({ error: 'Login required' })
      req.user = user
      next()
    })
    .catch(next)
}

async function bootstrap() {
  const root = await mysql.createConnection(dbConfig)
  await root.query('CREATE DATABASE IF NOT EXISTS safa_propertis DEFAULT CHARACTER SET utf8mb4')
  await root.end()

  pool = mysql.createPool({
    ...dbConfig,
    database: process.env.DB_NAME || 'safa_propertis',
  })

  const schema = fs.readFileSync(path.join(__dirname, '..', 'database', 'schema.sql'), 'utf8')
  const statements = schema
    .split(/;\s*$/m)
    .map((s) => s.trim())
    .filter((s) => s && !s.startsWith('CREATE DATABASE') && !s.startsWith('USE '))
  for (const sql of statements) {
    await pool.query(sql)
  }

  try {
    await pool.query('ALTER TABLE settings MODIFY setting_value TEXT NOT NULL')
  } catch {
    /* already TEXT or table missing on first create */
  }

  const [heroRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'hero_slides'")
  if (!heroRows[0]) {
    await pool.query('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)', [
      'hero_slides',
      JSON.stringify(HERO_SLIDES),
    ])
  }

  const [footerRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'footer_gallery'")
  if (!footerRows[0]) {
    await pool.query('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)', [
      'footer_gallery',
      JSON.stringify(defaultFooterGallery()),
    ])
  }

  const [pageRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'site_pages'")
  if (!pageRows[0]) {
    await pool.query('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)', [
      'site_pages',
      JSON.stringify(DEFAULT_PAGES),
    ])
  }

  const [users] = await pool.query('SELECT COUNT(*) AS c FROM users')
  if (!users[0].c) {
    const hash = await bcrypt.hash('safa123', 10)
    await pool.query('INSERT INTO users (username, password_hash) VALUES (?, ?)', ['admin', hash])
  }

  const [count] = await pool.query('SELECT COUNT(*) AS c FROM projects')
  if (!count[0].c) {
    for (const p of DEFAULT_PROJECTS) {
      await pool.query(
        `INSERT INTO projects
        (id, slug, title, status, deal, badge, category, type, location, city, price, beds, baths, rooms, parking, size, image, gallery_json, description, features_json, phone, whatsapp)
        VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)`,
        [
          p.id,
          p.slug,
          p.title,
          p.status,
          p.deal,
          p.badge,
          p.category,
          p.type,
          p.location,
          p.city,
          p.price,
          p.beds,
          p.baths,
          p.rooms,
          p.parking,
          p.size,
          p.image,
          JSON.stringify(p.gallery || []),
          p.description,
          JSON.stringify(p.features || []),
          p.phone || SITE.phone,
          p.whatsapp || SITE.whatsapp,
        ],
      )
    }
  }

  try {
    await pool.query('ALTER TABLE posts ADD COLUMN created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP')
  } catch {
    /* column already exists */
  }

  const [posts] = await pool.query('SELECT COUNT(*) AS c FROM posts')
  if (!posts[0].c) {
    for (const post of DEFAULT_POSTS) {
      await pool.query(
        'INSERT INTO posts (id, title, category, image, excerpt, body) VALUES (?,?,?,?,?,?)',
        [post.id, post.title, post.category, post.image, post.excerpt, post.body],
      )
    }
  }
}

function mapPost(row) {
  return {
    id: row.id,
    title: row.title,
    category: row.category,
    image: row.image,
    excerpt: row.excerpt,
    body: row.body,
  }
}

app.get('/api/posts', async (_req, res) => {
  const [rows] = await pool.query('SELECT * FROM posts ORDER BY created_at DESC, title ASC')
  res.json({ posts: rows.map(mapPost) })
})

app.get('/api/posts/:id', async (req, res) => {
  const [rows] = await pool.query('SELECT * FROM posts WHERE id = ?', [req.params.id])
  if (!rows[0]) return res.status(404).json({ error: 'Post not found' })
  res.json({ post: mapPost(rows[0]) })
})

app.post('/api/posts', requireAuth, upload.single('postImage'), async (req, res) => {
  const b = req.body
  const id = b.id || String(b.title || 'post').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '')
  let image = b.image || ''
  if (req.file) image = `/uploads/posts/${req.file.filename}`
  if (!image) {
    const [exist] = await pool.query('SELECT image FROM posts WHERE id = ?', [id])
    if (exist[0]?.image) image = exist[0].image
  }
  await pool.query(
    `INSERT INTO posts (id, title, category, image, excerpt, body)
     VALUES (?,?,?,?,?,?)
     ON DUPLICATE KEY UPDATE title=VALUES(title), category=VALUES(category), image=VALUES(image),
       excerpt=VALUES(excerpt), body=VALUES(body)`,
    [id, b.title || '', b.category || '', image, b.excerpt || '', b.body || ''],
  )
  res.json({ ok: true, id })
})

app.delete('/api/posts/:id', requireAuth, async (req, res) => {
  await pool.query('DELETE FROM posts WHERE id = ?', [req.params.id])
  res.json({ ok: true })
})

app.get('/api/projects', async (_req, res) => {
  const [rows] = await pool.query('SELECT * FROM projects ORDER BY created_at DESC')
  res.json({ projects: rows.map(mapProject) })
})

app.get('/api/projects/:slug', async (req, res) => {
  const [rows] = await pool.query('SELECT * FROM projects WHERE slug = ?', [req.params.slug])
  if (!rows[0]) return res.status(404).json({ error: 'Project not found' })
  res.json({ project: mapProject(rows[0]) })
})

app.post('/api/projects', requireAuth, upload.fields([
  { name: 'image', maxCount: 1 },
  { name: 'gallery', maxCount: 8 },
]), async (req, res) => {
  const b = req.body
  const id = b.id || String(b.title || 'project').toLowerCase().replace(/[^a-z0-9]+/g, '-')
  const slug = b.slug || id
  let image = b.image || ''
  if (req.files?.image?.[0]) image = `/uploads/projects/${req.files.image[0].filename}`
  if (!image) {
    const [exist] = await pool.query('SELECT image FROM projects WHERE id = ? OR slug = ?', [id, slug])
    if (exist[0]?.image) image = exist[0].image
  }
  let gallery = []
  try {
    gallery = JSON.parse(b.gallery || '[]')
  } catch {
    gallery = []
  }
  if (req.files?.gallery) {
    gallery = gallery.concat(req.files.gallery.map((f) => `/uploads/projects/${f.filename}`))
  }
  if (!image && gallery[0]) image = gallery[0]
  let features = []
  try {
    features = JSON.parse(b.features || '[]')
  } catch {
    features = String(b.features || '')
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
  }

  await pool.query(
    `INSERT INTO projects
    (id, slug, title, status, deal, badge, category, type, location, city, price, beds, baths, rooms, parking, size, image, gallery_json, description, features_json, phone, whatsapp)
    VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
    ON DUPLICATE KEY UPDATE title=VALUES(title), status=VALUES(status), deal=VALUES(deal), badge=VALUES(badge),
      category=VALUES(category), type=VALUES(type), location=VALUES(location), city=VALUES(city), price=VALUES(price),
      beds=VALUES(beds), baths=VALUES(baths), rooms=VALUES(rooms), parking=VALUES(parking), size=VALUES(size),
      image=VALUES(image), gallery_json=VALUES(gallery_json), description=VALUES(description), features_json=VALUES(features_json),
      phone=VALUES(phone), whatsapp=VALUES(whatsapp)`,
    [
      id, slug, b.title, b.status || 'ongoing', b.deal || 'buy', b.badge || '', b.category || '', b.type || '',
      b.location || '', b.city || '', Number(b.price) || 0, Number(b.beds) || 0, Number(b.baths) || 0,
      Number(b.rooms) || 0, Number(b.parking) || 0, Number(b.size) || 0, image, JSON.stringify(gallery),
      b.description || '', JSON.stringify(features), b.phone || SITE.phone, b.whatsapp || SITE.whatsapp,
    ],
  )
  res.json({ ok: true, id })
})

function parseHeroSlides(raw) {
  try {
    const slides = JSON.parse(raw || '[]')
    if (Array.isArray(slides) && slides.length) return slides
  } catch {
    /* use default */
  }
  return HERO_SLIDES
}

app.get('/api/hero', async (_req, res) => {
  const [rows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'hero_slides'")
  res.json({ slides: parseHeroSlides(rows[0]?.setting_value) })
})

app.post('/api/hero', requireAuth, upload.any(), async (req, res) => {
  let slides = []
  try {
    slides = JSON.parse(req.body.slides || '[]')
  } catch {
    slides = []
  }
  if (!Array.isArray(slides) || !slides.length) {
    return res.status(400).json({ error: 'At least one slide is required' })
  }
  for (const file of req.files || []) {
    const match = String(file.fieldname).match(/^hero(\d+)$/)
    if (!match) continue
    const index = Number(match[1])
    if (!slides[index]) continue
    slides[index].image = `/uploads/hero/${file.filename}`
  }
  const cleaned = slides.slice(0, 8).map((slide, i) => ({
    id: slide.id || `slide-${i + 1}`,
    image: slide.image || '',
    eyebrow: slide.eyebrow || '',
    title: slide.title || '',
    text: slide.text || '',
  }))
  await pool.query(
    `INSERT INTO settings (setting_key, setting_value) VALUES ('hero_slides', ?)
     ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
    [JSON.stringify(cleaned)],
  )
  res.json({ ok: true, slides: cleaned })
})

function parseFooterGallery(raw) {
  try {
    const items = JSON.parse(raw || '[]')
    if (Array.isArray(items) && items.length) return items
  } catch {
    /* use default */
  }
  return defaultFooterGallery()
}

app.get('/api/footer-gallery', async (_req, res) => {
  const [rows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'footer_gallery'")
  res.json({ items: parseFooterGallery(rows[0]?.setting_value) })
})

app.post('/api/footer-gallery', requireAuth, upload.any(), async (req, res) => {
  let items = []
  try {
    items = JSON.parse(req.body.items || '[]')
  } catch {
    items = []
  }
  if (!Array.isArray(items)) items = []
  for (const file of req.files || []) {
    const match = String(file.fieldname).match(/^foot(\d+)$/)
    if (!match) continue
    const index = Number(match[1])
    if (!items[index]) items[index] = { image: '', title: '', link: '/project' }
    items[index].image = `/uploads/gallery/${file.filename}`
  }
  const cleaned = items.slice(0, 6).map((item, i) => ({
    image: item.image || '',
    title: item.title || `Gallery ${i + 1}`,
    link: item.link || '/project',
  })).filter((item) => item.image)
  await pool.query(
    `INSERT INTO settings (setting_key, setting_value) VALUES ('footer_gallery', ?)
     ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
    [JSON.stringify(cleaned)],
  )
  res.json({ ok: true, items: cleaned })
})

app.delete('/api/projects/:id', requireAuth, async (req, res) => {
  await pool.query('DELETE FROM projects WHERE id = ? OR slug = ?', [req.params.id, req.params.id])
  res.json({ ok: true })
})

app.post('/api/auth/login', async (req, res) => {
  const [rows] = await pool.query('SELECT * FROM users WHERE username = ?', [req.body.username])
  const user = rows[0]
  if (!user || !(await bcrypt.compare(req.body.password || '', user.password_hash))) {
    return res.status(401).json({ error: 'Wrong username or password.' })
  }
  const token = crypto.randomBytes(24).toString('hex')
  await pool.query(
    'INSERT INTO tokens (token, user_id, expires_at) VALUES (?, ?, DATE_ADD(NOW(), INTERVAL 7 DAY))',
    [token, user.id],
  )
  res.json({ token })
})

app.get('/api/auth/me', requireAuth, (_req, res) => res.json({ ok: true }))

app.post('/api/auth/logout', async (req, res) => {
  const token = (req.headers.authorization || '').replace(/^Bearer\s+/i, '')
  if (token) await pool.query('DELETE FROM tokens WHERE token = ?', [token])
  res.json({ ok: true })
})

app.post('/api/messages', async (req, res) => {
  const { name, phone = '', email = '', address = '', message = '' } = req.body || {}
  if (!name || !message) return res.status(400).json({ error: 'Name and message required' })
  await pool.query(
    'INSERT INTO messages (name, phone, email, address, message) VALUES (?,?,?,?,?)',
    [name, phone, email, address, message],
  )
  res.json({ ok: true })
})

app.get('/api/messages', requireAuth, async (_req, res) => {
  const [rows] = await pool.query(
    'SELECT id, name, phone, email, address, message, created_at FROM messages ORDER BY created_at DESC, id DESC',
  )
  res.json({ messages: rows })
})

app.delete('/api/messages/:id', requireAuth, async (req, res) => {
  await pool.query('DELETE FROM messages WHERE id = ?', [req.params.id])
  res.json({ ok: true })
})

app.post('/api/subscribers', async (req, res) => {
  const email = String(req.body?.email || '').trim().toLowerCase()
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    return res.status(400).json({ error: 'Valid email required' })
  }
  try {
    await pool.query('INSERT INTO subscribers (email) VALUES (?)', [email])
  } catch (err) {
    if (err.code !== 'ER_DUP_ENTRY') throw err
  }
  res.json({ ok: true })
})

app.get('/api/subscribers', requireAuth, async (_req, res) => {
  const [rows] = await pool.query(
    'SELECT id, email, created_at FROM subscribers ORDER BY created_at DESC, id DESC',
  )
  res.json({ subscribers: rows })
})

app.delete('/api/subscribers/:id', requireAuth, async (req, res) => {
  await pool.query('DELETE FROM subscribers WHERE id = ?', [req.params.id])
  res.json({ ok: true })
})

app.get('/api/settings', async (_req, res) => {
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

app.post('/api/socials', requireAuth, async (req, res) => {
  const body = req.body || {}
  const up =
    'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
  for (const key of ['facebook', 'x', 'linkedin', 'instagram', 'whatsapp']) {
    await pool.query(up, [`social_${key}`, String(body[key] || '').trim()])
  }
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ ok: true, settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

app.post('/api/profile', requireAuth, upload.single('adminPhoto'), async (req, res) => {
  const name = String(req.body.admin_name || '').trim()
  const address = String(req.body.admin_address || '').trim()
  let photo = String(req.body.admin_photo || '').trim()
  const currentPass = String(req.body.current_password || '')
  const newPass = String(req.body.new_password || '')
  const confirmPass = String(req.body.confirm_password || '')
  if (req.file) photo = `/uploads/pages/${req.file.filename}`
  if (currentPass || newPass || confirmPass) {
    if (newPass.length < 6) return res.status(400).json({ error: 'New password must be at least 6 characters.' })
    if (newPass !== confirmPass) return res.status(400).json({ error: 'New passwords do not match.' })
    const [users] = await pool.query('SELECT password_hash FROM users WHERE id = ?', [req.user.user_id])
    const user = users[0]
    if (!user || !(await bcrypt.compare(currentPass, user.password_hash))) {
      return res.status(400).json({ error: 'Current password is wrong.' })
    }
    await pool.query('UPDATE users SET password_hash = ? WHERE id = ?', [await bcrypt.hash(newPass, 10), req.user.user_id])
  }
  const up = 'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
  await pool.query(up, ['admin_name', name])
  await pool.query(up, ['admin_address', address])
  if (photo) await pool.query(up, ['admin_photo', photo])
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ ok: true, settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

function mergePages(raw) {
  let parsed = {}
  try {
    parsed = JSON.parse(raw || '{}') || {}
  } catch {
    parsed = {}
  }
  return {
    about: { ...DEFAULT_PAGES.about, ...(parsed.about || {}) },
    contact: { ...DEFAULT_PAGES.contact, ...(parsed.contact || {}) },
  }
}

app.get('/api/pages', async (_req, res) => {
  const [rows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'site_pages'")
  res.json({ pages: mergePages(rows[0]?.setting_value) })
})

app.post('/api/pages', requireAuth, upload.single('aboutImage'), async (req, res) => {
  let incoming = {}
  try {
    incoming = JSON.parse(req.body.pages || '{}')
  } catch {
    incoming = {}
  }
  const pages = mergePages(JSON.stringify(incoming))
  if (req.file) pages.about.image = `/uploads/pages/${req.file.filename}`
  if (Array.isArray(pages.about.features)) {
    pages.about.features = pages.about.features.map((item) => String(item).trim()).filter(Boolean)
  } else {
    pages.about.features = String(pages.about.features || '')
      .split('\n')
      .map((item) => item.trim())
      .filter(Boolean)
  }
  await pool.query(
    `INSERT INTO settings (setting_key, setting_value) VALUES ('site_pages', ?)
     ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
    [JSON.stringify(pages)],
  )
  for (const key of ['phone', 'whatsapp', 'email', 'address']) {
    if (!pages.contact[key]) continue
    await pool.query(
      `INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)
       ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
      [key, pages.contact[key]],
    )
  }
  res.json({ ok: true, pages })
})

app.use((err, _req, res, _next) => {
  console.error(err)
  res.status(500).json({ error: err.message || 'Server error' })
})

const port = Number(process.env.PORT || 3001)

bootstrap()
  .then(() => {
    app.listen(port, () => {
      console.log(`MySQL API running on http://localhost:${port}`)
    })
  })
  .catch((err) => {
    console.error('MySQL bootstrap failed.')
    console.error(err.message)
    console.error('Create a database user, set .env, then run: npm run server')
    process.exit(1)
  })
