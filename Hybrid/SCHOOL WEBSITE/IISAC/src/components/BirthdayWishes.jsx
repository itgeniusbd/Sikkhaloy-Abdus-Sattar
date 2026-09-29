import { useEffect, useState } from 'react'

function visibleCount() {
  if (typeof window === 'undefined') return 3
  if (window.innerWidth < 640) return 1
  if (window.innerWidth < 980) return 2
  return 3
}

function chunk(list, size) {
  const pages = []
  for (let i = 0; i < list.length; i += size) pages.push(list.slice(i, size + i))
  return pages
}

function WishCard({ person, rollLabel }) {
  const role = [person.title, person.meta ? `${rollLabel || ''} ${person.meta}`.trim() : '']
    .filter(Boolean)
    .join(' · ')
  return (
    <article className="wish-card">
      <div className="wish-stage">
        <span className="wish-balloon is-teal" aria-hidden="true" />
        <span className="wish-balloon is-silver" aria-hidden="true" />
        <p className="wish-script">
          Happy
          <br />
          Birthday
        </p>
        <div className="wish-hex">
          <div className="wish-photo">
            {person.photoUrl ? (
              <img src={person.photoUrl} alt={person.name} />
            ) : (
              <span>{person.name.slice(0, 1)}</span>
            )}
          </div>
        </div>
      </div>
      <h3>{person.name}</h3>
      {role ? <p className="wish-role">{role}</p> : null}
    </article>
  )
}

export default function BirthdayWishes({ people = [], eyebrow, title, rollLabel }) {
  const items = Array.isArray(people) ? people.filter((person) => person?.name) : []
  const [perView, setPerView] = useState(() => visibleCount())
  const [page, setPage] = useState(0)
  const pages = chunk(items, perView)
  const last = Math.max(0, pages.length - 1)

  useEffect(() => {
    const update = () => setPerView(visibleCount())
    update()
    window.addEventListener('resize', update)
    return () => window.removeEventListener('resize', update)
  }, [])

  useEffect(() => {
    setPage(0)
  }, [perView, items.length])

  useEffect(() => {
    if (pages.length < 2) return undefined
    const timer = setInterval(() => setPage((current) => (current >= last ? 0 : current + 1)), 4500)
    return () => clearInterval(timer)
  }, [pages.length, last])

  if (!items.length) return null

  return (
    <section className="section birthday-block">
      <div className="container">
        {eyebrow ? <p className="eyebrow">{eyebrow}</p> : null}
        <h2 className="section-title">{title}</h2>
        <div className="people-slider wish-slider">
          <button
            type="button"
            className="slider-nav"
            aria-label="Previous"
            disabled={pages.length < 2}
            onClick={() => setPage((current) => (current <= 0 ? last : current - 1))}
          >
            ‹
          </button>
          <div className="people-slider-window">
            <div className="people-slider-track" style={{ transform: `translateX(-${page * 100}%)` }}>
              {pages.map((group, i) => (
                <div key={i} className="people-slider-page" style={{ gridTemplateColumns: `repeat(${perView}, 1fr)` }}>
                  {group.map((person) => (
                    <WishCard key={`${person.kind || 'p'}-${person.id || person.name}`} person={person} rollLabel={rollLabel} />
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
            onClick={() => setPage((current) => (current >= last ? 0 : current + 1))}
          >
            ›
          </button>
        </div>
      </div>
    </section>
  )
}
