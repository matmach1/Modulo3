import { useState } from 'react'
import { api } from '../api'

export default function AuthPage() {
  const [mode, setMode] = useState('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)

  const isLogin = mode === 'login'

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setBusy(true)
    try {
      if (!isLogin) await api.register(email, password)
      await api.login(email, password)
    } catch (err) {
      setError(err.status === 401 ? 'Email o contraseña incorrectos.' : err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="auth">
      <h1>Reporte de Riesgos Laborales</h1>
      <form onSubmit={handleSubmit} className="card">
        <h2>{isLogin ? 'Iniciar sesión' : 'Crear cuenta'}</h2>
        <label>
          Email
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Contraseña
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        {error && <p role="alert" className="error">{error}</p>}
        <button type="submit" disabled={busy}>{isLogin ? 'Ingresar' : 'Registrarme'}</button>
        <button type="button" className="link" onClick={() => setMode(isLogin ? 'register' : 'login')}>
          {isLogin ? '¿No tenés cuenta? Registrate' : '¿Ya tenés cuenta? Iniciá sesión'}
        </button>
      </form>
    </main>
  )
}
