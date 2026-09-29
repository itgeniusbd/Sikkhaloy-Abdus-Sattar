export function youtubeId(input) {
  const text = String(input || '').trim()
  if (!text) return ''
  if (/^[\w-]{11}$/.test(text)) return text
  const raw = /^https?:\/\//i.test(text) ? text : `https://${text}`
  try {
    const url = new URL(raw)
    if (url.hostname.includes('youtu.be')) return url.pathname.split('/').filter(Boolean)[0] || ''
    if (url.pathname.includes('/embed/')) return url.pathname.split('/embed/')[1]?.split(/[/?#]/)[0] || ''
    if (url.pathname.includes('/shorts/')) return url.pathname.split('/shorts/')[1]?.split(/[/?#]/)[0] || ''
    return url.searchParams.get('v') || ''
  } catch {
    const match = text.match(/(?:v=|\/embed\/|youtu\.be\/|\/shorts\/)([\w-]{11})/)
    return match?.[1] || ''
  }
}

export function youtubeEmbed(input) {
  const id = youtubeId(input)
  return id ? `https://www.youtube.com/embed/${id}` : ''
}

export function youtubeThumb(input) {
  const id = youtubeId(input)
  return id ? `https://img.youtube.com/vi/${id}/hqdefault.jpg` : ''
}
