import { useCallback, useEffect, useState } from 'react'
import { api } from '../api'
import NewReportForm from './NewReportForm'
import ReportDetail from './ReportDetail'
import ReportTable from './ReportTable'

export default function MyReports() {
  const [reports, setReports] = useState([])
  const [selected, setSelected] = useState(null)
  const [error, setError] = useState(null)

  const load = useCallback(() => {
    api.myReports().then(setReports, (err) => setError(err.message))
  }, [])

  useEffect(load, [load])

  if (selected) return <ReportDetail id={selected} isAdmin={false} onBack={() => setSelected(null)} />

  return (
    <>
      <NewReportForm onCreated={load} />
      <section className="card">
        <h2>Mis reportes</h2>
        {error && <p role="alert" className="error">{error}</p>}
        <ReportTable reports={reports} onSelect={setSelected} />
      </section>
    </>
  )
}
