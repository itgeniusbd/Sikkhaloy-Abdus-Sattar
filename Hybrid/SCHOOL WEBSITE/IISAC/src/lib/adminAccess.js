export const ACCESS_PAGES = [
  { id: 'profile', labelKey: 'admin.profile' },
  { id: 'menus', labelKey: 'admin.menus' },
  { id: 'slider', labelKey: 'admin.homeSlider' },
  { id: 'admissions', labelKey: 'admin.admissions' },
  { id: 'career', labelKey: 'admin.career' },
  { id: 'footer', labelKey: 'admin.gallery' },
  { id: 'blog', labelKey: 'admin.blog' },
  { id: 'about', labelKey: 'admin.about' },
  { id: 'messages', labelKey: 'admin.messages' },
  { id: 'subscribers', labelKey: 'admin.subscribers' },
  { id: 'socials', labelKey: 'admin.socials' },
  { id: 'contact', labelKey: 'admin.contact' },
]

export const ALL_ACCESS = ACCESS_PAGES.map((item) => item.id)

export function isSuperAdmin(user) {
  return user?.role === 'admin'
}

export function canAccess(user, page) {
  if (!user) return false
  if (page === 'users') return isSuperAdmin(user)
  if (page === 'profile') return true
  if (page === 'careerApps') return canAccess(user, 'career')
  if (isSuperAdmin(user)) return true
  return Array.isArray(user.permissions) && user.permissions.includes(page)
}

export function firstAllowedTab(user) {
  if (isSuperAdmin(user)) return 'menus'
  const found = ACCESS_PAGES.find((item) => item.id !== 'profile' && canAccess(user, item.id))
  return found?.id || 'profile'
}

export function publicUser(row) {
  if (!row) return null
  const role = row.role === 'admin' ? 'admin' : 'subadmin'
  let permissions = []
  try {
    permissions = JSON.parse(row.permissions || '[]')
  } catch {
    permissions = []
  }
  if (!Array.isArray(permissions)) permissions = []
  return {
    id: Number(row.id || row.user_id || 0),
    username: row.username || '',
    displayName: row.display_name || row.displayName || row.username || '',
    designation: row.designation || '',
    photo: row.photo || '',
    role,
    permissions: role === 'admin' ? ALL_ACCESS : permissions.filter((item) => ALL_ACCESS.includes(item)),
    active: Number(row.active ?? 1) !== 0,
  }
}
