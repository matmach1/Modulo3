import { CATEGORY, CRITICALITY, STATUS, TYPE, formatDate, label } from '../labels'

export default function ReportTable({ reports, onSelect }) {
  if (reports.length === 0) return <p>No hay reportes.</p>

  return (
    <table className="reports">
      <thead>
        <tr>
          <th>#</th>
          <th>Fecha</th>
          <th>Descripción</th>
          <th>Tipo</th>
          <th>Categoría</th>
          <th>Criticidad</th>
          <th>Estado</th>
        </tr>
      </thead>
      <tbody>
        {reports.map((report) => (
          <tr key={report.id} onClick={() => onSelect(report.id)} className="clickable">
            <td>{report.id}</td>
            <td>{formatDate(report.createdAt)}</td>
            <td>{report.description}</td>
            <td>{label(TYPE, report.type)}</td>
            <td>{label(CATEGORY, report.category)}</td>
            <td><span className={`badge criticality-${report.criticality}`}>{label(CRITICALITY, report.criticality)}</span></td>
            <td>{label(STATUS, report.status)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
