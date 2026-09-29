import { useEffect, useState } from 'react'

function chunk(list, size) {
  const pages = []
  for (let i = 0; i < list.length; i += size) pages.push(list.slice(i, size + i))
  return pages
}

export default function PeopleSlider({ people = [], resetKey, emptyLabel, rollLabel }) {
  const [perPage, setPerPage] = useState(4)
  const [page, setPage] = useState(0)
  const items = Array.isArray(people) ? people.filter((p) => p?.name) : []

  useEffect(() => {
    const update = () => {
      if (window.innerWidth < 640) setPerPage(1)
      else if (window.innerWidth < 900) setPerPage(2)
      else if (window.innerWidth < 1100) setPerPage(3)
      else setPerPage(4)
    }
    update()
    window.addEventListener('resize', update)
    return () => window.removeEventListener('resize', update)
  }, [])

  useEffect(() => {
    setPage(0)
  }, [resetKey, perPage, items.length])

  const pages = chunk(items, perPage)
  const last = Math.max(0, pages.length - 1)

  useEffect(() => {
    if (pages.length < 2) return undefined
    const timer = setInterval(() => setPage((p) => (p >= last ? 0 : p + 1)), 5000)
    return () => clearInterval(timer)
  }, [pages.length, last])

  if (!items.length) {
    return emptyLabel ? <p className="empty">{emptyLabel}</p> : null
  }

  return (
    <div className="people-slider">
      <button
        type="button"
        className="slider-nav"
        aria-label="Previous"
        disabled={pages.length < 2}
        onClick={() => setPage((p) => (p <= 0 ? last : p - 1))}
      >
        ‹
      </button>
      <div className="people-slider-window">
        <div className="people-slider-track" style={{ transform: `translateX(-${page * 100}%)` }}>
          {pages.map((group, i) => (
            <div key={i} className="people-slider-page" style={{ gridTemplateColumns: `repeat(${perPage}, 1fr)` }}>
              {group.map((person) => (
                <article key={person.id || person.name} className="person-card">
                  <div className="person-photo">
                    {person.photoUrl ? (
                      <img src={person.photoUrl} alt={person.name} />
                    ) : (
                      <span className="person-fallback">{person.name.slice(0, 1)}</span>
                    )}
                  </div>
                  <h3>{person.name}</h3>
                  {person.title ? <p>{person.title}</p> : null}
                  {person.meta ? (
                    <small>
                      {rollLabel ? `${rollLabel} ${person.meta}` : person.meta}
                    </small>
                  ) : null}
                </article>
              ))}
            </div>
          ))}
        </div>
      </div>
      <button
        type="button"
        className="slider-nav"
        aria-label="Next"
        disabled={pages.length < 2}
        onClick={() => setPage((p) => (p >= last ? 0 : p + 1))}
      >
        ›
      </button>
    </div>
  )
}
