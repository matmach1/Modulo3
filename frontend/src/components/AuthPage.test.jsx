import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { api, ApiError } from '../api'
import AuthPage from './AuthPage'

vi.mock('../api', async (importOriginal) => {
  const original = await importOriginal()
  return { ...original, api: { login: vi.fn(), register: vi.fn() } }
})

afterEach(() => vi.clearAllMocks())

test('inicia sesión con el email y la contraseña ingresados', async () => {
  render(<AuthPage />)

  await userEvent.type(screen.getByLabelText('Email'), 'ana@test.com')
  await userEvent.type(screen.getByLabelText('Contraseña'), 'secreta')
  await userEvent.click(screen.getByRole('button', { name: 'Ingresar' }))

  expect(api.login).toHaveBeenCalledWith('ana@test.com', 'secreta')
  expect(api.register).not.toHaveBeenCalled()
})

test('registrarse crea la cuenta y luego inicia sesión', async () => {
  render(<AuthPage />)

  await userEvent.click(screen.getByRole('button', { name: /registrate/i }))
  await userEvent.type(screen.getByLabelText('Email'), 'ana@test.com')
  await userEvent.type(screen.getByLabelText('Contraseña'), 'secreta')
  await userEvent.click(screen.getByRole('button', { name: 'Registrarme' }))

  expect(api.register).toHaveBeenCalledWith('ana@test.com', 'secreta')
  expect(api.login).toHaveBeenCalledWith('ana@test.com', 'secreta')
})

test('credenciales inválidas muestran un error', async () => {
  api.login.mockRejectedValue(new ApiError(401, 'x'))
  render(<AuthPage />)

  await userEvent.type(screen.getByLabelText('Email'), 'ana@test.com')
  await userEvent.type(screen.getByLabelText('Contraseña'), 'mala')
  await userEvent.click(screen.getByRole('button', { name: 'Ingresar' }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Email o contraseña incorrectos.')
})
