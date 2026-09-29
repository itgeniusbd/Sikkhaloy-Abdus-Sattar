import { useState } from 'react'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import AdminThemePanel from './AdminThemePanel.jsx'
import {
  IconClose,
  IconDoc,
  IconGear,
  IconImage,
  IconImages,
  IconInfo,
  IconMail,
  IconNewsletter,
  IconPhone,
  IconShare,
  IconUser,
} from './Icons.jsx'

const ITEMS = [
  { id: 'profile', icon: IconUser, labelKey: 'admin.profile' },
  { id: 'menus', icon: IconDoc, labelKey: 'admin.menus' },
  { id: 'slider', icon: IconImage, labelKey: 'admin.homeSlider' },
  { id: 'admissions', icon: IconUser, labelKey: 'admin.admissions' },
  { id: 'career', icon: IconDoc, labelKey: 'admin.career' },
  { id: 'careerApps', icon: IconUser, labelKey: 'admin.careerApps' },
  { id: 'footer', icon: IconImages, labelKey: 'admin.gallery' },
  { id: 'blog', icon: IconDoc, labelKey: 'admin.blog' },
  { id: 'about', icon: IconInfo, labelKey: 'admin.about' },
  { id: 'messages', icon: IconMail, labelKey: 'admin.messages' },
  { id: 'subscribers', icon: IconNewsletter, labelKey: 'admin.subscribers' },
  { id: 'socials', icon: IconShare, labelKey: 'admin.socials' },
  { id: 'contact', icon: IconPhone, labelKey: 'admin.contact' },
  { id: 'users', icon: IconUser, labelKey: 'admin.users' },
]

export default function AdminSidebar({ tab, onTab, open, onClose }) {
  const { adminProfile, canAccess, isSuperAdmin } = useApp()
  const { t } = useLang()
  const [themeOpen, setThemeOpen] = useState(false)

  return (
    <aside className={`admin-sidebar ${open ? 'is-open' : ''}`}>
      <div className="admin-sidebar-top">
        <button type="button" className="admin-sidebar-close" onClick={onClose} aria-label={t('nav.menu')}>
          <IconClose />
        </button>
      </div>

      <div className="admin-profile">
        <button
          type="button"
          className="admin-avatar"
          onClick={() => {
            onTab('profile')
            onClose()
          }}
        >
          <span className="admin-avatar-orb is-left" aria-hidden="true" />
          <span className="admin-avatar-orb is-right" aria-hidden="true" />
          <span className="admin-avatar-ring" aria-hidden="true" />
          <img
            src={adminProfile?.photo || '/admin-avatar.png'}
            alt={adminProfile?.name || t('admin.administrator')}
          />
        </button>
        <strong>{adminProfile?.name || t('admin.administrator')}</strong>
        <small className="admin-designation">
          {adminProfile?.designation || (isSuperAdmin ? t('admin.administrator') : t('admin.subAdmin'))}
        </small>
      </div>

      <nav className="admin-side-nav">
        {ITEMS.filter((item) => canAccess?.(item.id)).map((item) => {
          const Icon = item.icon
          return (
            <button
              key={item.id}
              type="button"
              className={tab === item.id ? 'is-active' : ''}
              onClick={() => {
                onTab(item.id)
                onClose()
              }}
            >
              <span className="admin-nav-ico">
                <Icon />
              </span>
              <span>{t(item.labelKey)}</span>
            </button>
          )
        })}
      </nav>

      <div className="admin-sidebar-foot">
        {themeOpen ? <AdminThemePanel onClose={() => setThemeOpen(false)} /> : null}
        <button
          type="button"
          className={`admin-theme-btn ${themeOpen ? 'is-open' : ''}`}
          onClick={(e) => {
            e.preventDefault()
            setThemeOpen((open) => !open)
          }}
          aria-label={t('admin.theme')}
        >
          <IconGear />
          <span>{t('admin.theme')}</span>
        </button>
      </div>
    </aside>
  )
}
