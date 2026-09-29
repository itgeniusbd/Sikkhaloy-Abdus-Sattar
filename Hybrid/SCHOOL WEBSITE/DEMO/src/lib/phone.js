export function digitsOnly(value) {
  return String(value || '').replace(/\D/g, '')
}

export function telHref(value) {
  const digits = digitsOnly(value)
  return digits ? `tel:+${digits}` : '#'
}

export function waHref(value, message = '') {
  const digits = digitsOnly(value)
  const text = message ? `?text=${encodeURIComponent(message)}` : ''
  return digits ? `https://wa.me/${digits}${text}` : '#'
}

export function displayPhone(value) {
  const digits = digitsOnly(value)
  if (digits.startsWith('880') && digits.length >= 13) {
    return `+${digits.slice(0, 3)} ${digits.slice(3)}`
  }
  return value || ''
}
