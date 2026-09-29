import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Login() {
  const { authed, login, dbReady } = useApp()
  const { t } = useLang()
  const navigate = useNavigate()
  const [user, setUser] = useState('')
  const [pass, setPass] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  if (authed) return <Navigate to="/admin" replace />

  return (
    <section className="page-pad">
      <form
        className="quote-form login-form"
        onSubmit={async (e) => {
          e.preventDefault()
          setBusy(true)
          setError('')
          try {
            await login(user, pass)
            navigate('/admin')
          } catch (err) {
            setError(err.message || t('login.offline'))
          } finally {
            setBusy(false)
          }
        }}
      >
        <p className="eyebrow">{t('login.eyebrow')}</p>
        <h1 className="page-title">{t('login.title')}</h1>
        <p className="lead">{dbReady ? t('login.ready') : t('login.offline')}</p>
        <label className="full">
          {t('login.user')}
          <span>
            <input value={user} onChange={(e) => setUser(e.target.value)} required />
          </span>
        </label>
        <label className="full">
          {t('login.pass')}
          <span>
            <input type="password" value={pass} onChange={(e) => setPass(e.target.value)} required />
          </span>
        </label>
        {error && <p className="form-error">{error}</p>}
        <button className="btn-orange full" type="submit" disabled={busy}>
          {busy ? t('login.wait') : t('login.submit')}
        </button>
      </form>
    </section>
  )
}
