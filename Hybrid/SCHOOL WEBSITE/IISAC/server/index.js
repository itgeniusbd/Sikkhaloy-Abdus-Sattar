import './loadEnv.js'
import { passwordPolicyError } from './passwordPolicy.js'
import { rateLimit, eduLog } from './rateLimit.js'
import cors from 'cors'
import crypto from 'node:crypto'
import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import bcrypt from 'bcryptjs'
import dotenv from 'dotenv'
import express from 'express'
import { upload, mapUploadedFiles } from './cloudinaryUpload.js'
import mysql from 'mysql2/promise'
import { DEFAULT_CMS_PAGES, DEFAULT_MENU } from '../src/data/menu.js'
import { DEFAULT_POSTS, DEFAULT_PROJECTS } from '../src/data/projects.js'
import { DEFAULT_PAGES, HERO_SLIDES, SITE } from '../src/data/site.js'

dotenv.config()

const __dirname = path.dirname(fileURLToPath(import.meta.url))
// Local uploads kept for fallback when Cloudinary env is not set
for (const folder of ['projects', 'hero', 'gallery', 'posts', 'pages', 'admissions', 'careers', 'jobs']) {
  fs.mkdirSync(path.join(__dirname, '..', 'uploads', folder), { recursive: true })
}

function withUploads(middleware) {
  return [
    middleware,
    async (req, res, next) => {
      try {
        await mapUploadedFiles(req)
        next()
      } catch (err) {
        console.error('Upload failed', err)
        res.status(500).json({ error: err.message || 'Upload failed' })
      }
    },
  ]
}

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

const ALL_ACCESS = [
  'profile',
  'menus',
  'slider',
  'admissions',
  'footer',
  'blog',
  'about',
  'messages',
  'subscribers',
  'socials',
  'contact',
  'career',
]

function parsePermissions(raw, role) {
  if (role === 'admin') return ALL_ACCESS.slice()
  try {
    const list = JSON.parse(raw || '[]')
    return Array.isArray(list) ? list.filter((item) => ALL_ACCESS.includes(item)) : []
  } catch {
    return []
  }
}

function publicUser(row) {
  if (!row) return null
  const role = row.role === 'admin' ? 'admin' : 'subadmin'
  return {
    id: Number(row.id),
    username: row.username,
    displayName: row.display_name || row.username,
    designation: row.designation || '',
    photo: row.photo || '',
    role,
    permissions: parsePermissions(row.permissions, role),
    active: Number(row.active ?? 1) !== 0,
  }
}

function hasPerm(user, ...perms) {
  if (!user) return false
  if (user.role === 'admin') return true
  const list = user.permissions || []
  return perms.some((perm) => perm === 'profile' || list.includes(perm))
}

