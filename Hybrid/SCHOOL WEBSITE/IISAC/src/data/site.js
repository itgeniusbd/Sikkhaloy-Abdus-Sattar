export const SITE = {
  name: 'Demo',
  tagline: 'Demo School Website',
  phone: '01896-203620',
  whatsapp: '8801896203620',
  email: 'iisacedu@gmail.com',
  address: '761, Tushardhara R/A, Katubpur, Fatullah, Narayanganj',
  about:
    'Demo is a sample school website. Edit the site name, logo, and pages from Admin when the education API is offline.',
}

export const HERO_SLIDES = [
  {
    id: 'welcome',
    image: '/hero/slide-1.jpg',
    eyebrow: 'DEMO',
    title: 'A Place to Learn and Grow',
    text: 'Welcome to our campus — where students, teachers, and families work together for a brighter future.',
  },
  {
    id: 'students',
    image: '/hero/slide-2.jpg',
    eyebrow: 'OUR STUDENTS',
    title: 'Every Child Matters',
    text: 'From the first class to the last, we guide students with care, discipline, and a love of learning.',
  },
  {
    id: 'teachers',
    image: '/hero/slide-3.jpg',
    eyebrow: 'OUR TEACHERS',
    title: 'Dedicated Educators',
    text: 'Experienced teachers who prepare students for exams, life skills, and confident next steps.',
  },
  {
    id: 'campus',
    image: '/hero/slide-4.jpg',
    eyebrow: 'CAMPUS LIFE',
    title: 'A Safe and Active Campus',
    text: 'Classrooms, library, sports, and cultural programs — a complete environment for young minds.',
  },
]

export const SERVICES = [
  {
    id: 'academic',
    title: 'Academic Excellence',
    desc: 'A clear syllabus, regular assessment, and personal attention so every student can progress with confidence.',
    icon: 'chart',
  },
  {
    id: 'islamic',
    title: 'Moral Education',
    desc: 'Character, manners, and values are taught alongside textbooks — so students grow as good people.',
    icon: 'bank',
  },
  {
    id: 'lab',
    title: 'Science & Computer Lab',
    desc: 'Hands-on science and ICT practice that turns classroom theory into real understanding.',
    icon: 'globe',
  },
  {
    id: 'library',
    title: 'Library & Reading',
    desc: 'A quiet reading space and curated books that build curiosity beyond the exam syllabus.',
    icon: 'house',
  },
  {
    id: 'sports',
    title: 'Sports & Culture',
    desc: 'Games, debate, and cultural programs that keep campus life active, healthy, and joyful.',
    icon: 'crane',
  },
  {
    id: 'care',
    title: 'Student Care',
    desc: 'Attendance, guardian communication, and a safe campus so families stay connected every day.',
    icon: 'land',
  },
]

export const ABOUT_FEATURES = [
  'Qualified Teaching Staff',
  'Safe Campus Environment',
  'Regular Guardian Contact',
  'Sports and Cultural Life',
]

export const DEFAULT_PAGES = {
  about: {
    image: 'https://images.unsplash.com/photo-1523050854058-8df90110c9f1?auto=format&fit=crop&w=1200&q=80',
    eyebrow: 'ABOUT US',
    title: 'Building Confident Learners',
    lead: SITE.about,
    body: 'We combine strong academics with moral guidance, sports, and a caring campus. Institution name, logo, student numbers, teachers, and current students come live from Sikkhaloy. News, photos, and pages are managed from this website.',
    features: ABOUT_FEATURES,
  },
  contact: {
    eyebrow: 'CONTACT US',
    title: 'Visit or Send a Message',
    lead: 'Come to the campus office or write to us. We reply with admission and academic information.',
    phone: SITE.phone,
    whatsapp: SITE.whatsapp,
    email: SITE.email,
    address: SITE.address,
  },
}

export const LOCATIONS = ['Campus']
export const PROPERTY_TYPES = ['General']
export const BLOG_CATEGORIES = ['News', 'Notice', 'Events', 'Academic', 'Sports', 'Admission']
export const CATEGORIES = ['Campus', 'Academic', 'Events', 'Facilities']
