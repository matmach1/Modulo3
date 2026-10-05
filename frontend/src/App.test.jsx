import { render, screen } from '@testing-library/react'
import App from './App'

test('muestra el título de la aplicación', () => {
  render(<App />)
  expect(screen.getByRole('heading', { name: /reporte de riesgos laborales/i })).toBeInTheDocument()
})
