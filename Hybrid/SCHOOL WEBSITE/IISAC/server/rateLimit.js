import fs from 'node:fs'
import os from 'node:os'

const buckets = new Map()

function clientIp(req) {
  const xff = String(req.headers['x-forwarded-for'] || '')
  if (xff) return xff.split(',')[0].trim()
  return req.socket?.remoteAddress || '0.0.0.0'
}

export function rateLimit({ name = 'default', limit = 60, windowSec = 60 } = {}) {
  return function rateLimitMiddleware(req, res, next) {
    const ip = clientIp(req)
    const key = `${name}:${ip}`
    const now = Date.now()
    let entry = buckets.get(key)
    if (!entry || now - entry.start >= windowSec * 1000) {
      entry = { start: now, count: 0 }
    }
    entry.count += 1
    buckets.set(key, entry)
    if (entry.count > limit) {
      res.setHeader('Retry-After', String(windowSec))
      return res.status(429).json({ error: 'Too many requests. Please try again later.' })
    }
    return next()
  }
}

export function eduLog(schoolId, path, status, req) {
  try {
    const dir = `${os.tmpdir()}/iisac-logs`
    fs.mkdirSync(dir, { recursive: true })
    const line = JSON.stringify({
      ts: new Date().toISOString(),
      ip: clientIp(req),
      school_id: Number(schoolId) || 0,
      path: String(path || ''),
      status: Number(status) || 0,
    })
    const day = new Date().toISOString().slice(0, 10)
    fs.appendFileSync(`${dir}/edu-${day}.log`, `${line}\n`)
  } catch {
    // ignore logging failures
  }
}
