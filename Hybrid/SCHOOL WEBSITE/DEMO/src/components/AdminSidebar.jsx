import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import {
  IconClose,
  IconDoc,
  IconGrid,
  IconHouse,
  IconImage,
  IconImages,
  IconInfo,
  IconLogout,
  IconMail,
  IconNewsletter,
  IconPhone,
  IconShare,
  IconUser,
} from './Icons.jsx'

const ITEMS = [
  { id: 'profile', icon: IconUser, labelKey: 'admin.profile' },
  { id: 'projects', icon: IconGrid, labelKey: 'admin.projects' },
  { id: 'slider', icon: IconImage, labelKey: 'admin.homeSlider' },
  { id: 'footer', icon: IconImages, labelKey: 'admin.footerGallery' },
  { id: 'blog', icon: IconDoc, labelKey: 'admin.blog' },
  { id: 'about', icon: IconInfo, labelKey: 'admin.about' },
  { id: 'messages', icon: IconMail, labelKey: 'admin.messages' },
  { id: 'subscribers', icon: IconNewsletter, labelKey: 'admin.subscribers' },
  { id: 'socials', icon: IconShare, labelKey: 'admin.socials' },
  { id: 'contact', icon: IconPhone, labelKey: 'admin.contact' },
]

export default function AdminSidebar({ tab, onTab, open, onClose, onLogout }) {
  const { adminProfile } = useApp()
  const { t, lang, setLang } = useLang()

  return (
    <aside className={`admin-sidebar ${open ? 'is-open' : ''}`}>
      <div className="admin-sidebar-top">
        <Link to="/" className="admin-brand" onClick={onClose}>
          <img src="/logo.svg" alt="" width="40" height="40" />
          <span>
            <strong>SAFA</strong>
            <small>PROPERTIS</small>
          </span>
        </Link>
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
        <small>{t('admin.administrator')}</small>
        <button type="button" className="admin-logout" onClick={onLogout}>
          <IconLogout />
          {t('admin.logout')}
        </button>
      </div>

      <nav className="admin-side-nav">
        {ITEMS.map((item) => {
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
              <Icon />
              <span>{t(item.labelKey)}</span>
            </button>
          )
        })}
      </nav>

      <div className="admin-sidebar-foot">
        <div className="lang-switch" role="group" aria-label="Language">
          <button type="button" className={lang === 'bn' ? 'is-active' : ''} onClick={() => setLang('bn')}>
            BN
          </button>
          <button type="button" className={lang === 'en' ? 'is-active' : ''} onClick={() => setLang('en')}>
            EN
          </button>
        </div>
        <Link to="/" className="admin-view-site" onClick={onClose}>
          <IconHouse />
          {t('admin.viewSite')}
        </Link>
      </div>
    </aside>
  )
}
