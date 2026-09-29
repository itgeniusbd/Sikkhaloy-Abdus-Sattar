import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const root = path.join(path.dirname(fileURLToPath(import.meta.url)), '..')
const dist = path.join(root, 'dist')

function copyDir(from, to, skip = []) {
  if (!fs.existsSync(from)) return
  fs.mkdirSync(to, { recursive: true })
  for (const entry of fs.readdirSync(from, { withFileTypes: true })) {
    if (skip.includes(entry.name)) continue
    const src = path.join(from, entry.name)
    const dest = path.join(to, entry.name)
    if (entry.isDirectory()) copyDir(src, dest, skip)
    else fs.copyFileSync(src, dest)
  }
}

if (!fs.existsSync(dist)) {
  console.error('dist/ not found. Run vite build first.')
  process.exit(1)
}

copyDir(path.join(root, 'install'), path.join(dist, 'install'))
copyDir(path.join(root, 'api'), path.join(dist, 'api'), ['config.json', 'config.local.php'])
fs.copyFileSync(path.join(root, 'index.php'), path.join(dist, 'index.php'))

const uploadRoot = path.join(dist, 'uploads')
for (const folder of ['', 'projects', 'hero', 'gallery', 'posts', 'pages']) {
  fs.mkdirSync(path.join(uploadRoot, folder), { recursive: true })
}
const uploadHt = path.join(root, 'uploads', '.htaccess')
if (fs.existsSync(uploadHt)) {
  fs.copyFileSync(uploadHt, path.join(uploadRoot, '.htaccess'))
}

const rootHt = path.join(root, '.htaccess')
if (fs.existsSync(rootHt)) {
  fs.copyFileSync(rootHt, path.join(dist, '.htaccess'))
}


const routerSrc = path.join(root, 'scripts', 'php-router.php')
const routerDest = path.join(dist, 'router.php')
if (fs.existsSync(routerSrc)) {
  fs.copyFileSync(routerSrc, routerDest)
  console.log('router.php copied into dist/')
}
console.log('Hosting files copied into dist/ (installer, api, index.php, uploads, router).')
