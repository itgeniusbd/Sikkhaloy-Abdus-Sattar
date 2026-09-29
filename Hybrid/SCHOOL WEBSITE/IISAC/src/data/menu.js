const FACULTY_IDS = new Set(['teachers', 'teacher-day', 'teacher-morning', 'office-staff'])

export function isFacultySlug(value) {
  return FACULTY_IDS.has(String(value || '').trim().toLowerCase())
}

function isFacultyItem(item) {
  return Boolean(item && (item.type === 'teachers' || isFacultySlug(item.id) || isFacultySlug(item.slug)))
}

function facultyMenuItem(from) {
  const keepLabel = from?.label && !/day shift|morning shift|office staff/i.test(from.label)
  const keepLabelBn = from?.labelBn && !/দিবা|প্রভাতী|অফিস স্টাফ/.test(from.labelBn)
  return {
    id: 'teachers',
    slug: 'teachers',
    label: keepLabel ? from.label : 'Teachers & Staff',
    labelBn: keepLabelBn ? from.labelBn : 'শিক্ষক ও কর্মচারী',
    type: 'teachers',
    visible: from?.visible !== false,
    children: [],
  }
}

function insertFaculty(children, faculty) {
  const index = children.findIndex((child) => child.id === 'admin-body' || child.slug === 'admin-body')
  const next = [...children]
  if (index >= 0) next.splice(index + 1, 0, faculty)
  else next.push(faculty)
  return next
}

function withCareerMenu(items = []) {
  const list = [...(items || [])]
  const hasCareer = flattenMenu(list).some((item) => item.type === 'career' || item.id === 'career' || item.slug === 'career')
  if (hasCareer) return list
  const career = item('career', 'Career', 'কেরিয়ার', 'career')
  const contactIdx = list.findIndex((row) => row.type === 'contact' || row.id === 'contact' || row.slug === 'contact')
  if (contactIdx >= 0) list.splice(contactIdx, 0, career)
  else list.push(career)
  return list
}

export function normalizeMenus(items = []) {
  return withCareerMenu((items || []).map((item) => {
    if (isFacultyItem(item) && !(item.children || []).length) {
      return facultyMenuItem(item)
    }
    const children = item.children || []
    const facultySource = children.find(isFacultyItem)
    const rest = children.filter((child) => !isFacultyItem(child))
    const nextChildren = facultySource ? insertFaculty(rest, facultyMenuItem(facultySource)) : rest
    return {
      ...item,
      type:
        item.id === 'career' || item.slug === 'career'
          ? 'career'
          : item.id === 'events' || item.slug === 'events'
            ? 'events'
            : item.type,
      children: nextChildren.map((child) => ({
        ...child,
        type:
          child.id === 'events' || child.slug === 'events'
            ? 'events'
            : child.id === 'video-gallery' || child.slug === 'video-gallery'
              ? 'videos'
              : child.id === 'career' || child.slug === 'career'
                ? 'career'
                : child.type,
      })),
    }
  }))
}

function item(id, label, labelBn, type, extra = {}) {
  return { id, slug: extra.slug || id, label, labelBn, type, visible: true, children: extra.children || [], ...extra }
}

