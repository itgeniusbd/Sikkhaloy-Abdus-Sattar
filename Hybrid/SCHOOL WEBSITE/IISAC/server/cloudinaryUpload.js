import dotenv from 'dotenv'
dotenv.config()

import { v2 as cloudinary } from 'cloudinary'
import multer from 'multer'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import fs from 'node:fs'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const uploadsRoot = path.join(__dirname, '..', 'uploads')

function applyCloudinaryConfig() {
  const cloud_name = process.env.CLOUDINARY_CLOUD_NAME || ''
  const api_key = process.env.CLOUDINARY_API_KEY || ''
  const api_secret = process.env.CLOUDINARY_API_SECRET || ''
  if (!cloud_name || !api_key || !api_secret) return false
  cloudinary.config({ cloud_name, api_key, api_secret, secure: true })
  return true
}

export function cloudinaryConfigured() {
  return Boolean(
    process.env.CLOUDINARY_CLOUD_NAME &&
      process.env.CLOUDINARY_API_KEY &&
      process.env.CLOUDINARY_API_SECRET,
  )
}

export function uploadFolderKey(fieldname) {
  const f = String(fieldname || '')
  if (f.startsWith('hero')) return 'hero'
  if (f.startsWith('foot')) return 'gallery'
  if (f.startsWith('post')) return 'posts'
  if (f.startsWith('about') || f.startsWith('admin') || f.startsWith('page') || f.startsWith('member')) return 'pages'
  if (f.startsWith('admission') || f === 'studentPhoto') return 'admissions'
  if (f === 'jobCover') return 'jobs'
  if (f === 'jobCv' || f.startsWith('cv')) return 'careers'
  if (f === 'image' || f === 'gallery') return 'projects'
  return 'projects'
}

function localAbsFolder(fieldname) {
  return path.join(uploadsRoot, uploadFolderKey(fieldname))
}

function localPublicUrl(fieldname, filename) {
  return `/uploads/${uploadFolderKey(fieldname)}/${filename}`
}

const diskStorage = multer.diskStorage({
  destination: (_req, file, cb) => {
    const dir = localAbsFolder(file.fieldname)
    fs.mkdirSync(dir, { recursive: true })
    cb(null, dir)
  },
  filename: (_req, file, cb) => {
    const ext = path.extname(file.originalname || '.jpg') || '.jpg'
    const prefix = uploadFolderKey(file.fieldname).slice(0, 2)
    cb(null, `${prefix}_${Date.now()}_${Math.random().toString(16).slice(2)}${ext}`)
  },
})

const memoryStorage = multer.memoryStorage()

// Decide storage after dotenv.config() above
export const upload = multer({
  storage: cloudinaryConfigured() ? memoryStorage : diskStorage,
  limits: { fileSize: 12 * 1024 * 1024 },
})

function resourceTypeFor(file) {
  const mime = String(file.mimetype || '')
  const name = String(file.originalname || '').toLowerCase()
  if (mime.startsWith('image/')) return 'image'
  if (mime === 'application/pdf' || name.endsWith('.pdf')) return 'raw'
  return 'auto'
}

export async function toPublicUrl(file) {
  if (!file) return ''
  if (file.publicUrl) return file.publicUrl

  if (cloudinaryConfigured()) {
    if (!applyCloudinaryConfig()) {
      throw new Error('Cloudinary credentials missing')
    }
    const folderBase = (process.env.CLOUDINARY_FOLDER || 'iisac').replace(/\/$/, '')
    const folder = `${folderBase}/${uploadFolderKey(file.fieldname)}`
    const result = await new Promise((resolve, reject) => {
      const stream = cloudinary.uploader.upload_stream(
        {
          folder,
          resource_type: resourceTypeFor(file),
          use_filename: true,
          unique_filename: true,
          overwrite: false,
        },
        (err, res) => (err ? reject(err) : resolve(res)),
      )
      if (!file.buffer) {
        reject(new Error('Missing file buffer — restart server after enabling Cloudinary'))
        return
      }
      stream.end(file.buffer)
    })
    file.publicUrl = result.secure_url
    return file.publicUrl
  }

  file.publicUrl = localPublicUrl(file.fieldname, file.filename)
  return file.publicUrl
}

export async function mapUploadedFiles(req) {
  const list = []
  if (req.file) list.push(req.file)
  if (Array.isArray(req.files)) list.push(...req.files)
  else if (req.files && typeof req.files === 'object') {
    for (const arr of Object.values(req.files)) {
      if (Array.isArray(arr)) list.push(...arr)
    }
  }
  for (const file of list) {
    await toPublicUrl(file)
  }
}