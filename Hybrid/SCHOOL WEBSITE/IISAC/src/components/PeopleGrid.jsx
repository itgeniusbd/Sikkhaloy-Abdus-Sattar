export default function PeopleGrid({ people = [], emptyLabel, rollLabel, variant }) {
  const items = Array.isArray(people) ? people.filter((person) => person?.name) : []
  if (!items.length) {
    return emptyLabel ? <p className="empty">{emptyLabel}</p> : null
  }

  return (
    <div className={`people-grid ${variant === 'official' ? 'is-official' : ''}`}>
      {items.map((person) => (
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
          {person.qualification ? <small className="person-qual">{person.qualification}</small> : null}
          {person.meta ? <small>{rollLabel ? `${rollLabel} ${person.meta}` : person.meta}</small> : null}
        </article>
      ))}
    </div>
  )
}
