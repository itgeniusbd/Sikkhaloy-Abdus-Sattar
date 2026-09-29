import { IconFacebook, IconInstagram, IconLinkedIn, IconWhatsApp, IconX } from '../components/Icons.jsx'

export const SOCIAL_NETWORKS = [
  { id: 'facebook', labelKey: 'admin.socialFacebook', placeholder: 'https://facebook.com/yourpage' },
  { id: 'x', labelKey: 'admin.socialX', placeholder: 'https://x.com/yourpage' },
  { id: 'linkedin', labelKey: 'admin.socialLinkedin', placeholder: 'https://linkedin.com/company/yourpage' },
  { id: 'instagram', labelKey: 'admin.socialInstagram', placeholder: 'https://instagram.com/yourpage' },
  { id: 'whatsapp', labelKey: 'admin.socialWhatsapp', placeholder: '8801XXXXXXXXX or https://wa.me/8801...' },
]

const ICONS = {
  facebook: IconFacebook,
  x: IconX,
  linkedin: IconLinkedIn,
  instagram: IconInstagram,
  whatsapp: IconWhatsApp,
}

export function socialIcon(id) {
  return ICONS[id]
}

export function socialsFromSettings(settings = {}) {
  return {
    facebook: settings.social_facebook || '',
    x: settings.social_x || '',
    linkedin: settings.social_linkedin || '',
    instagram: settings.social_instagram || '',
    whatsapp: settings.social_whatsapp || '',
  }
}

export function socialHref(id, value) {
  const raw = String(value || '').trim()
  if (!raw || raw === '#') return ''
  if (id === 'whatsapp') {
    if (/^https?:\/\//i.test(raw)) return raw
    const digits = raw.replace(/\D/g, '')
    return digits ? `https://wa.me/${digits}` : ''
  }
  if (/^https?:\/\//i.test(raw)) return raw
  return `https://${raw}`
}
