import { useEffect, useState } from 'react'
import { ACCESS_PAGES } from '../lib/adminAccess.js'
import { api } from '../lib/api.js'
import { useLang } from '../context/LanguageContext.jsx'

const emptyForm = {
  username: '',
  displayName: '',
  designation: '',
  password: '',
  active: true,
  permissions: ['admissions'],
}

export default function AdminUsers() {
  const { t } = useLang()
  const [users, setUsers] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(0)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)

  async function load() {
    try {
      const data = await api.users()
      setUsers(data.users || [])
    } catch (err) {
      setError(err.message || t('admin.usersLoadError'))
    }
  }

  useEffect(() => {
    load()
  }, [])

  function togglePerm(id) {
    setForm((prev) => {
      const has = prev.permissions.includes(id)
      return {
        ...prev,
        permissions: has ? prev.permissions.filter((item) => item !== id) : [...prev.permissions, id],
      }
    })
  }

  function startEdit(user) {
    setEditingId(user.id)
    setForm({
      username: user.username,
      displayName: user.displayName || '',
      designation: user.designation || '',
      password: '',
      active: user.active !== false,
      permissions: user.permissions || [],
    })
    setNotice('')
    setError('')
  }

  function resetForm() {
    setEditingId(0)
    setForm(emptyForm)
  }

  async function submit(e) {
    e.preventDefault()
    setBusy(true)
    setError('')
    setNotice('')
    try {
      if (editingId) {
        await api.updateUser(editingId, {
          displayName: form.displayName,
          designation: form.designation,
          permissions: form.permissions,
          active: form.active,
          password: form.password,
        })
        setNotice(t('admin.usersSaved'))
      } else {
        await api.createUser({
          username: form.username,
          displayName: form.displayName,
          designation: form.designation,
          password: form.password,
          permissions: form.permissions,
          active: form.active,
        })
        setNotice(t('admin.usersCreated'))
      }
      resetForm()
      await load()
    } catch (err) {
      setError(err.message || t('admin.usersSaveError'))
    } finally {
      setBusy(false)
    }
  }

  async function remove(user) {
    if (!window.confirm(t('admin.usersDeleteConfirm', { name: user.username }))) return
    try {
      await api.deleteUser(user.id)
      if (editingId === user.id) resetForm()
      await load()
    } catch (err) {
      setError(err.message || t('admin.usersSaveError'))
    }
  }

  return (
    <div className="container admin-users">
      <p className="lead">{t('admin.usersLead')}</p>
      {error ? <p className="form-error">{error}</p> : null}
      {notice ? <p className="admission-ok">{notice}</p> : null}

      <div className="admin-users-grid">
        <form className="quote-form admin-form" onSubmit={submit}>
          <h3>{editingId ? t('admin.usersEdit') : t('admin.usersAdd')}</h3>
          <label className="full">
            {t('admin.usersUsername')}
            <span>
              <input
                required={!editingId}
                disabled={Boolean(editingId)}
                value={form.username}
                onChange={(e) => setForm({ ...form, username: e.target.value })}
                autoComplete="off"
              />
            </span>
          </label>
          <label className="full">
            {t('admin.usersName')}
            <span>
              <input value={form.displayName} onChange={(e) => setForm({ ...form, displayName: e.target.value })} />
            </span>
          </label>
          <label className="full">
            {t('admin.designation')}
            <span>
              <input value={form.designation} onChange={(e) => setForm({ ...form, designation: e.target.value })} />
            </span>
          </label>
          <label className="full">
            {editingId ? t('admin.usersNewPassword') : t('login.pass')}
            <span>
              <input
                type="password"
                required={!editingId}
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                autoComplete="new-password"
              />
            </span>
          </label>
          <label className="admin-user-active">
            <input type="checkbox" checked={form.active} onChange={(e) => setForm({ ...form, active: e.target.checked })} />
            {t('admin.usersActive')}
          </label>
          <fieldset className="admin-user-perms">
            <legend>{t('admin.usersAccess')}</legend>
            {ACCESS_PAGES.filter((item) => item.id !== 'profile').map((item) => (
              <label key={item.id}>
                <input
                  type="checkbox"
                  checked={form.permissions.includes(item.id)}
                  onChange={() => togglePerm(item.id)}
                />
                {t(item.labelKey)}
              </label>
            ))}
          </fieldset>
          <div className="admin-user-actions">
            <button className="btn-orange" type="submit" disabled={busy}>
              {busy ? t('admin.profileSaving') : editingId ? t('admin.usersSave') : t('admin.usersCreate')}
            </button>
            {editingId ? (
              <button type="button" className="btn-ghost" onClick={resetForm}>
                {t('admin.usersCancel')}
              </button>
            ) : null}
          </div>
        </form>

        <div className="adm-admin-table-wrap">
          <table className="adm-admin-table">
            <thead>
              <tr>
                <th>{t('admin.usersName')}</th>
                <th>{t('admin.designation')}</th>
                <th>{t('admin.usersUsername')}</th>
                <th>{t('admin.usersRole')}</th>
                <th>{t('admin.usersAccess')}</th>
                <th>{t('admin.colStatus')}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.displayName}</td>
                  <td>{user.designation || '—'}</td>
                  <td>{user.username}</td>
                  <td>{user.role === 'admin' ? t('admin.administrator') : t('admin.subAdmin')}</td>
                  <td>
                    {user.role === 'admin' ? (
                      t('admin.usersAllAccess')
                    ) : (
                      <div className="admin-user-chips">
                        {(user.permissions || []).map((id) => {
                          const page = ACCESS_PAGES.find((item) => item.id === id)
                          return <span key={id}>{page ? t(page.labelKey) : id}</span>
                        })}
                      </div>
                    )}
                  </td>
                  <td>{user.active === false ? t('admin.usersDisabled') : t('admin.usersActive')}</td>
                  <td>
                    {user.role === 'admin' ? (
                      '—'
                    ) : (
                      <>
                        <button type="button" className="btn-ghost" onClick={() => startEdit(user)}>
                          {t('admission.edit')}
                        </button>
                        <button type="button" className="btn-del" onClick={() => remove(user)}>
                          {t('admin.deleteMenu')}
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
