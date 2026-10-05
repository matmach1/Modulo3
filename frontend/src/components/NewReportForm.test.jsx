import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { api } from '../api'
import NewReportForm from './NewReportForm'

vi.mock('../api', () => ({ api: { createReport: vi.fn() } }))

afterEach(() => vi.clearAllMocks())

test('sin descripción o ubicación no envía el reporte', async () => {
  render(<NewReportForm />)

  await userEvent.type(screen.getByLabelText('Descripción'), 'Cable pelado')
  await userEvent.click(screen.getByRole('button', { name: 'Enviar reporte' }))

  expect(screen.getByRole('alert')).toHaveTextContent('obligatorias')
  expect(api.createReport).not.toHaveBeenCalled()
})

test('envía el reporte y muestra la clasificación y la recomendación', async () => {
  api.createReport.mockResolvedValue({
    id: 7,
    type: 'CondicionInsegura',
    category: 'RiesgoElectrico',
    criticality: 'Critica',
    recommendation: 'Detener actividad',
  })
  const onCreated = vi.fn()
  render(<NewReportForm onCreated={onCreated} />)

  await userEvent.type(screen.getByLabelText('Descripción'), 'Cable pelado')
  await userEvent.type(screen.getByLabelText('Ubicación'), 'Planta 1')
  await userEvent.selectOptions(screen.getByLabelText(/categoría sugerida/i), 'RiesgoElectrico')
  await userEvent.click(screen.getByRole('button', { name: 'Enviar reporte' }))

  expect(api.createReport).toHaveBeenCalledWith({
    description: 'Cable pelado',
    location: 'Planta 1',
    suggestedCategory: 'RiesgoElectrico',
  })
  const status = await screen.findByRole('status')
  expect(status).toHaveTextContent('Condición insegura')
  expect(status).toHaveTextContent('Riesgo eléctrico')
  expect(status).toHaveTextContent('Crítica')
  expect(status).toHaveTextContent('Detener actividad')
  expect(onCreated).toHaveBeenCalled()
})
