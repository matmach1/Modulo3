import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { api } from '../api'
import ReportDetail from './ReportDetail'

vi.mock('../api', () => ({ api: { getReport: vi.fn(), changeStatus: vi.fn() } }))

const report = (overrides = {}) => ({
  id: 3,
  description: 'Cable pelado',
  location: 'Planta 1',
  type: 'CondicionInsegura',
  category: 'RiesgoElectrico',
  suggestedCategory: null,
  criticality: 'Alta',
  status: 'Abierto',
  createdAt: '2026-10-05T12:00:00Z',
  createdBy: 'ana@test.com',
  recommendation: 'Requiere intervención inmediata',
  ...overrides,
})

afterEach(() => vi.clearAllMocks())

test('muestra los datos y la recomendación', async () => {
  api.getReport.mockResolvedValue(report())
  render(<ReportDetail id={3} isAdmin={false} onBack={() => {}} />)

  expect(await screen.findByText('Requiere intervención inmediata')).toBeInTheDocument()
  expect(screen.getByText(/ana@test.com/)).toBeInTheDocument()
  expect(screen.queryByRole('button', { name: /en progreso/i })).not.toBeInTheDocument()
})

test('el admin pasa un reporte abierto a en progreso', async () => {
  api.getReport.mockResolvedValue(report())
  api.changeStatus.mockResolvedValue(report({ status: 'EnProgreso' }))
  render(<ReportDetail id={3} isAdmin onBack={() => {}} />)

  await userEvent.click(await screen.findByRole('button', { name: 'Pasar a En progreso' }))

  expect(api.changeStatus).toHaveBeenCalledWith(3, { status: 'EnProgreso' })
  expect(await screen.findByRole('heading', { name: 'Cerrar reporte' })).toBeInTheDocument()
})

test('cerrar exige acción realizada y observación', async () => {
  api.getReport.mockResolvedValue(report({ status: 'EnProgreso' }))
  render(<ReportDetail id={3} isAdmin onBack={() => {}} />)

  await userEvent.type(await screen.findByLabelText('Acción realizada'), 'Se cambió el cable')
  await userEvent.click(screen.getByRole('button', { name: 'Cerrar reporte' }))

  expect(screen.getByRole('alert')).toHaveTextContent('observación de cierre')
  expect(api.changeStatus).not.toHaveBeenCalled()
})

test('el admin cierra un reporte en progreso', async () => {
  api.getReport.mockResolvedValue(report({ status: 'EnProgreso' }))
  api.changeStatus.mockResolvedValue(
    report({ status: 'Cerrado', closedAt: '2026-10-05T13:00:00Z', closedBy: 'admin@test.com', closingAction: 'Se cambió el cable', closingNote: 'OK' }),
  )
  render(<ReportDetail id={3} isAdmin onBack={() => {}} />)

  await userEvent.type(await screen.findByLabelText('Acción realizada'), 'Se cambió el cable')
  await userEvent.type(screen.getByLabelText('Observación de cierre'), 'OK')
  await userEvent.click(screen.getByRole('button', { name: 'Cerrar reporte' }))

  expect(api.changeStatus).toHaveBeenCalledWith(3, { status: 'Cerrado', closingAction: 'Se cambió el cable', closingNote: 'OK' })
  expect(await screen.findByText(/admin@test.com/)).toBeInTheDocument()
})
