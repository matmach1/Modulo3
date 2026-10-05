import { act, render, screen } from '@testing-library/react'
import App from './App'
import { setSession } from './api'

afterEach(() => act(() => setSession(null)))

test('sin sesión muestra el inicio de sesión', () => {
  render(<App />)
  expect(screen.getByRole('heading', { name: /iniciar sesión/i })).toBeInTheDocument()
})

test('al cerrar la sesión vuelve al inicio de sesión', () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('[]', { status: 200 }))
  act(() => setSession({ token: 't', email: 'ana@test.com', role: 'Comun' }))
  render(<App />)
  expect(screen.getByText(/ana@test.com/)).toBeInTheDocument()

  act(() => setSession(null))

  expect(screen.getByRole('heading', { name: /iniciar sesión/i })).toBeInTheDocument()
})