export const DEFAULT_MENU = [
  item('home', 'Home', 'হোম', 'home'),
  item('about', 'About Us', 'আমাদের সম্পর্কে', 'group', {
    children: [
      item('glance', 'At a Glance', 'এক নজরে', 'page'),
      item('achievement', 'Achievement', 'অর্জন', 'page'),
      item('history', 'History', 'ইতিহাস', 'page'),
      item('why-iisac', 'Why study here', 'কেন এখানে পড়বেন', 'page'),
      item('events', 'Events', 'ইভেন্ট', 'events'),
    ],
  }),
  item('administrations', 'Administrations', 'প্রশাসন', 'group', {
    children: [
      item('chairman-message', "Chairman's Message", 'চেয়ারম্যানের বাণী', 'page'),
      item('principal-message', "Principal's Message", 'অধ্যক্ষের বাণী', 'page'),
      item('admin-body', 'Administrative Body', 'প্রশাসনিক পরিষদ', 'page'),
      item('teachers', 'Teachers & Staff', 'শিক্ষক ও কর্মচারী', 'teachers'),
    ],
  }),
  item('academics', 'Academics', 'একাডেমিক', 'group', {
    children: [
      item('rules-regulations', 'Rules Regulations', 'নিয়মাবলি', 'page'),
      item('results', 'Results', 'ফলাফল', 'page'),
      item('dress-code', 'Dress Code', 'পোশাক বিধি', 'page'),
    ],
  }),
  item('admission', 'Admission', 'ভর্তি', 'group', {
    children: [
      item('admission-info', 'Admission Info', 'ভর্তি তথ্য', 'page'),
      item('apply-online', 'Apply online', 'অনলাইন আবেদন', 'page'),
    ],
  }),
  item('facilities', 'Facilities', 'সুবিধাসমূহ', 'page'),
  item('gallery', 'Gallery', 'গ্যালারি', 'group', {
    children: [
      item('photo-gallery', 'Photo Gallery', 'ফটো গ্যালারি', 'gallery'),
      item('video-gallery', 'Video Gallery', 'ভিডিও গ্যালারি', 'videos'),
    ],
  }),
  item('notice', 'Notice', 'নোটিশ', 'notice'),
  item('career', 'Career', 'কেরিয়ার', 'career'),
  item('contact', 'Contact', 'যোগাযোগ', 'contact'),
]

function page(title, titleBn, body, bodyBn = '', extra = {}) {
  return {
    title,
    titleBn,
    eyebrow: title,
    eyebrowBn: titleBn,
    lead: '',
    leadBn: '',
    body,
    bodyBn: bodyBn || body,
    image: '',
    ...extra,
  }
}

export const DEFAULT_CMS_PAGES = {
  glance: page(
    'At a Glance',
    'এক নজরে',
    'Imperial Ideal School & College (IISAC), EIIN 139997, is a well-known educational institute established in 2006 at Tushardhara R/A, Fatullah, Narayanganj. It has separate boys and girls campuses and maintains an inclusive learning environment.',
    'ইম্পেরিয়াল আইডিয়াল স্কুল অ্যান্ড কলেজ (IISAC), EIIN ১৩৯৯৯৭, ২০০৬ সালে নারায়ণগঞ্জের ফতুল্লায় তুষারধারা আর/এ-তে প্রতিষ্ঠিত একটি পরিচিত শিক্ষাপ্রতিষ্ঠান। এখানে আলাদা বালক ও বালিকা ক্যাম্পাস রয়েছে।',
  ),
  achievement: page('Achievement', 'অর্জন', 'Write the institution’s academic and extra-curricular achievements here from Admin → Menus & Pages.'),
  history: page('History', 'ইতিহাস', 'IISAC was established in 2006. Add the full history, founders, and campus growth from the admin dashboard.'),
  'why-iisac': page('Why study here', 'কেন এখানে পড়বেন', 'Add reasons for choosing IISAC — academics, values, campus care, and results — from the admin dashboard.'),
  events: page('Events', 'ইভেন্ট', 'Publish campus events, tours, and celebrations here. You can update this page anytime from Admin.'),
  'chairman-message': page(
    "Chairman's Message",
    'চেয়ারম্যানের বাণী',
    'Greeting from the Chairman. Imperial Ideal School and College considers every child as unique and maintains an inclusive learning-teaching environment.\n\nDr. M A Taleb\nChairman\nImperial Ideal School & College',
  ),
  'principal-message': page(
    "Principal's Message",
    'অধ্যক্ষের বাণী',
    'Message from the Principal. Add the full welcome note from Admin → Menus & Pages.\n\nMd. Asaduzzaman\nPrincipal\nImperial Ideal School & College',
  ),
  'admin-body': page(
    'Administrative Body',
    'প্রশাসনিক পরিষদ',
    '',
    '',
    {
      members: [
        {
          id: 'taleb',
          name: 'Dr. M A Taleb',
          nameBn: 'ড. এম এ তালেব',
          title: 'Chairman - Imperial Ideal School & College',
          titleBn: 'চেয়ারম্যান - ইম্পেরিয়াল আইডিয়াল স্কুল অ্যান্ড কলেজ',
          qualification: '',
          qualificationBn: '',
          image: '',
        },
        {
          id: 'asaduzzaman',
          name: 'Md. Asaduzzaman',
          nameBn: 'মো. আসাদুজ্জামান',
          title: 'Founding Director & Principal - Imperial Ideal School & College',
          titleBn: 'প্রতিষ্ঠাতা পরিচালক ও অধ্যক্ষ - ইম্পেরিয়াল আইডিয়াল স্কুল অ্যান্ড কলেজ',
          qualification: 'B.A (Honours) , M.A (English)-JnU, B.Ed (First Class)-NU, Advanced Research Methodology - DU',
          qualificationBn: 'বি.এ (সম্মান), এম.এ (ইংরেজি)-JnU, বি.এড (প্রথম শ্রেণি)-NU, Advanced Research Methodology - DU',
          image: '',
        },
      ],
    },
  ),
  'rules-regulations': page('Rules & Regulations', 'নিয়মাবলি', 'Add campus rules, class discipline, and guardian guidelines from the admin dashboard.'),
  results: page('Results', 'ফলাফল', 'Publish public exam and internal result notices here from the admin dashboard.'),
  'dress-code': page('Dress Code', 'পোশাক বিধি', 'Describe the student uniform and dress code from the admin dashboard.'),
  'admission-info': page(
    'Admission Info',
    'ভর্তি তথ্য',
    'Admission information, required papers, and office hours can be edited from Admin → Menus & Pages.\n\nCampus: 761, Tushardhara R/A, Katubpur, Fatullah, Narayanganj',
  ),
  'apply-online': page('Apply Online', 'অনলাইন আবেদন', 'The Sikkhaloy new-admission form is live on this page. Submitted applications appear in Admin → Online Admission.'),
  facilities: page(
    'Our Facilities',
    'আমাদের সুবিধাসমূহ',
    '1. Transportation — safe and comfortable bus service.\n2. Multimedia Classrooms — smart boards and projectors.\n3. On-Campus Medical Care — first aid when needed.\n4. Modern Library — books, e-resources, and a reading room.\n5. Advanced Computer Lab — internet and modern computers.\n\nEdit this list from Admin → Menus & Pages.',
  ),
  'video-gallery': page('Video Gallery', 'ভিডিও গ্যালারি', 'Paste YouTube links or embed codes in this page body from the admin dashboard.'),
}

