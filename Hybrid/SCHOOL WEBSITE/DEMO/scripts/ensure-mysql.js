import { spawn } from 'node:child_process'
import net from 'node:net'
import fs from 'node:fs'

const host = process.env.DB_HOST || 'localhost'
const port = Number(process.env.DB_PORT || 3306)
const mysqld = 'C:\\Program Files\\MariaDB 12.3\\bin\\mysqld.exe'
const defaults = 'C:\\Program Files\\MariaDB 12.3\\data\\my.ini'

function canConnect() {
  return new Promise((resolve) => {
    const socket = net.connect({ host, port })
    socket.setTimeout(800)
    socket.on('connect', () => {
      socket.end()
      resolve(true)
    })
    socket.on('error', () => resolve(false))
    socket.on('timeout', () => {
      socket.destroy()
      resolve(false)
    })
  })
}

async function waitForMysql(tries = 20) {
  for (let i = 0; i < tries; i += 1) {
    if (await canConnect()) return true
    await new Promise((r) => setTimeout(r, 500))
  }
  return false
}

if (await canConnect()) {
  console.log('MariaDB already running on port', port)
  process.exit(0)
}

if (!fs.existsSync(mysqld)) {
  console.error('MariaDB not found at', mysqld)
  process.exit(1)
}

console.log('Starting MariaDB...')
const child = spawn(mysqld, [`--defaults-file=${defaults}`], {
  detached: true,
  stdio: 'ignore',
  windowsHide: true,
})
child.unref()

if (await waitForMysql()) {
  console.log('MariaDB is ready')
  process.exit(0)
}

console.error('MariaDB did not start. Open mysqld.exe manually.')
process.exit(1)
