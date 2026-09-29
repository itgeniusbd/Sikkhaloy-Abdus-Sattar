import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'
import { DEFAULT_CMS_PAGES, flattenMenu, menuPath, newMenuId } from '../data/menu.js'

const TYPE_META = {
  home: { label: 'Home', labelBn: 'হোম' },
  page: { label: 'Page', labelBn: 'পাতা' },
  group: { label: 'Dropdown', labelBn: 'ড্রপডাউন' },
  gallery: { label: 'Photo Gallery', labelBn: 'ফটো গ্যালারি' },
  videos: { label: 'Video Gallery', labelBn: 'ভিডিও গ্যালারি' },
  notice: { label: 'Notice', labelBn: 'নোটিশ' },
  notices: { label: 'Latest Notices', labelBn: 'সাম্প্রতিক নোটিশ' },
  events: { label: 'Events', labelBn: 'ইভেন্ট' },
  teachers: { label: 'Teachers & Staff', labelBn: 'শিক্ষক ও কর্মচারী' },
  contact: { label: 'Contact', labelBn: 'যোগাযোগ' },
  career: { label: 'Career', labelBn: 'কেরিয়ার' },
}

function cloneMenu(items) {
  return JSON.parse(JSON.stringify(items || []))
}

function mergeCmsPages(stored) {
  const next = { ...DEFAULT_CMS_PAGES }
  Object.entries(stored || {}).forEach(([key, page]) => {
    const base = DEFAULT_CMS_PAGES[key] || {}
    next[key] = {
      ...base,
      ...page,
      members: Array.isArray(page?.members) ? page.members : base.members || [],
    }
  })
  return next
}

function typeName(type, lang) {
  const meta = TYPE_META[type] || TYPE_META.page
  return lang === 'bn' ? meta.labelBn : meta.label
}

function parentOf(tree, id) {
  return tree.find((item) => (item.children || []).some((child) => child.id === id)) || null
}

