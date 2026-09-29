import dotenv from 'dotenv'
import mysql from 'mysql2/promise'
import { DEFAULT_PROJECTS } from '../src/data/projects.js'

dotenv.config()

const extras = DEFAULT_PROJECTS.filter((p) =>
  ['dhanmondi-pearl', 'mirpur-green-court'].includes(p.id),
)

const db = await mysql.createConnection({
  host: process.env.DB_HOST || 'localhost',
  user: process.env.DB_USER || 'root',
  password: process.env.DB_PASS || '',
  database: process.env.DB_NAME || 'safa_propertis',
})

for (const p of extras) {
  await db.query(
    `INSERT IGNORE INTO projects
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
      p.phone || '',
      p.whatsapp || '',
    ],
  )
}

const [rows] = await db.query("SELECT id, title FROM projects WHERE status='completed'")
console.log('completed projects:', rows.length)
await db.end()
