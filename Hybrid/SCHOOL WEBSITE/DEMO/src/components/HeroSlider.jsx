import { useEffect, useState } from 'react'
import { HERO_SLIDES } from '../data/site.js'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function HeroSlider() {
  const { heroSlides } = useApp()
  const { slideText } = useLang()
  const slides = (heroSlides?.length ? heroSlides : HERO_SLIDES).map(slideText)
  const [index, setIndex] = useState(0)
  const total = slides.length

  useEffect(() => {
    setIndex(0)
  }, [total])

  useEffect(() => {
    if (!total) return undefined
    const timer = setInterval(() => setIndex((i) => (i + 1) % total), 5500)
    return () => clearInterval(timer)
  }, [total])

  function go(next) {
    setIndex((i) => (i + next + total) % total)
  }

  if (!total) return null

  return (
    <div className="hero-slider">
      {slides.map((slide, i) => (
        <article key={slide.id || i} className={`hero-slide ${i === index ? 'is-active' : ''}`}>
          <img src={slide.image} alt={slide.title} />
          <div className="hero-overlay">
            <div className="container hero-caption">
              <p className="eyebrow">{slide.eyebrow}</p>
              <h1>{slide.title}</h1>
              <p>{slide.text}</p>
            </div>
          </div>
        </article>
      ))}
      <button type="button" className="hero-arrow is-prev" aria-label="Previous slide" onClick={() => go(-1)}>
        ‹
      </button>
      <button type="button" className="hero-arrow is-next" aria-label="Next slide" onClick={() => go(1)}>
        ›
      </button>
      <div className="hero-dots">
        {slides.map((slide, i) => (
          <button
            key={slide.id || i}
            type="button"
            className={i === index ? 'is-active' : ''}
            aria-label={`Slide ${i + 1}`}
            onClick={() => setIndex(i)}
          />
        ))}
      </div>
    </div>
  )
}
