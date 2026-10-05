import { useCallback, useEffect, useState } from 'react'
import { api } from '../api'
import ReportDetail from './ReportDetail'
import ReportTable from './ReportTable'

const PAGE_SIZE = 20

export default function AdminReports() {
  const [page, setPage] = useState(1)
  const [result, setResult] = useState({ items: [], total: 0 })
  const [selected, setSelected] = useState(null)
  const [error, setError] = useState(null)

  const load = useCallback(() => {
    api.allReports(page, PAGE_SIZE).then(setResult, (err) => setError(err.message))
  }, [page])

  useEffect(load, [load])

  if (selected) {
    return <ReportDetail id={selected} isAdmin onBack={() => { setSelected(null); load() }} />
  }

  const pages = Math.max(1, Math.ceil(result.total / PAGE_SIZE))
  return (
    <section className="card">
      <h2>Todos los reportes</h2>
      {error && <p role="alert" className="error">{error}</p>}
      <ReportTable reports={result.items} onSelect={setSelected} />
      <nav className="pager">
        <button type="button" disabled={page <= 1} onClick={() => setPage(page - 1)}>Anterior</button>
        <span>Página {page} de {pages}</span>
        <button type="button" disabled={page >= pages} onClick={() => setPage(page + 1)}>Siguiente</button>
      </nav>
    </section>
  )
}