async function bearerUser(req) {
  const token = (req.headers.authorization || req.headers['x-auth-token'] || '').replace(/^Bearer\s+/i, '')
  if (!token) return null
  const [rows] = await pool.query(
    `SELECT u.id, u.username, u.role, u.display_name, u.designation, u.photo, u.permissions, u.active, t.user_id
     FROM tokens t
     INNER JOIN users u ON u.id = t.user_id
     WHERE t.token = ? AND t.expires_at > NOW()`,
    [token],
  )
  const row = rows[0]
  if (!row || Number(row.active) === 0) return null
  return {
    user_id: row.id,
    ...publicUser(row),
  }
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

function requirePerm(...perms) {
  return (req, res, next) => {
    if (!req.user) return res.status(401).json({ error: 'Login required' })
    if (hasPerm(req.user, ...perms)) return next()
    return res.status(403).json({ error: 'No access.' })
  }
}

function requireAdmin(req, res, next) {
  if (req.user?.role !== 'admin') return res.status(403).json({ error: 'Only the main admin can manage users.' })
  next()
}

async function ensureUserColumns() {
  const alters = [
    "ALTER TABLE users ADD COLUMN role VARCHAR(20) NOT NULL DEFAULT 'admin'",
    "ALTER TABLE users ADD COLUMN display_name VARCHAR(120) NOT NULL DEFAULT ''",
    'ALTER TABLE users ADD COLUMN permissions TEXT',
    'ALTER TABLE users ADD COLUMN active TINYINT NOT NULL DEFAULT 1',
    "ALTER TABLE users ADD COLUMN photo VARCHAR(255) NOT NULL DEFAULT ''",
    "ALTER TABLE users ADD COLUMN designation VARCHAR(160) NOT NULL DEFAULT ''",
  ]
  for (const sql of alters) {
    try {
      await pool.query(sql)
    } catch {
      /* column exists */
    }
  }
  const [rows] = await pool.query('SELECT id FROM users ORDER BY id ASC LIMIT 1')
  if (rows[0]) {
    await pool.query(
      "UPDATE users SET role = 'admin', display_name = IF(display_name = '', username, display_name) WHERE id = ?",
      [rows[0].id],
    )
    const [photoRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'admin_photo'")
    const sitePhoto = photoRows[0]?.setting_value || ''
    if (sitePhoto) {
      await pool.query("UPDATE users SET photo = ? WHERE id = ? AND (photo IS NULL OR photo = '')", [
        sitePhoto,
        rows[0].id,
      ])
    }
  }
}

async function bootstrap() {
  const root = await mysql.createConnection(dbConfig)
  await root.query('CREATE DATABASE IF NOT EXISTS iisac_website DEFAULT CHARACTER SET utf8mb4')
  await root.end()

  pool = mysql.createPool({
    ...dbConfig,
    database: process.env.DB_NAME || 'iisac_website',
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
    await pool.query('ALTER TABLE settings MODIFY setting_value MEDIUMTEXT NOT NULL')
  } catch {
    /* already wide enough or table missing on first create */
  }

  await pool.query(`CREATE TABLE IF NOT EXISTS admissions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    students_name VARCHAR(160) NOT NULL,
    sms_phone VARCHAR(20) NOT NULL,
    gender VARCHAR(20) DEFAULT 'Male',
    fathers_name VARCHAR(160) NOT NULL,
    education_year_id INT NOT NULL,
    education_year VARCHAR(80) DEFAULT '',
    class_id INT NOT NULL,
    class_name VARCHAR(80) DEFAULT '',
    group_id INT DEFAULT 0,
    group_name VARCHAR(80) DEFAULT '',
    section_id INT DEFAULT 0,
    section_name VARCHAR(80) DEFAULT '',
    shift_id INT DEFAULT 0,
    shift_name VARCHAR(80) DEFAULT '',
    roll_no VARCHAR(40) DEFAULT '',
    student_email VARCHAR(160) DEFAULT '',
    date_of_birth VARCHAR(20) DEFAULT '',
    legal_identity VARCHAR(80) DEFAULT '',
    blood_group VARCHAR(20) DEFAULT '',
    religion VARCHAR(40) DEFAULT '',
    permanent_address TEXT,
    local_address TEXT,
    photo VARCHAR(255) DEFAULT '',
    mothers_name VARCHAR(160) DEFAULT '',
    mother_phone VARCHAR(20) DEFAULT '',
    mother_occupation VARCHAR(120) DEFAULT '',
    father_phone VARCHAR(20) DEFAULT '',
    father_occupation VARCHAR(120) DEFAULT '',
    guardian_name VARCHAR(160) DEFAULT '',
    guardian_relation VARCHAR(80) DEFAULT '',
    guardian_phone VARCHAR(20) DEFAULT '',
    prev_school VARCHAR(200) DEFAULT '',
    prev_class VARCHAR(80) DEFAULT '',
    prev_exam_year VARCHAR(20) DEFAULT '',
    prev_grade VARCHAR(40) DEFAULT '',
    subjects_json TEXT,
    assigned_id VARCHAR(40) DEFAULT '',
    sikkhaloy_student_id INT DEFAULT NULL,
    confirm_error TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    confirmed_at DATETIME NULL
  )`)
  try {
    await pool.query('ALTER TABLE admissions ADD COLUMN payments_json TEXT')
  } catch {
    /* already present */
  }

  await pool.query(`CREATE TABLE IF NOT EXISTS jobs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    title_bn VARCHAR(200) DEFAULT '',
    department VARCHAR(120) DEFAULT '',
    location VARCHAR(160) DEFAULT '',
    job_type VARCHAR(40) DEFAULT 'Full-time',
    deadline VARCHAR(20) DEFAULT '',
    description TEXT,
    description_bn TEXT,
    published TINYINT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  )`)
  try {
    await pool.query("ALTER TABLE jobs ADD COLUMN image VARCHAR(255) DEFAULT ''")
  } catch {
    /* already present */
  }
  await pool.query(`CREATE TABLE IF NOT EXISTS job_applications (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL,
    name VARCHAR(160) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(160) DEFAULT '',
    address TEXT,
    education TEXT,
    experience TEXT,
    cover_letter TEXT,
    cv VARCHAR(255) DEFAULT '',
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  )`)

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

  const [menuRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'site_menus'")
  if (!menuRows[0]) {
    await pool.query('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)', [
      'site_menus',
      JSON.stringify(DEFAULT_MENU),
    ])
  }
  const [cmsRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'cms_pages'")
  if (!cmsRows[0]) {
    await pool.query('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?)', [
      'cms_pages',
      JSON.stringify(DEFAULT_CMS_PAGES),
    ])
  }

  await ensureUserColumns()

  const [users] = await pool.query('SELECT COUNT(*) AS c FROM users')
  if (!users[0].c) {
    const hash = await bcrypt.hash('iisac123', 10)
    await pool.query(
      'INSERT INTO users (username, password_hash, role, display_name, permissions, active) VALUES (?, ?, ?, ?, ?, 1)',
      ['admin', hash, 'admin', 'Admin', JSON.stringify(ALL_ACCESS)],
    )
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

app.post('/api/posts', requireAuth, requirePerm('blog'), ...withUploads(upload.single('postImage')), async (req, res) => {
  const b = req.body
  const id = b.id || String(b.title || 'post').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '')
  let image = b.image || ''
  if (req.file) image = req.file.publicUrl
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

app.delete('/api/posts/:id', requireAuth, requirePerm('blog'), async (req, res) => {
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

app.post('/api/projects', requireAuth, ...withUploads(upload.fields([
  { name: 'image', maxCount: 1 },
  { name: 'gallery', maxCount: 8 },
])), async (req, res) => {
  const b = req.body
  const id = b.id || String(b.title || 'project').toLowerCase().replace(/[^a-z0-9]+/g, '-')
  const slug = b.slug || id
  let image = b.image || ''
  if (req.files?.image?.[0]) image = req.files.image[0].publicUrl
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
    gallery = gallery.concat(req.files.gallery.map((f) => f.publicUrl))
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

app.post('/api/hero', requireAuth, requirePerm('slider'), ...withUploads(upload.any()), async (req, res) => {
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
    slides[index].image = file.publicUrl
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

app.post('/api/footer-gallery', requireAuth, requirePerm('footer'), ...withUploads(upload.any()), async (req, res) => {
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
    items[index].image = file.publicUrl
  }
  const cleaned = items.slice(0, 48).map((item, i) => ({
    image: item.image || '',
    title: item.title || `Gallery ${i + 1}`,
    link: item.link || '/gallery',
  })).filter((item) => item.image)
  await pool.query(
    `INSERT INTO settings (setting_key, setting_value) VALUES ('footer_gallery', ?)
     ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
    [JSON.stringify(cleaned)],
  )
  res.json({ ok: true, items: cleaned })
})

app.get('/api/videos', async (_req, res) => {
  const [rows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'video_gallery'")
  res.json({ videos: parseSettingJson(rows[0]?.setting_value, []) })
})

app.post('/api/videos', requireAuth, requirePerm('footer'), async (req, res) => {
  let items = req.body?.videos
  if (typeof items === 'string') {
    try {
      items = JSON.parse(items)
    } catch {
      items = []
    }
  }
  if (!Array.isArray(items)) items = []
  const cleaned = items
    .map((item, i) => ({
      id: item.id || `v-${i + 1}`,
      url: String(item.url || '').trim(),
      title: item.title || '',
      titleBn: item.titleBn || '',
    }))
    .filter((item) => item.url)
    .slice(0, 24)
  await pool.query(
    `INSERT INTO settings (setting_key, setting_value) VALUES ('video_gallery', ?)
     ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
    [JSON.stringify(cleaned)],
  )
  res.json({ ok: true, videos: cleaned })
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
  if (Number(user.active) === 0) {
    return res.status(403).json({ error: 'This account is disabled.' })
  }
  const token = crypto.randomBytes(24).toString('hex')
  await pool.query(
    'INSERT INTO tokens (token, user_id, expires_at) VALUES (?, ?, DATE_ADD(NOW(), INTERVAL 7 DAY))',
    [token, user.id],
  )
  res.json({ token, user: publicUser(user) })
})

app.get('/api/auth/me', requireAuth, (req, res) => res.json({ ok: true, user: req.user }))

app.post('/api/auth/logout', async (req, res) => {
  const token = (req.headers.authorization || '').replace(/^Bearer\s+/i, '')
  if (token) await pool.query('DELETE FROM tokens WHERE token = ?', [token])
  res.json({ ok: true })
})

app.get('/api/users', requireAuth, requireAdmin, async (_req, res) => {
  const [rows] = await pool.query(
    'SELECT id, username, role, display_name, designation, photo, permissions, active, created_at FROM users ORDER BY id ASC',
  )
  res.json({ users: rows.map(publicUser) })
})

app.post('/api/users', requireAuth, requireAdmin, async (req, res) => {
  const username = String(req.body?.username || '').trim().toLowerCase()
  const displayName = String(req.body?.displayName || req.body?.display_name || username).trim()
  const designation = String(req.body?.designation || '').trim()
  const password = String(req.body?.password || '')
  const permissions = parsePermissions(JSON.stringify(req.body?.permissions || []), 'subadmin')
  const active = req.body?.active === false || req.body?.active === 0 ? 0 : 1
  if (!/^[a-z0-9._-]{3,40}$/.test(username)) {
    return res.status(400).json({ error: 'Username must be 3-40 letters, numbers, or . _ -' })
  }
  {
    const pwErr = passwordPolicyError(password)
    if (pwErr) return res.status(400).json({ error: pwErr })
  }
  try {
    const [result] = await pool.query(
      'INSERT INTO users (username, password_hash, role, display_name, designation, permissions, active) VALUES (?, ?, ?, ?, ?, ?, ?)',
      [username, await bcrypt.hash(password, 10), 'subadmin', displayName || username, designation, JSON.stringify(permissions), active],
    )
    const [rows] = await pool.query(
      'SELECT id, username, role, display_name, designation, photo, permissions, active, created_at FROM users WHERE id = ?',
      [result.insertId],
    )
    res.json({ ok: true, user: publicUser(rows[0]) })
  } catch (err) {
    if (err.code === 'ER_DUP_ENTRY') return res.status(400).json({ error: 'Username already exists.' })
    throw err
  }
})

async function updateSubAdmin(req, res) {
  const id = Number(req.params.id)
  const [rows] = await pool.query('SELECT * FROM users WHERE id = ?', [id])
  const user = rows[0]
  if (!user) return res.status(404).json({ error: 'User not found.' })
  if (user.role === 'admin') return res.status(400).json({ error: 'The main admin account cannot be edited here.' })
  const displayName = String(req.body?.displayName || req.body?.display_name || user.display_name || user.username).trim()
  const designation = String(req.body?.designation ?? user.designation ?? '').trim()
  const permissions = parsePermissions(JSON.stringify(req.body?.permissions || []), 'subadmin')
  const active = req.body?.active === false || req.body?.active === 0 ? 0 : 1
  const password = String(req.body?.password || '')
  if (password) {
    const pwErr = passwordPolicyError(password)
    if (pwErr) return res.status(400).json({ error: pwErr })
  }
  if (password) {
    await pool.query(
      'UPDATE users SET display_name = ?, designation = ?, permissions = ?, active = ?, password_hash = ? WHERE id = ?',
      [displayName || user.username, designation, JSON.stringify(permissions), active, await bcrypt.hash(password, 10), id],
    )
  } else {
    await pool.query('UPDATE users SET display_name = ?, designation = ?, permissions = ?, active = ? WHERE id = ?', [
      displayName || user.username,
      designation,
      JSON.stringify(permissions),
      active,
      id,
    ])
  }
  if (!active) await pool.query('DELETE FROM tokens WHERE user_id = ?', [id])
  const [next] = await pool.query(
    'SELECT id, username, role, display_name, designation, photo, permissions, active, created_at FROM users WHERE id = ?',
    [id],
  )
  res.json({ ok: true, user: publicUser(next[0]) })
}

app.put('/api/users/:id', requireAuth, requireAdmin, updateSubAdmin)
app.post('/api/users/:id', requireAuth, requireAdmin, updateSubAdmin)

app.delete('/api/users/:id', requireAuth, requireAdmin, async (req, res) => {
  const id = Number(req.params.id)
  const [rows] = await pool.query('SELECT role FROM users WHERE id = ?', [id])
  if (!rows[0]) return res.status(404).json({ error: 'User not found.' })
  if (rows[0].role === 'admin') return res.status(400).json({ error: 'The main admin account cannot be deleted.' })
  await pool.query('DELETE FROM users WHERE id = ?', [id])
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

app.get('/api/messages', requireAuth, requirePerm('messages'), async (_req, res) => {
  const [rows] = await pool.query(
    'SELECT id, name, phone, email, address, message, created_at FROM messages ORDER BY created_at DESC, id DESC',
  )
  res.json({ messages: rows })
})

app.delete('/api/messages/:id', requireAuth, requirePerm('messages'), async (req, res) => {
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

app.get('/api/subscribers', requireAuth, requirePerm('subscribers'), async (_req, res) => {
  const [rows] = await pool.query(
    'SELECT id, email, created_at FROM subscribers ORDER BY created_at DESC, id DESC',
  )
  res.json({ subscribers: rows })
})

app.delete('/api/subscribers/:id', requireAuth, requirePerm('subscribers'), async (req, res) => {
  await pool.query('DELETE FROM subscribers WHERE id = ?', [req.params.id])
  res.json({ ok: true })
})

app.get('/api/settings', async (_req, res) => {
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

function themeHex(value, fallback) {
  const raw = String(value || '').trim()
  return /^#[0-9a-fA-F]{6}$/.test(raw) ? raw.toLowerCase() : fallback
}

app.post('/api/theme', requireAuth, async (req, res) => {
  const body = req.body || {}
  const primary = themeHex(body.primary || body.theme_primary, '#f58634')
  const button = themeHex(body.button || body.theme_button, primary)
  const menuHover = themeHex(body.menuHover || body.theme_menu_hover, primary)
  const sidebar = themeHex(body.sidebar || body.theme_sidebar, '#141414')
  const menuBg = themeHex(body.menuBg || body.theme_menu_bg, '#252525')
  const navBg = themeHex(body.navBg || body.theme_nav_bg, '#fff4ea')
  const brand = themeHex(body.brand || body.theme_brand, '#161616')
  const headerBg = themeHex(body.headerBg || body.theme_header_bg, '#ffffff')
  const headerMode = String(body.headerMode || body.theme_header_mode || 'light').toLowerCase() === 'dark' ? 'dark' : 'light'
  const siteName = String(body.siteName || body.site_name || '').trim()
  const siteDialog = String(body.siteDialog || body.site_dialog || '').trim()
  const up =
    'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
  await pool.query(up, ['theme_primary', primary])
  await pool.query(up, ['theme_button', button])
  await pool.query(up, ['theme_menu_hover', menuHover])
  await pool.query(up, ['theme_sidebar', sidebar])
  await pool.query(up, ['theme_menu_bg', menuBg])
  await pool.query(up, ['theme_nav_bg', navBg])
  await pool.query(up, ['theme_brand', brand])
  await pool.query(up, ['theme_header_bg', headerBg])
  await pool.query(up, ['theme_header_mode', headerMode])
  await pool.query(up, ['site_name', siteName])
  await pool.query(up, ['site_dialog', siteDialog])
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ ok: true, settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

app.post('/api/socials', requireAuth, requirePerm('socials'), async (req, res) => {
  const body = req.body || {}
  const up =
    'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
  for (const key of ['facebook', 'x', 'linkedin', 'instagram', 'whatsapp']) {
    await pool.query(up, [`social_${key}`, String(body[key] || '').trim()])
  }
  await pool.query(up, ['pay_online', String(body.payOnline || body.pay_online || '').trim()])
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  res.json({ ok: true, settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])) })
})

app.post('/api/profile', requireAuth, ...withUploads(upload.single('adminPhoto')), async (req, res) => {
  const name = String(req.body.admin_name || '').trim()
  const designation = String(req.body.designation || req.body.admin_designation || '').trim()
  const address = String(req.body.admin_address || '').trim()
  let photo = String(req.body.admin_photo || '').trim()
  const currentPass = String(req.body.current_password || '')
  const newPass = String(req.body.new_password || '')
  const confirmPass = String(req.body.confirm_password || '')
  if (req.file) photo = req.file.publicUrl
  if (currentPass || newPass || confirmPass) {
    {
      const pwErr = passwordPolicyError(newPass)
      if (pwErr) return res.status(400).json({ error: pwErr })
    }
    if (newPass !== confirmPass) return res.status(400).json({ error: 'New passwords do not match.' })
    const [users] = await pool.query('SELECT password_hash FROM users WHERE id = ?', [req.user.user_id])
    const user = users[0]
    if (!user || !(await bcrypt.compare(currentPass, user.password_hash))) {
      return res.status(400).json({ error: 'Current password is wrong.' })
    }
    await pool.query('UPDATE users SET password_hash = ? WHERE id = ?', [await bcrypt.hash(newPass, 10), req.user.user_id])
  }
  if (photo) {
    await pool.query('UPDATE users SET display_name = ?, designation = ?, photo = ? WHERE id = ?', [
      name || req.user.username,
      designation,
      photo,
      req.user.user_id,
    ])
  } else {
    await pool.query('UPDATE users SET display_name = ?, designation = ? WHERE id = ?', [
      name || req.user.username,
      designation,
      req.user.user_id,
    ])
  }
  if (req.user.role === 'admin') {
    const up = 'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
    await pool.query(up, ['admin_name', name])
    await pool.query(up, ['admin_address', address])
    if (photo) await pool.query(up, ['admin_photo', photo])
  }
  const [rows] = await pool.query('SELECT setting_key, setting_value FROM settings')
  const [fresh] = await pool.query('SELECT id, username, role, display_name, designation, photo, permissions, active FROM users WHERE id = ?', [req.user.user_id])
  res.json({
    ok: true,
    settings: Object.fromEntries(rows.map((r) => [r.setting_key, r.setting_value])),
    user: publicUser(fresh[0]),
  })
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

app.post('/api/pages', requireAuth, requirePerm('about', 'contact'), ...withUploads(upload.single('aboutImage')), async (req, res) => {
  let incoming = {}
  try {
    incoming = JSON.parse(req.body.pages || '{}')
  } catch {
    incoming = {}
  }
  const pages = mergePages(JSON.stringify(incoming))
  if (req.file) pages.about.image = req.file.publicUrl
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
  const siteName = String(pages.contact?.siteName || pages.contact?.site_name || '').trim()
  if (siteName) {
    await pool.query(
      `INSERT INTO settings (setting_key, setting_value) VALUES ('site_name', ?)
       ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)`,
      [siteName],
    )
  }
  res.json({ ok: true, pages })
})

function parseSettingJson(raw, fallback) {
  try {
    const parsed = JSON.parse(raw || '')
    return parsed == null ? fallback : parsed
  } catch {
    return fallback
  }
}

async function readMenus() {
  const [menuRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'site_menus'")
  const [pageRows] = await pool.query("SELECT setting_value FROM settings WHERE setting_key = 'cms_pages'")
  const menus = parseSettingJson(menuRows[0]?.setting_value, null)
  const pages = parseSettingJson(pageRows[0]?.setting_value, {})
  return {
    menus: Array.isArray(menus) && menus.length ? menus : DEFAULT_MENU,
    pages: { ...DEFAULT_CMS_PAGES, ...(pages && typeof pages === 'object' && !Array.isArray(pages) ? pages : {}) },
  }
}

app.get('/api/menus', async (_req, res) => {
  res.json(await readMenus())
})

app.post('/api/menus', requireAuth, requirePerm('menus'), ...withUploads(upload.any()), async (req, res) => {
  let menus = DEFAULT_MENU
  let pages = { ...DEFAULT_CMS_PAGES }
  try {
    const incoming = JSON.parse(req.body.menus || '[]')
    if (Array.isArray(incoming) && incoming.length) menus = incoming
  } catch {
    /* keep defaults */
  }
  try {
    const incoming = JSON.parse(req.body.pages || '{}')
    if (incoming && typeof incoming === 'object' && !Array.isArray(incoming)) {
      pages = { ...DEFAULT_CMS_PAGES, ...incoming }
    }
  } catch {
    /* keep defaults */
  }
  const slug = String(req.body.pageSlug || '').trim()
  const files = req.files || []
  const pageImage = files.find((file) => file.fieldname === 'pageImage')
  if (pageImage && slug) {
    pages[slug] = { ...(pages[slug] || {}), image: pageImage.publicUrl }
  }
  files.forEach((file) => {
    const match = /^member_(.+)$/.exec(file.fieldname || '')
    if (!match) return
    const memberId = match[1]
    const targetSlug =
      slug ||
      Object.keys(pages).find((key) => (pages[key]?.members || []).some((member) => member.id === memberId))
    if (!targetSlug || !pages[targetSlug]) return
    pages[targetSlug].members = (pages[targetSlug].members || []).map((member) =>
      member.id === memberId ? { ...member, image: file.publicUrl } : member,
    )
  })
  const up =
    'INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)'
  await pool.query(up, ['site_menus', JSON.stringify(menus)])
  await pool.query(up, ['cms_pages', JSON.stringify(pages)])
  res.json({ ok: true, menus, pages })
})

const eduApi = String(process.env.SIKKHALOY_API || 'https://hybridapi.sikkhaloy.com').replace(/\/$/, '')
const eduApiLocal = String(process.env.SIKKHALOY_API_LOCAL || '').replace(/\/$/, '')
const eduSchoolId = Number(process.env.SCHOOL_ID || 1002)
const eduApiBases = [...new Set([eduApi, eduApiLocal].filter(Boolean))]

function mapEduPerson(row, kind) {
  const id = Number(row?.id || row?.Id || 0)
  const photo = row?.photoUrl || row?.PhotoUrl
  const role = String(row?.kind || row?.Kind || kind || 'student').toLowerCase()
  return {
    id,
    name: row?.name || row?.Name || '',
    title: row?.title || row?.Title || '',
    meta: row?.meta || row?.Meta || '',
    kind: role,
    photoUrl: photo ? `/api/edu/${role}-photo/${id}` : '',
  }
}

async function eduFetch(path) {
  const key = process.env.SIKKHALOY_WEBSITE_KEY || ''
  let lastError = new Error(`Sikkhaloy API failed ${path}`)
  for (const base of eduApiBases) {
    try {
      const res = await fetch(`${base}${path}`, {
        headers: {
          ...(key ? { 'X-Website-Key': key } : {}),
        },
      })
      if (res.ok) return res
      lastError = new Error(`Sikkhaloy API ${res.status}`)
    } catch (err) {
      lastError = err
    }
  }
  throw lastError
}

function mapEduHome(data, fallbackSchool) {
  const school = data.school || data.School || fallbackSchool || {}
  const stats = data.stats || data.Stats || {}
  const classes = stats.classes || stats.Classes || []
  const hasLogo = Boolean(school.logoUrl || school.LogoUrl || school.hasLogo || school.HasLogo || fallbackSchool?.hasLogo)
  return {
    school: {
      schoolID: Number(school.schoolID || school.SchoolID || eduSchoolId),
      schoolName: school.schoolName || school.SchoolName || '',
      dialog: school.dialog || school.Dialog || '',
      established: school.established || school.Established || '',
      principal: school.principal || school.Principal || '',
      address: school.address || school.Address || '',
      phone: school.phone || school.Phone || '',
      email: school.email || school.Email || '',
      website: school.website || school.Website || '',
      logoUrl: hasLogo ? '/api/edu/logo' : '',
    },
    stats: {
      students: Number(stats.students || stats.Students || 0),
      teachers: Number(stats.teachers || stats.Teachers || 0),
      staff: Number(stats.staff || stats.Staff || 0),
      educationYear: stats.educationYear || stats.EducationYear || '',
      classes: (Array.isArray(classes) ? classes : []).map((row) => ({
        classID: Number(row.classID || row.ClassID || 0),
        name: row.name || row.Name || '',
        count: Number(row.count || row.Count || 0),
      })),
    },
    teachers: (data.teachers || data.Teachers || []).map((row) => mapEduPerson(row, 'teacher')),
    staff: (data.staff || data.Staff || []).map((row) => mapEduPerson(row, 'staff')),
    students: (data.students || data.Students || []).map((row) => mapEduPerson(row, 'student')),
    birthdays: (data.birthdays || data.Birthdays || []).map((row) =>
      mapEduPerson(row, row.kind || row.Kind || 'student'),
    ),
    notices: (data.notices || data.Notices || [])
      .map((row) => ({
        id: Number(row.id || row.Id || 0),
        title: String(row.title || row.Title || '').trim(),
        body: String(row.body || row.Body || '').trim(),
        date: row.date || row.Date || '',
      }))
      .filter((row) => row.title),
  }
}

function statsFromInstitute(row) {
  if (!row) return {}
  const classes = row.classes || row.Classes || []
  return {
    students: Number(row.students || row.Students || 0),
    teachers: Number(row.teachers || row.Teachers || 0),
    staff: Number(row.staff || row.Staff || 0),
    educationYear: row.educationYear || row.EducationYear || '',
    classes: Array.isArray(classes) ? classes : [],
  }
}

async function eduInstituteFallback() {
  const response = await eduFetch('/api/public/institutes')
  const list = await response.json()
  const rows = Array.isArray(list) ? list : []
  const found = rows.find((row) => Number(row.schoolID || row.SchoolID) === eduSchoolId)
  if (!found) throw new Error('Institution not found on Sikkhaloy API')
  return {
    schoolID: Number(found.schoolID || found.SchoolID),
    schoolName: found.schoolName || found.SchoolName || '',
    hasLogo: Boolean(found.hasLogo || found.HasLogo),
    stats: statsFromInstitute(found),
  }
}

function isFullEduHome(data) {
  if (!data) return false
  const teachers = data.teachers || data.Teachers
  const students = data.students || data.Students
  const birthdays = data.birthdays || data.Birthdays
  const stats = data.stats || data.Stats || {}
  return (
    Array.isArray(teachers) ||
    Array.isArray(students) ||
    Array.isArray(birthdays) ||
    Number(stats.students || stats.Students || 0) > 0
  )
}

async function eduWebsiteHome() {
  const paths = [`/api/public/website/${eduSchoolId}`, `/api/public/institutes/${eduSchoolId}`]
  let thin = null
  for (const base of eduApiBases) {
    for (const path of paths) {
      try {
        const response = await fetch(`${base}${path}`)
        if (!response.ok) continue
        const data = await response.json()
        if (isFullEduHome(data)) return data
        if (!thin && data && (data.school || data.School || data.stats || data.Stats)) thin = data
      } catch {
        /* try the next API host */
      }
    }
  }
  return thin
}

app.use('/api/edu', rateLimit({ name: 'edu', limit: 90, windowSec: 60 }))
app.use('/api/admissions', rateLimit({ name: 'admissions', limit: 40, windowSec: 60 }))

app.get('/api/edu/home', async (_req, res) => {
  try {
    const data = await eduWebsiteHome()
    if (data) return res.json(mapEduHome(data))
    const fallback = await eduInstituteFallback()
    res.json(mapEduHome({ stats: fallback.stats }, fallback))
  } catch (err) {
    res.status(502).json({ error: err.message || 'Sikkhaloy API is not reachable' })
  }
})

app.get('/api/edu/logo', async (_req, res) => {
  try {
    const response = await eduFetch(`/api/public/institutes/${eduSchoolId}/logo`)
    const buffer = Buffer.from(await response.arrayBuffer())
    res.setHeader('Content-Type', response.headers.get('content-type') || 'image/jpeg')
    res.setHeader('Cache-Control', 'public, max-age=3600')
    res.send(buffer)
  } catch {
    res.status(404).json({ error: 'Logo not found' })
  }
})

app.get('/api/edu/student-photo/:id', async (req, res) => {
  try {
    const response = await eduFetch(`/api/public/website/${eduSchoolId}/students/${Number(req.params.id)}/photo`)
    const buffer = Buffer.from(await response.arrayBuffer())
    res.setHeader('Content-Type', response.headers.get('content-type') || 'image/jpeg')
    res.setHeader('Cache-Control', 'public, max-age=3600')
    res.send(buffer)
  } catch {
    res.status(404).json({ error: 'Photo not found' })
  }
})

app.get('/api/edu/teacher-photo/:id', async (req, res) => {
  try {
    const response = await eduFetch(`/api/public/website/${eduSchoolId}/teachers/${Number(req.params.id)}/photo`)
    const buffer = Buffer.from(await response.arrayBuffer())
    res.setHeader('Content-Type', response.headers.get('content-type') || 'image/jpeg')
    res.setHeader('Cache-Control', 'public, max-age=3600')
    res.send(buffer)
  } catch {
    res.status(404).json({ error: 'Photo not found' })
  }
})

app.get('/api/edu/staff-photo/:id', async (req, res) => {
  try {
    const response = await eduFetch(`/api/public/website/${eduSchoolId}/staff/${Number(req.params.id)}/photo`)
    const buffer = Buffer.from(await response.arrayBuffer())
    res.setHeader('Content-Type', response.headers.get('content-type') || 'image/jpeg')
    res.setHeader('Cache-Control', 'public, max-age=3600')
    res.send(buffer)
  } catch {
    res.status(404).json({ error: 'Photo not found' })
  }
})

function pickText(body, ...keys) {
  for (const key of keys) {
    if (body[key] != null && String(body[key]).trim() !== '') return String(body[key]).trim()
  }
  return ''
}

function pickInt(body, ...keys) {
  for (const key of keys) {
    const n = Number(body[key])
    if (Number.isFinite(n) && n > 0) return Math.trunc(n)
  }
  return 0
}

function parseSubjects(raw) {
  if (Array.isArray(raw)) return raw
  if (!raw) return []
  try {
    const parsed = typeof raw === 'string' ? JSON.parse(raw) : raw
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

function mapAdmissionRow(row) {
  return {
    id: row.id,
    status: row.status,
    studentsName: row.students_name,
    smsPhone: row.sms_phone,
    gender: row.gender,
    fathersName: row.fathers_name,
    educationYearId: row.education_year_id,
    educationYear: row.education_year,
    classId: row.class_id,
    className: row.class_name,
    groupId: row.group_id,
    groupName: row.group_name,
    sectionId: row.section_id,
    sectionName: row.section_name,
    shiftId: row.shift_id,
    shiftName: row.shift_name,
    rollNo: row.roll_no,
    studentEmail: row.student_email,
    dateOfBirth: row.date_of_birth,
    legalIdentity: row.legal_identity,
    bloodGroup: row.blood_group,
    religion: row.religion,
    permanentAddress: row.permanent_address,
    localAddress: row.local_address,
    photo: row.photo,
    mothersName: row.mothers_name,
    motherPhone: row.mother_phone,
    motherOccupation: row.mother_occupation,
    fatherPhone: row.father_phone,
    fatherOccupation: row.father_occupation,
    guardianName: row.guardian_name,
    guardianRelation: row.guardian_relation,
    guardianPhone: row.guardian_phone,
    prevSchool: row.prev_school,
    prevClass: row.prev_class,
    prevExamYear: row.prev_exam_year,
    prevGrade: row.prev_grade,
    subjects: parseSubjects(row.subjects_json),
    payments: parseSubjects(row.payments_json),
    assignedId: row.assigned_id,
    sikkhaloyStudentId: row.sikkhaloy_student_id,
    confirmError: row.confirm_error,
    createdAt: row.created_at,
    confirmedAt: row.confirmed_at,
  }
}

function readAdmissionInput(body) {
  const studentsName = pickText(body, 'studentsName', 'StudentsName')
  const smsPhone = pickText(body, 'smsPhone', 'SMSPhoneNo', 'smsPhoneNo').replace(/\D/g, '')
  const fathersName = pickText(body, 'fathersName', 'FathersName')
  const educationYearId = pickInt(body, 'educationYearId', 'EducationYearID')
  const classId = pickInt(body, 'classId', 'ClassID')
  return {
    studentsName,
    smsPhone,
    gender: pickText(body, 'gender', 'Gender') || 'Male',
    fathersName,
    educationYearId,
    educationYear: pickText(body, 'educationYear', 'EducationYear'),
    classId,
    className: pickText(body, 'className', 'ClassName'),
    groupId: pickInt(body, 'groupId', 'SubjectGroupID'),
    groupName: pickText(body, 'groupName', 'GroupName'),
    sectionId: pickInt(body, 'sectionId', 'SectionID'),
    sectionName: pickText(body, 'sectionName', 'SectionName'),
    shiftId: pickInt(body, 'shiftId', 'ShiftID'),
    shiftName: pickText(body, 'shiftName', 'ShiftName'),
    rollNo: pickText(body, 'rollNo', 'RollNo'),
    studentEmail: pickText(body, 'studentEmail', 'StudentEmailAddress'),
    dateOfBirth: pickText(body, 'dateOfBirth', 'DateofBirth'),
    legalIdentity: pickText(body, 'legalIdentity', 'LegalIdentity'),
    bloodGroup: pickText(body, 'bloodGroup', 'BloodGroup'),
    religion: pickText(body, 'religion', 'Religion'),
    permanentAddress: pickText(body, 'permanentAddress', 'StudentPermanentAddress'),
    localAddress: pickText(body, 'localAddress', 'StudentsLocalAddress'),
    mothersName: pickText(body, 'mothersName', 'MothersName'),
    motherPhone: pickText(body, 'motherPhone', 'MotherPhoneNumber'),
    motherOccupation: pickText(body, 'motherOccupation', 'MotherOccupation'),
    fatherPhone: pickText(body, 'fatherPhone', 'FatherPhoneNumber'),
    fatherOccupation: pickText(body, 'fatherOccupation', 'FatherOccupation'),
    guardianName: pickText(body, 'guardianName', 'GuardianName'),
    guardianRelation: pickText(body, 'guardianRelation', 'GuardianRelationshipwithStudent'),
    guardianPhone: pickText(body, 'guardianPhone', 'GuardianPhoneNumber'),
    prevSchool: pickText(body, 'prevSchool', 'PrevSchoolName'),
    prevClass: pickText(body, 'prevClass', 'PrevClass'),
    prevExamYear: pickText(body, 'prevExamYear', 'PrevExamYear'),
    prevGrade: pickText(body, 'prevGrade', 'PrevExamGrade'),
    subjects: parseSubjects(body.subjects || body.Subjects),
  }
}

function photoDataUrl(photo) {
  if (!photo) return ''
  if (String(photo).startsWith('data:')) return photo
  const rel = String(photo).replace(/^\//, '').replace(/\//g, path.sep)
  const abs = path.join(__dirname, '..', rel)
  if (!fs.existsSync(abs)) return ''
  const buf = fs.readFileSync(abs)
  const ext = path.extname(abs).toLowerCase()
  const mime = ext === '.png' ? 'image/png' : ext === '.webp' ? 'image/webp' : 'image/jpeg'
  return `data:${mime};base64,${buf.toString('base64')}`
}

function mapAdmitPayload(row, photo) {
  return {
    studentsName: row.studentsName,
    smsPhoneNo: row.smsPhone,
    gender: row.gender,
    fathersName: row.fathersName,
    educationYearID: row.educationYearId,
    classID: row.classId,
    sectionID: row.sectionId || 0,
    shiftID: row.shiftId || 0,
    subjectGroupID: row.groupId || 0,
    rollNo: row.rollNo,
    studentEmailAddress: row.studentEmail,
    dateofBirth: row.dateOfBirth,
    legalIdentity: row.legalIdentity,
    bloodGroup: row.bloodGroup,
    religion: row.religion,
    studentsLocalAddress: row.localAddress,
    studentPermanentAddress: row.permanentAddress,
    mothersName: row.mothersName,
    motherOccupation: row.motherOccupation,
    motherPhoneNumber: row.motherPhone,
    fatherOccupation: row.fatherOccupation,
    fatherPhoneNumber: row.fatherPhone,
    guardianName: row.guardianName,
    guardianRelationshipwithStudent: row.guardianRelation,
    guardianPhoneNumber: row.guardianPhone,
    prevSchoolName: row.prevSchool,
    prevClass: row.prevClass,
    prevExamYear: row.prevExamYear,
    prevExamGrade: row.prevGrade,
    photoDataUrl: photoDataUrl(photo || row.photo),
    subjects: (row.subjects || []).map((item) => ({
      subjectID: Number(item.subjectID || item.SubjectID || 0),
      subjectType: item.subjectType || item.SubjectType || 'Compulsory',
    })),
    sendSms: row.sendSms !== false,
    payments: (row.payments || []).map((item) => ({
      roleID: Number(item.roleID || item.RoleID || 0),
      role: item.role || item.Role || '',
      payFor: item.payFor || item.PayFor || '',
      amount: Number(item.amount ?? item.Amount ?? 0),
      discount: Number(item.discount ?? item.Discount ?? 0),
      assignRoleID: Number(item.assignRoleID || item.AssignRoleID || 0),
      lateFee: Number(item.lateFee ?? item.LateFee ?? 0),
      startDate: item.startDate || item.StartDate || null,
      endDate: item.endDate || item.EndDate || null,
    })),
  }
}

function mapPayments(raw) {
  return (Array.isArray(raw) ? raw : []).map((item) => ({
    roleID: Number(item.roleID || item.RoleID || 0),
    role: item.role || item.Role || '',
    payFor: item.payFor || item.PayFor || '',
    amount: Number(item.amount ?? item.Amount ?? 0),
    discount: Number(item.discount ?? item.Discount ?? 0),
    assignRoleID: Number(item.assignRoleID || item.AssignRoleID || 0),
    lateFee: Number(item.lateFee ?? item.LateFee ?? 0),
    startDate: item.startDate || item.StartDate || null,
    endDate: item.endDate || item.EndDate || null,
  }))
}

function mapAdmissionForm(data) {
  const sessions = data.sessions || data.Sessions || []
  const classes = data.classes || data.Classes || []
  return {
    lastStudentId: data.lastStudentId || data.LastStudentId || '',
    activeEducationYearId: Number(data.activeEducationYearId || data.ActiveEducationYearId || 0),
    sessions: sessions.map((row) => ({
      id: Number(row.id || row.Id || 0),
      name: row.name || row.Name || '',
    })),
    classes: classes.map((row) => ({
      id: Number(row.id || row.Id || 0),
      name: row.name || row.Name || '',
    })),
    feeRoles: (data.feeRoles || data.FeeRoles || []).map((row) => ({
      roleID: Number(row.roleID || row.RoleID || 0),
      role: row.role || row.Role || '',
      numberOfPay: Number(row.numberOfPay || row.NumberOfPay || 0),
    })),
  }
}

function mapAdmissionOptions(data) {
  const mapLookup = (rows) =>
    (Array.isArray(rows) ? rows : []).map((row) => ({
      id: Number(row.id || row.Id || 0),
      name: row.name || row.Name || '',
    }))
  const subjects = data.subjects || data.Subjects || []
  return {
    lastRollNo: data.lastRollNo || data.LastRollNo || '',
    groups: mapLookup(data.groups || data.Groups),
    sections: mapLookup(data.sections || data.Sections),
    shifts: mapLookup(data.shifts || data.Shifts),
    subjects: subjects.map((row) => ({
      subjectID: Number(row.subjectID || row.SubjectID || 0),
      subjectName: row.subjectName || row.SubjectName || '',
      subjectType: row.subjectType || row.SubjectType || 'Compulsory',
      selected: Boolean(row.selected ?? row.Selected),
    })),
    fees: (data.fees || data.Fees || []).map((row) => ({
      assignRoleID: Number(row.assignRoleID || row.AssignRoleID || 0),
      roleID: Number(row.roleID || row.RoleID || 0),
      role: row.role || row.Role || '',
      payFor: row.payFor || row.PayFor || '',
      amount: Number(row.amount ?? row.Amount ?? 0),
      lateFee: Number(row.lateFee ?? row.LateFee ?? 0),
      startDate: row.startDate || row.StartDate || null,
      endDate: row.endDate || row.EndDate || null,
    })),
  }
}

async function eduPost(path, body) {
  const key = process.env.SIKKHALOY_WEBSITE_KEY || ''
  let lastError = new Error(`Sikkhaloy API failed ${path}`)
  for (const base of eduApiBases) {
    try {
      const res = await fetch(`${base}${path}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(key ? { 'X-Website-Key': key } : {}),
        },
        body: JSON.stringify(body),
      })
      const data = await res.json().catch(() => ({}))
      if (res.ok) return data
      lastError = new Error(data.error || data.Error || `Sikkhaloy API ${res.status}`)
    } catch (err) {
      lastError = err
    }
  }
  throw lastError
}

app.get('/api/admissions/form', async (_req, res) => {
  try {
    const response = await eduFetch(`/api/public/website/${eduSchoolId}/admission`)
    res.json(mapAdmissionForm(await response.json()))
  } catch (err) {
    res.status(502).json({ error: err.message || 'Sikkhaloy admission form is not reachable' })
  }
})

app.get('/api/admissions/options', async (req, res) => {
  try {
    const classId = Number(req.query.classId || 0)
    if (classId <= 0) return res.status(400).json({ error: 'Class is required' })
    const qs = new URLSearchParams({
      classId: String(classId),
      educationYearId: String(Number(req.query.educationYearId || 0)),
      groupId: String(Number(req.query.groupId || 0)),
      sectionId: String(Number(req.query.sectionId || 0)),
      shiftId: String(Number(req.query.shiftId || 0)),
    })
    const response = await eduFetch(`/api/public/website/${eduSchoolId}/admission/options?${qs}`)
    res.json(mapAdmissionOptions(await response.json()))
  } catch (err) {
    res.status(502).json({ error: err.message || 'Sikkhaloy class options are not reachable' })
  }
})

app.post('/api/admissions', ...withUploads(upload.single('studentPhoto')), async (req, res) => {
  const body = { ...(req.body || {}) }
  const item = readAdmissionInput(body)
  if (!item.studentsName || !item.fathersName) {
    return res.status(400).json({ error: 'Student name and father\'s name are required.' })
  }
  if (!/^01[3-9]\d{8}$/.test(item.smsPhone)) {
    return res.status(400).json({ error: 'SMS mobile must be 11 digits (01XXXXXXXXX).' })
  }
  if (!item.dateOfBirth) {
    return res.status(400).json({ error: 'Date of birth is required.' })
  }
  if (!item.permanentAddress) {
    return res.status(400).json({ error: 'Permanent address is required.' })
  }
  if (!/^01[3-9]\d{8}$/.test(item.fatherPhone) || !/^01[3-9]\d{8}$/.test(item.motherPhone)) {
    return res.status(400).json({ error: 'Father and mother mobile must be 11 digits (01XXXXXXXXX).' })
  }
  if (item.educationYearId <= 0 || item.classId <= 0) {
    return res.status(400).json({ error: 'Session year and class are required.' })
  }
  const photo = req.file ? req.file.publicUrl : ''
  const [result] = await pool.query(
    `INSERT INTO admissions (
      students_name, sms_phone, gender, fathers_name, education_year_id, education_year,
      class_id, class_name, group_id, group_name, section_id, section_name, shift_id, shift_name,
      roll_no, student_email, date_of_birth, legal_identity, blood_group, religion,
      permanent_address, local_address, photo, mothers_name, mother_phone, mother_occupation,
      father_phone, father_occupation, guardian_name, guardian_relation, guardian_phone,
      prev_school, prev_class, prev_exam_year, prev_grade, subjects_json
    ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)`,
    [
      item.studentsName,
      item.smsPhone,
      item.gender,
      item.fathersName,
      item.educationYearId,
      item.educationYear,
      item.classId,
      item.className,
      item.groupId,
      item.groupName,
      item.sectionId,
      item.sectionName,
      item.shiftId,
      item.shiftName,
      item.rollNo,
      item.studentEmail,
      item.dateOfBirth,
      item.legalIdentity,
      item.bloodGroup,
      item.religion,
      item.permanentAddress,
      item.localAddress,
      photo,
      item.mothersName,
      item.motherPhone,
      item.motherOccupation,
      item.fatherPhone,
      item.fatherOccupation,
      item.guardianName,
      item.guardianRelation,
      item.guardianPhone,
      item.prevSchool,
      item.prevClass,
      item.prevExamYear,
      item.prevGrade,
      JSON.stringify(item.subjects),
    ],
  )
  res.json({ ok: true, id: result.insertId })
})

app.get('/api/admissions', requireAuth, requirePerm('admissions'), async (_req, res) => {
  const [rows] = await pool.query('SELECT * FROM admissions ORDER BY created_at DESC, id DESC')
  res.json({ admissions: rows.map(mapAdmissionRow) })
})

app.delete('/api/admissions/:id', requireAuth, requirePerm('admissions'), async (req, res) => {
  await pool.query('DELETE FROM admissions WHERE id = ?', [req.params.id])
  res.json({ ok: true })
})

app.post('/api/admissions/:id/confirm', requireAuth, requirePerm('admissions'), async (req, res) => {
  const id = Number(req.params.id)
  const [rows] = await pool.query('SELECT * FROM admissions WHERE id = ?', [id])
  const row = rows[0]
  if (!row) return res.status(404).json({ error: 'Application not found' })
  const body = req.body || {}
  const payments = mapPayments(body.payments || body.Payments)
  const sendSms = body.sendSms !== false && body.SendSms !== false
  if (row.status === 'confirmed' && row.sikkhaloy_student_id) {
    if (payments.length === 0) {
      return res.json({ ok: true, admission: mapAdmissionRow(row) })
    }
    try {
      const result = await eduPost(`/api/public/website/${eduSchoolId}/admit-payments`, {
        studentID: Number(row.sikkhaloy_student_id),
        educationYearID: Number(row.education_year_id),
        classID: Number(row.class_id),
        payments,
      })
      const storedPayments = mapPayments(result.payments || result.Payments || payments)
      await pool.query(
        `UPDATE admissions SET payments_json = ?, confirm_error = NULL WHERE id = ?`,
        [JSON.stringify(storedPayments), id],
      )
      const [next] = await pool.query('SELECT * FROM admissions WHERE id = ?', [id])
      return res.json({ ok: true, admission: mapAdmissionRow(next[0]), result })
    } catch (err) {
      const message = err.message || 'Could not save admission payments.'
      await pool.query('UPDATE admissions SET confirm_error = ? WHERE id = ?', [message, id])
      return res.status(502).json({ error: message })
    }
  }
  try {
    const payload = mapAdmitPayload({ ...mapAdmissionRow(row), payments, sendSms })
    const result = await eduPost(`/api/public/website/${eduSchoolId}/admit`, payload)
    const assignedId = result.studentCode || result.StudentCode || ''
    const studentId = Number(result.studentID || result.StudentID || 0)
    const storedPayments = mapPayments(result.payments || result.Payments || payments)
    await pool.query(
      `UPDATE admissions
       SET status = 'confirmed', assigned_id = ?, sikkhaloy_student_id = ?, payments_json = ?, confirm_error = NULL, confirmed_at = NOW()
       WHERE id = ?`,
      [assignedId, studentId || null, JSON.stringify(storedPayments), id],
    )
    const [next] = await pool.query('SELECT * FROM admissions WHERE id = ?', [id])
    res.json({ ok: true, admission: mapAdmissionRow(next[0]), result })
  } catch (err) {
    const message = err.message || 'Could not push this student to Sikkhaloy.'
    await pool.query('UPDATE admissions SET confirm_error = ? WHERE id = ?', [message, id])
    res.status(502).json({ error: message })
  }
})

function mapJob(row) {
  return {
    id: Number(row.id),
    title: row.title,
    titleBn: row.title_bn || '',
    department: row.department || '',
    location: row.location || '',
    jobType: row.job_type || 'Full-time',
    deadline: row.deadline || '',
    description: row.description || '',
    descriptionBn: row.description_bn || '',
    image: row.image || '',
    published: Number(row.published) !== 0,
    createdAt: row.created_at || '',
  }
}

function mapJobApp(row) {
  return {
    id: Number(row.id),
    jobId: Number(row.job_id),
    jobTitle: row.job_title || '',
    name: row.name,
    phone: row.phone,
    email: row.email || '',
    address: row.address || '',
    education: row.education || '',
    experience: row.experience || '',
    coverLetter: row.cover_letter || '',
    cv: row.cv || '',
    status: row.status || 'pending',
    createdAt: row.created_at || '',
  }
}

app.get('/api/jobs', async (_req, res) => {
  const [rows] = await pool.query('SELECT * FROM jobs WHERE published = 1 ORDER BY id DESC')
  res.json({ jobs: rows.map(mapJob) })
})

app.get('/api/jobs/manage', requireAuth, requirePerm('career'), async (_req, res) => {
  const [rows] = await pool.query('SELECT * FROM jobs ORDER BY id DESC')
  res.json({ jobs: rows.map(mapJob) })
})

app.get('/api/jobs/:id', async (req, res) => {
  const [rows] = await pool.query('SELECT * FROM jobs WHERE id = ?', [Number(req.params.id)])
  const row = rows[0]
  if (!row || (!Number(row.published) && !req.headers.authorization)) {
    return res.status(404).json({ error: 'Job not found' })
  }
  res.json({ job: mapJob(row) })
})

app.post('/api/jobs', requireAuth, requirePerm('career'), ...withUploads(upload.single('jobCover')), async (req, res) => {
  const body = req.body || {}
  const id = Number(body.id || 0)
  const published = body.published === false || body.published === 'false' || body.published === '0' || body.published === 0 ? 0 : 1
  const fields = [
    String(body.title || '').trim(),
    String(body.titleBn || body.title_bn || '').trim(),
    String(body.department || '').trim(),
    String(body.location || '').trim(),
    String(body.jobType || body.job_type || 'Full-time').trim(),
    String(body.deadline || '').trim(),
    String(body.description || ''),
    String(body.descriptionBn || body.description_bn || ''),
    published,
  ]
  if (!fields[0]) return res.status(400).json({ error: 'Job title is required.' })
  let image = String(body.image || '')
  if (req.file) image = req.file.publicUrl
  let jobId = id
  if (id) {
    if (!req.file) {
      const [prev] = await pool.query('SELECT image FROM jobs WHERE id = ?', [id])
      image = prev[0]?.image || image
    }
    await pool.query(
      'UPDATE jobs SET title=?, title_bn=?, department=?, location=?, job_type=?, deadline=?, description=?, description_bn=?, published=?, image=? WHERE id=?',
      [...fields, image, id],
    )
  } else {
    const [result] = await pool.query(
      'INSERT INTO jobs (title, title_bn, department, location, job_type, deadline, description, description_bn, published, image) VALUES (?,?,?,?,?,?,?,?,?,?)',
      [...fields, image],
    )
    jobId = result.insertId
  }
  const [rows] = await pool.query('SELECT * FROM jobs WHERE id = ?', [jobId])
  res.json({ ok: true, job: mapJob(rows[0]) })
})

app.delete('/api/jobs/:id', requireAuth, requirePerm('career'), async (req, res) => {
  const id = Number(req.params.id)
  await pool.query('DELETE FROM job_applications WHERE job_id = ?', [id])
  await pool.query('DELETE FROM jobs WHERE id = ?', [id])
  res.json({ ok: true })
})

app.post('/api/jobs/:id/apply', ...withUploads(upload.single('jobCv')), async (req, res) => {
  const jobId = Number(req.params.id)
  const [jobs] = await pool.query('SELECT id FROM jobs WHERE id = ? AND published = 1', [jobId])
  if (!jobs[0]) return res.status(404).json({ error: 'Job not found' })
  const name = String(req.body.name || '').trim()
  const phone = String(req.body.phone || '').trim()
  if (!name || !phone) return res.status(400).json({ error: 'Name and phone are required.' })
  if (!req.file) return res.status(400).json({ error: 'CV file is required.' })
  const ext = path.extname(req.file.originalname || '').toLowerCase()
  if (!['.pdf', '.doc', '.docx'].includes(ext)) return res.status(400).json({ error: 'CV must be PDF or Word file.' })
  const cv = req.file.publicUrl
  await pool.query(
    'INSERT INTO job_applications (job_id, name, phone, email, address, education, experience, cover_letter, cv) VALUES (?,?,?,?,?,?,?,?,?)',
    [
      jobId,
      name,
      phone,
      String(req.body.email || '').trim(),
      String(req.body.address || '').trim(),
      String(req.body.education || '').trim(),
      String(req.body.experience || '').trim(),
      String(req.body.coverLetter || req.body.cover_letter || '').trim(),
      cv,
    ],
  )
  res.json({ ok: true })
})

app.get('/api/job-applications', requireAuth, requirePerm('career'), async (req, res) => {
  const args = []
  let sql = 'SELECT a.*, j.title AS job_title FROM job_applications a LEFT JOIN jobs j ON j.id = a.job_id WHERE 1=1'
  if (req.query.jobId) {
    sql += ' AND a.job_id = ?'
    args.push(Number(req.query.jobId))
  }
  if (req.query.status) {
    sql += ' AND a.status = ?'
    args.push(String(req.query.status))
  }
  sql += ' ORDER BY a.id DESC'
  const [rows] = await pool.query(sql, args)
  res.json({ applications: rows.map(mapJobApp) })
})

app.post('/api/job-applications/:id/status', requireAuth, requirePerm('career'), async (req, res) => {
  const status = String(req.body?.status || '')
  if (!['pending', 'shortlisted', 'rejected', 'confirmed'].includes(status)) {
    return res.status(400).json({ error: 'Invalid status.' })
  }
  await pool.query('UPDATE job_applications SET status = ? WHERE id = ?', [status, Number(req.params.id)])
  res.json({ ok: true })
})

app.get('/api/job-applications/:id/cv', requireAuth, requirePerm('career'), async (req, res) => {
  const [rows] = await pool.query('SELECT cv FROM job_applications WHERE id = ?', [Number(req.params.id)])
  const file = rows[0]?.cv
  if (!file) return res.status(404).json({ error: 'CV not found' })
  const abs = path.join(__dirname, '..', String(file).replace(/^\//, '').replace(/\//g, path.sep))
  if (!fs.existsSync(abs)) return res.status(404).json({ error: 'CV file missing' })
  res.download(abs, path.basename(file))
})

app.use((err, _req, res, _next) => {
  console.error(err)
  res.status(500).json({ error: err.message || 'Server error' })
})

const port = Number(process.env.PORT || 3002)

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