export default function AdminMenus() {
  const { menus, cmsPages, saveMenus } = useApp()
  const { t, lang } = useLang()
  const [tree, setTree] = useState(() => cloneMenu(menus))
  const [pages, setPages] = useState(() => mergeCmsPages(cmsPages))
  const [selectedId, setSelectedId] = useState(
    flattenMenu(menus).find((item) => item.type === 'page')?.id || menus[0]?.id || '',
  )
  const [imageFile, setImageFile] = useState(null)
  const [memberFiles, setMemberFiles] = useState({})
  const [fileKey, setFileKey] = useState(0)
  const [busy, setBusy] = useState(false)
  const [saved, setSaved] = useState(false)
  const [error, setError] = useState('')
  const [langTab, setLangTab] = useState('en')
  const [advanced, setAdvanced] = useState(false)

  useEffect(() => {
    setTree(cloneMenu(menus))
  }, [menus])

  useEffect(() => {
    setPages(mergeCmsPages(cmsPages))
  }, [cmsPages])

  const flat = useMemo(() => flattenMenu(tree), [tree])
  const selected = flat.find((item) => item.id === selectedId) || null
  const parent = selected ? parentOf(tree, selected.id) : null
  const pageKey = selected ? selected.slug || selected.id : ''
  const page = selected
    ? pages[pageKey] || {
        title: selected.label,
        titleBn: selected.labelBn,
        body: '',
        bodyBn: '',
        image: '',
        lead: '',
        leadBn: '',
        eyebrow: '',
        eyebrowBn: '',
      }
    : null
  const preview = imageFile ? URL.createObjectURL(imageFile) : page?.image
  const isContent = selected?.type === 'page'
  const showMembers = pageKey === 'admin-body' || selected?.id === 'admin-body' || Array.isArray(page?.members)
  const members = showMembers ? page?.members || [] : []
  const hintKey = selected && !isContent ? `admin.hint${selected.type[0].toUpperCase()}${selected.type.slice(1)}` : ''

  function patchItem(id, patch) {
    setSaved(false)
    setTree((current) =>
      current.map((item) => {
        if (item.id === id) return { ...item, ...patch }
        return {
          ...item,
          children: (item.children || []).map((child) => (child.id === id ? { ...child, ...patch } : child)),
        }
      }),
    )
  }

  function patchPage(slug, patch) {
    setSaved(false)
    setPages((current) => ({ ...current, [slug]: { ...(current[slug] || {}), ...patch } }))
  }

  function patchMember(id, patch) {
    patchPage(pageKey, {
      members: members.map((member) => (member.id === id ? { ...member, ...patch } : member)),
    })
  }

  function addMember() {
    const id = `m-${Date.now().toString(36)}`
    patchPage(pageKey, {
      members: [
        ...members,
        {
          id,
          name: '',
          nameBn: '',
          title: '',
          titleBn: '',
          qualification: '',
          qualificationBn: '',
          image: '',
        },
      ],
    })
  }

  function removeMember(id) {
    patchPage(pageKey, { members: members.filter((member) => member.id !== id) })
    setMemberFiles((current) => {
      const next = { ...current }
      delete next[id]
      return next
    })
  }

  function moveMember(id, dir) {
    const index = members.findIndex((member) => member.id === id)
    const nextIndex = index + dir
    if (index < 0 || nextIndex < 0 || nextIndex >= members.length) return
    const next = [...members]
    ;[next[index], next[nextIndex]] = [next[nextIndex], next[index]]
    patchPage(pageKey, { members: next })
  }

  function addTop() {
    const id = newMenuId('menu')
    const next = { id, slug: id, label: 'New menu', labelBn: 'নতুন মেনু', type: 'page', visible: true, children: [] }
    setTree((current) => [...current, next])
    setPages((current) => ({ ...current, [id]: { title: 'New menu', titleBn: 'নতুন মেনু', body: '', bodyBn: '', image: '' } }))
    setSelectedId(id)
    setSaved(false)
  }

  function addChild(parentId) {
    const id = newMenuId('page')
    const next = { id, slug: id, label: 'New page', labelBn: 'নতুন পাতা', type: 'page', visible: true, children: [] }
    setTree((current) =>
      current.map((item) =>
        item.id === parentId
          ? { ...item, type: item.type === 'page' ? 'group' : item.type, children: [...(item.children || []), next] }
          : item,
      ),
    )
    setPages((current) => ({ ...current, [id]: { title: 'New page', titleBn: 'নতুন পাতা', body: '', bodyBn: '', image: '' } }))
    setSelectedId(id)
    setSaved(false)
  }

  function removeItem(id) {
    if (!window.confirm(t('admin.deleteMenuConfirm'))) return
    setTree((current) =>
      current
        .filter((item) => item.id !== id)
        .map((item) => ({ ...item, children: (item.children || []).filter((child) => child.id !== id) })),
    )
    setSelectedId(tree.find((item) => item.id !== id)?.id || '')
    setSaved(false)
  }

  function move(id, dir, parentId) {
    const shift = (list) => {
      const index = list.findIndex((item) => item.id === id)
      const next = index + dir
      if (index < 0 || next < 0 || next >= list.length) return list
      const copy = [...list]
      ;[copy[index], copy[next]] = [copy[next], copy[index]]
      return copy
    }
    setSaved(false)
    setTree((current) => {
      if (!parentId) return shift(current)
      return current.map((item) => (item.id === parentId ? { ...item, children: shift(item.children || []) } : item))
    })
  }

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setSaved(false)
    try {
      await saveMenus(
        tree,
        pages,
        selected ? { slug: pageKey, file: imageFile } : null,
        memberFiles,
      )
      setImageFile(null)
      setMemberFiles({})
      setFileKey((k) => k + 1)
      setSaved(true)
    } catch (err) {
      setError(err.message || t('admin.menusError'))
    } finally {
      setBusy(false)
    }
  }

  function renderRow(item, index, list, parentId) {
    const active = selectedId === item.id
    const hidden = item.visible === false
    return (
      <div key={item.id} className={`menu-row ${active ? 'is-active' : ''} ${hidden ? 'is-hidden' : ''} ${parentId ? 'is-child' : ''}`}>
        <button type="button" className="menu-row-main" onClick={() => setSelectedId(item.id)}>
          <strong>{lang === 'bn' && item.labelBn ? item.labelBn : item.label}</strong>
          <small>
            {typeName(item.type, lang)}
            {!parentId && item.children?.length ? ` · ${item.children.length}` : ''}
            {hidden ? ` · ${t('admin.hidden')}` : ''}
          </small>
        </button>
        <div className="menu-row-tools">
          <button type="button" aria-label="Up" disabled={index === 0} onClick={() => move(item.id, -1, parentId)}>
            ↑
          </button>
          <button type="button" aria-label="Down" disabled={index === list.length - 1} onClick={() => move(item.id, 1, parentId)}>
            ↓
          </button>
        </div>
      </div>
    )
  }

  return (
    <form className="menu-admin" onSubmit={submit}>
      {error ? <p className="form-error">{error}</p> : null}

      <div className="menu-admin-grid">
        <aside className="menu-panel">
          <div className="menu-panel-head">
            <div>
              <p>{t('admin.menuList')}</p>
              <strong>{tree.length}</strong>
            </div>
            <button type="button" className="btn-orange menu-mini" onClick={addTop}>
              + {t('admin.addMenu')}
            </button>
          </div>
          <div className="menu-tree">
            {tree.map((item, index) => (
              <div key={item.id} className="menu-group">
                {renderRow(item, index, tree)}
                {(item.children || []).length ? (
                  <div className="menu-children">{item.children.map((child, cIndex) => renderRow(child, cIndex, item.children, item.id))}</div>
                ) : null}
              </div>
            ))}
          </div>
        </aside>

        {selected && page ? (
          <section className="menu-editor">
            <header className="menu-editor-head">
              <div>
                <p className="menu-crumb">
                  {parent ? `${lang === 'bn' && parent.labelBn ? parent.labelBn : parent.label} / ` : ''}
                  {typeName(selected.type, lang)}
                </p>
                <h2>{lang === 'bn' && selected.labelBn ? selected.labelBn : selected.label}</h2>
              </div>
              <div className="menu-editor-head-actions">
                {selected.type !== 'group' ? (
                  <Link className="menu-view" to={menuPath(selected)} target="_blank" rel="noreferrer">
                    {t('admin.viewPage')}
                  </Link>
                ) : null}
                <label className="menu-switch">
                  <input
                    type="checkbox"
                    checked={selected.visible !== false}
                    onChange={(e) => patchItem(selected.id, { visible: e.target.checked })}
                  />
                  <span>{selected.visible !== false ? t('admin.showInMenu') : t('admin.hidden')}</span>
                </label>
              </div>
            </header>

            <div className="menu-card">
              <h3>{t('admin.menuSettings')}</h3>
              <div className="menu-fields">
                <label>
                  {t('admin.menuNameEn')}
                  <input required value={selected.label || ''} onChange={(e) => patchItem(selected.id, { label: e.target.value })} />
                </label>
                <label>
                  {t('admin.menuNameBn')}
                  <input value={selected.labelBn || ''} onChange={(e) => patchItem(selected.id, { labelBn: e.target.value })} />
                </label>
              </div>
              {!parent && selected.type === 'page' ? (
                <button type="button" className="menu-add-page" onClick={() => addChild(selected.id)}>
                  + {t('admin.addSubmenu')}
                </button>
              ) : null}
            </div>

            {isContent ? (
              <div className="menu-card">
                <div className="menu-card-head">
                  <h3>{t('admin.pageContent')}</h3>
                  <div className="menu-lang-tabs" role="tablist">
                    <button type="button" className={langTab === 'en' ? 'is-active' : ''} onClick={() => setLangTab('en')}>
                      English
                    </button>
                    <button type="button" className={langTab === 'bn' ? 'is-active' : ''} onClick={() => setLangTab('bn')}>
                      বাংলা
                    </button>
                  </div>
                </div>

                <label className="menu-photo">
                  {preview ? <img src={preview} alt="" /> : <span>{t('admin.pagePhotoHint')}</span>}
                  <input key={fileKey} type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
                </label>

                {langTab === 'en' ? (
                  <div className="menu-stack">
                    <label>
                      {t('admin.pageTitle')}
                      <input value={page.title || ''} onChange={(e) => patchPage(pageKey, { title: e.target.value })} />
                    </label>
                    <label>
                      {t('admin.pageIntro')}
                      <textarea rows="2" value={page.lead || ''} onChange={(e) => patchPage(pageKey, { lead: e.target.value })} />
                    </label>
                    <label>
                      {t('admin.pageBody')}
                      <textarea rows="10" value={page.body || ''} onChange={(e) => patchPage(pageKey, { body: e.target.value })} />
                    </label>
                  </div>
                ) : (
                  <div className="menu-stack">
                    <label>
                      {t('admin.pageTitleBn')}
                      <input value={page.titleBn || ''} onChange={(e) => patchPage(pageKey, { titleBn: e.target.value })} />
                    </label>
                    <label>
                      {t('admin.pageIntroBn')}
                      <textarea rows="2" value={page.leadBn || ''} onChange={(e) => patchPage(pageKey, { leadBn: e.target.value })} />
                    </label>
                    <label>
                      {t('admin.pageBodyBn')}
                      <textarea rows="10" value={page.bodyBn || ''} onChange={(e) => patchPage(pageKey, { bodyBn: e.target.value })} />
                    </label>
                  </div>
                )}
              </div>
            ) : (
              <div className="menu-hint">
                <p>{t(hintKey)}</p>
                {selected.type === 'group' ? (
                  <button type="button" className="btn-outline" onClick={() => addChild(selected.id)}>
                    + {t('admin.addSubmenu')}
                  </button>
                ) : null}
              </div>
            )}

            {isContent && showMembers ? (
              <div className="menu-card">
                <div className="menu-card-head">
                  <h3>{t('admin.pageMembers')}</h3>
                  <button type="button" className="menu-mini btn-orange" onClick={addMember}>
                    + {t('admin.addMember')}
                  </button>
                </div>
                <p className="menu-member-lead">{t('admin.pageMembersLead')}</p>
                {members.map((member, index) => {
                  const photo = memberFiles[member.id]
                    ? URL.createObjectURL(memberFiles[member.id])
                    : member.image
                  return (
                    <article key={member.id} className="menu-member">
                      <div className="menu-member-tools">
                        <button type="button" aria-label="Up" disabled={index === 0} onClick={() => moveMember(member.id, -1)}>
                          ↑
                        </button>
                        <button type="button" aria-label="Down" disabled={index === members.length - 1} onClick={() => moveMember(member.id, 1)}>
                          ↓
                        </button>
                        <button type="button" className="btn-del" onClick={() => removeMember(member.id)}>
                          {t('admin.deleteMember')}
                        </button>
                      </div>
                      <label className="menu-member-photo">
                        {photo ? <img src={photo} alt="" /> : <span>{t('admin.memberPhoto')}</span>}
                        <input
                          key={`${fileKey}-${member.id}`}
                          type="file"
                          accept="image/*"
                          onChange={(e) => setMemberFiles((current) => ({ ...current, [member.id]: e.target.files?.[0] || null }))}
                        />
                      </label>
                      {langTab === 'en' ? (
                        <div className="menu-stack">
                          <label>
                            {t('admin.memberName')}
                            <input value={member.name || ''} onChange={(e) => patchMember(member.id, { name: e.target.value })} />
                          </label>
                          <label>
                            {t('admin.memberTitle')}
                            <input value={member.title || ''} onChange={(e) => patchMember(member.id, { title: e.target.value })} />
                          </label>
                          <label>
                            {t('admin.memberQualification')}
                            <textarea rows="2" value={member.qualification || ''} onChange={(e) => patchMember(member.id, { qualification: e.target.value })} />
                          </label>
                        </div>
                      ) : (
                        <div className="menu-stack">
                          <label>
                            {t('admin.memberNameBn')}
                            <input value={member.nameBn || ''} onChange={(e) => patchMember(member.id, { nameBn: e.target.value })} />
                          </label>
                          <label>
                            {t('admin.memberTitleBn')}
                            <input value={member.titleBn || ''} onChange={(e) => patchMember(member.id, { titleBn: e.target.value })} />
                          </label>
                          <label>
                            {t('admin.memberQualificationBn')}
                            <textarea rows="2" value={member.qualificationBn || ''} onChange={(e) => patchMember(member.id, { qualificationBn: e.target.value })} />
                          </label>
                        </div>
                      )}
                    </article>
                  )
                })}
              </div>
            ) : null}

            <button type="button" className="menu-advanced-toggle" onClick={() => setAdvanced((v) => !v)}>
              {advanced ? t('admin.hideAdvanced') : t('admin.showAdvanced')}
            </button>
            {advanced ? (
              <div className="menu-card menu-advanced">
                <label>
                  {t('admin.menuType')}
                  <select value={selected.type || 'page'} onChange={(e) => patchItem(selected.id, { type: e.target.value })}>
                    {Object.keys(TYPE_META).map((id) => (
                      <option key={id} value={id}>
                        {typeName(id, lang)}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  {t('admin.menuSlug')}
                  <input value={selected.slug || ''} onChange={(e) => patchItem(selected.id, { slug: e.target.value })} />
                </label>
              </div>
            ) : null}

            <div className="menu-editor-actions">
              <button className="btn-orange" type="submit" disabled={busy}>
                {busy ? t('admin.profileSaving') : saved ? t('admin.profileSaved') : t('admin.saveMenus')}
              </button>
              <button type="button" className="btn-del" onClick={() => removeItem(selected.id)}>
                {t('admin.deleteMenu')}
              </button>
            </div>
          </section>
        ) : (
          <p className="lead">{t('admin.selectMenu')}</p>
        )}
      </div>
    </form>
  )
}
