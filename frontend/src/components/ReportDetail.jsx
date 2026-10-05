import { useEffect, useState } from 'react'
import { api } from '../api'
import { CATEGORY, CRITICALITY, STATUS, TYPE, formatDate, label } from '../labels'

export default function ReportDetail({ id, isAdmin, onBack, onChanged }) {
  const [report, setReport] = useState(null)
  const [error, setError] = useState(null)
  const [closingAction, setClosingAction] = useState('')
  const [closingNote, setClosingNote] = useState('')

  useEffect(() => {
    api.getReport(id).then(setReport, (err) => setError(err.message))
  }, [id])

  async function changeStatus(change) {
    setError(null)
    try {
      const updated = await api.changeStatus(id, change)
      setReport(updated)
      onChanged?.(updated)
    } catch (err) {
      setError(err.message)
    }
  }

  function handleClose(event) {
    event.preventDefault()
    if (!closingAction.trim() || !closingNote.trim()) {
      setError('Para cerrar hay que indicar la acción realizada y la observación de cierre.')
      return
    }
    changeStatus({ status: 'Cerrado', closingAction, closingNote })
  }

  return (
    <section className="card">
      <button type="button" className="link" onClick={onBack}>← Volver</button>
      {error && <p role="alert" className="error">{error}</p>}
      {report && (
        <>
          <h2>Reporte #{report.id}</h2>
          {report.recommendation && <p className="recommendation">{report.recommendation}</p>}
          <dl>
            <dt>Estado</dt><dd>{label(STATUS, report.status)}</dd>
            <dt>Descripción</dt><dd>{report.description}</dd>
            <dt>Ubicación</dt><dd>{report.location}</dd>
            <dt>Tipo</dt><dd>{label(TYPE, report.type)}</dd>
            <dt>Categoría</dt><dd>{label(CATEGORY, report.category)}</dd>
            {report.suggestedCategory && report.suggestedCategory !== report.category && (
              <><dt>Categoría sugerida</dt><dd>{label(CATEGORY, report.suggestedCategory)}</dd></>
            )}
            <dt>Criticidad</dt><dd>{label(CRITICALITY, report.criticality)}</dd>
            <dt>Creado</dt><dd>{formatDate(report.createdAt)} por {report.createdBy}</dd>
            {report.closedAt && (
              <>
                <dt>Cerrado</dt><dd>{formatDate(report.closedAt)} por {report.closedBy}</dd>
                <dt>Acción realizada</dt><dd>{report.closingAction}</dd>
                <dt>Observación de cierre</dt><dd>{report.closingNote}</dd>
              </>
            )}
          </dl>

          {isAdmin && report.status === 'Abierto' && (
            <button type="button" onClick={() => changeStatus({ status: 'EnProgreso' })}>Pasar a En progreso</button>
          )}
          {isAdmin && report.status === 'EnProgreso' && (
            <form onSubmit={handleClose}>
              <h3>Cerrar reporte</h3>
              <label>
                Acción realizada
                <textarea value={closingAction} onChange={(e) => setClosingAction(e.target.value)} rows={2} />
              </label>
              <label>
                Observación de cierre
                <textarea value={closingNote} onChange={(e) => setClosingNote(e.target.value)} rows={2} />
              </label>
              <button type="submit">Cerrar reporte</button>
            </form>
          )}
        </>
      )}
    </section>
  )
}
