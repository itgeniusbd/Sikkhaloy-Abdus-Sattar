import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

function chunk(list, size) {
  const pages = []
  for (let i = 0; i < list.length; i += size) pages.push(list.slice(i, size + i))
  return pages
}

export default function GallerySlider({ items = [], emptyLabel }) {
  const [perPage, setPerPage] = useState(4)
  const [page, setPage] = useState(0)
  const photos = (Array.isArray(items) ? items : []).filter((item) => item?.image)

  useEffect(() => {
    const update = () => {
      if (window.innerWidth < 640) setPerPage(1)
      else if (window.innerWidth < 900) setPerPage(2)
      else setPerPage(4)
    }
    update()
    window.addEventListener('resize', update)
    return () => window.removeEventListener('resize', update)
  }, [])

  useEffect(() => {
    setPage(0)
  }, [photos.length, perPage])

  const pages = chunk(photos, perPage)
  const last = Math.max(0, pages.length - 1)

  useEffect(() => {
    if (pages.length < 2) return undefined
    const timer = setInterval(() => setPage((p) => (p >= last ? 0 : p + 1)), 4200)
    return () => clearInterval(timer)
  }, [pages.length, last])

  if (!photos.length) {
    return emptyLabel ? <p className="empty">{emptyLabel}</p> : null
  }

  return (
    <div className="gallery-slider">
      <button
        type="button"
        className="slider-nav"
        aria-label="Previous photos"
        disabled={pages.length < 2}
        onClick={() => setPage((p) => (p <= 0 ? last : p - 1))}
      >
        ‹
      </button>
      <div className="gallery-slider-window">
        <div className="gallery-slider-track" style={{ transform: `translateX(-${page * 100}%)` }}>
          {pages.map((group, i) => (
            <div key={i} className="gallery-slider-page" style={{ gridTemplateColumns: `repeat(${perPage}, 1fr)` }}>
              {group.map((item, index) => {
                const inner = <img src={item.image} alt={item.title || 'Gallery'} />
                return item.link ? (
                  <Link key={`${item.image}-${index}`} to={item.link} className="gallery-slide-card">
                    {inner}
                    {item.title ? <span>{item.title}</span> : null}
                  </Link>
                ) : (
                  <figure key={`${item.image}-${index}`} className="gallery-slide-card">
                    {inner}
                    {item.title ? <figcaption>{item.title}</figcaption> : null}
                  </figure>
                )
              })}
            </div>
          ))}
        </div>
      </div>
      <button
        type="button"
        className="slider-nav"
        aria-label="Next photos"
        disabled={pages.length < 2}
        onClick={() => setPage((p) => (p >= last ? 0 : p + 1))}
      >
        ›
      </button>
    </div>
  )
}