export function isAdminBodyHint(text) {
  return /add names, designations, and photos of the administrative body/i.test(String(text || ''))
}

export function menuLabel(item, lang, schoolName = 'Demo') {
  if (!item) return ''
  const raw = lang === 'bn' && item.labelBn ? item.labelBn : item.label
  const name = String(schoolName || 'Demo').trim() || 'Demo'
  return String(raw || '')
    .replace(/\bIISAC\b/gi, name)
    .replace(/\bSRAC\b/g, name)
}

export function menuPath(item) {
  if (!item) return '/'
  if (item.type === 'home') return '/'
  if (item.type === 'gallery') return '/gallery'
  if (item.type === 'videos' || item.slug === 'video-gallery' || item.id === 'video-gallery') return '/videos'
  if (item.type === 'notice' || item.type === 'notices') return '/notices'
  if (item.type === 'events' || item.slug === 'events' || item.id === 'events') return '/events'
  if (item.type === 'teachers' || isFacultySlug(item.slug) || isFacultySlug(item.id)) return '/teachers'
  if (item.type === 'contact') return '/contact'
  if (item.type === 'career' || item.slug === 'career' || item.id === 'career') return '/career'
  if (item.type === 'group') {
    const first = (item.children || []).find((child) => child.visible !== false)
    return first ? menuPath(first) : '#'
  }
  return `/p/${item.slug || item.id}`
}

export function flattenMenu(items = []) {
  const out = []
  items.forEach((item) => {
    out.push(item)
    ;(item.children || []).forEach((child) => out.push(child))
  })
  return out
}

export function findMenuItem(items = [], slug) {
  return flattenMenu(items).find((item) => item.slug === slug || item.id === slug) || null
}

export function visibleMenu(items = []) {
  return (items || [])
    .filter((item) => item.visible !== false)
    .map((item) => ({
      ...item,
      children: (item.children || []).filter((child) => child.visible !== false),
    }))
}

export function newMenuId(label) {
  const base = String(label || 'page')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '') || 'page'
  return `${base}-${Date.now().toString(36).slice(-4)}`
}
