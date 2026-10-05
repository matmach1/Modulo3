import { useState } from 'react'
import { api } from '../api'
import { CATEGORY, CRITICALITY, TYPE, label } from '../labels'

const SUGGESTABLE = Object.keys(CATEGORY).filter((key) => key !== 'Pendiente')

export default function NewReportForm({ onCreated }) {
  const [description, setDescription] = useState('')
  const [location, setLocation] = useState('')
  const [suggestedCategory, setSuggestedCategory] = useState('')
  const [error, setError] = useState(null)
  const [created, setCreated] = useState(null)
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    if (!description.trim() || !location.trim()) {
      setError('La descripción y la ubicación son obligatorias.')
      return
    }
    setError(null)
    setBusy(true)
    try {
      const report = await api.createReport({
        description,
        location,
        suggestedCategory: suggestedCategory || null,
      })
      setCreated(report)
      setDescription('')
      setLocation('')
      setSuggestedCategory('')
      onCreated?.(report)
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="card">
      <h2>Nuevo reporte</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Descripción
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={4} />
        </label>
        <label>
          Ubicación
          <input value={location} onChange={(e) => setLocation(e.target.value)} />
        </label>
        <label>
          Categoría sugerida (opcional)
          <select value={suggestedCategory} onChange={(e) => setSuggestedCategory(e.target.value)}>
            <option value="">Sin sugerencia</option>
            {SUGGESTABLE.map((key) => (
              <option key={key} value={key}>{CATEGORY[key]}</option>
            ))}
          </select>
        </label>
        {error && <p role="alert" className="error">{error}</p>}
        <button type="submit" disabled={busy}>{busy ? 'Enviando y clasificando…' : 'Enviar reporte'}</button>
      </form>
      {created && (
        <div role="status" className="success">
          <p>Reporte #{created.id} enviado.</p>
          <p>
            Tipo: {label(TYPE, created.type)} · Categoría: {label(CATEGORY, created.category)} ·
            Criticidad: {label(CRITICALITY, created.criticality)}
          </p>
          {created.recommendation && <p className="recommendation">{created.recommendation}</p>}
        </div>
      )}
    </section>
  )
}
