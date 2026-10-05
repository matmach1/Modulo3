import { useEffect, useState } from 'react'
import { api, getSession, onSessionChange } from './api'
import AdminReports from './components/AdminReports'
import AuthPage from './components/AuthPage'
import MyReports from './components/MyReports'
import NewReportForm from './components/NewReportForm'

export default function App() {
  const [session, setSession] = useState(getSession())
  useEffect(() => onSessionChange(setSession), [])

  if (!session) return <AuthPage />

  const isAdmin = session.role === 'Admin'
  return (
    <>
      <header className="topbar">
        <strong>Reporte de Riesgos Laborales</strong>
        <span>
          {session.email} ({isAdmin ? 'Administrador' : 'Empleado'})
          <button type="button" className="link" onClick={api.logout}>Salir</button>
        </span>
      </header>
      <main>
        {isAdmin ? (
          <>
            <NewReportForm />
            <AdminReports />
          </>
        ) : (
          <MyReports />
        )}
      </main>
    </>
  )
}
