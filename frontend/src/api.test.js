import { api, getSession, setSession } from './api'

afterEach(() => {
  setSession(null)
  vi.restoreAllMocks()
})

function respond(status, body, headers = {}) {
  return vi.spyOn(globalThis, 'fetch').mockResolvedValue(
    new Response(body === undefined ? null : JSON.stringify(body), { status, headers }),
  )
}

test('el login guarda la sesión', async () => {
  respond(200, { token: 'abc', email: 'ana@test.com', role: 'Comun' })

  await api.login('ana@test.com', 'secreta')

  expect(getSession()).toEqual({ token: 'abc', email: 'ana@test.com', role: 'Comun' })
})

test('envía el token y lo reemplaza por el renovado', async () => {
  setSession({ token: 'viejo', email: 'ana@test.com', role: 'Comun' })
  const fetchMock = respond(200, [], { 'X-Session-Token': 'nuevo' })

  await api.myReports()

  expect(fetchMock.mock.calls[0][1].headers.Authorization).toBe('Bearer viejo')
  expect(getSession().token).toBe('nuevo')
})

test('un 401 con sesión la cierra', async () => {
  setSession({ token: 'vencido', email: 'ana@test.com', role: 'Comun' })
  respond(401)

  await expect(api.myReports()).rejects.toMatchObject({ status: 401 })
  expect(getSession()).toBeNull()
})

test('propaga el mensaje de error de la API', async () => {
  respond(400, { error: 'La descripción y la ubicación son obligatorias.' })

  await expect(api.createReport({})).rejects.toThrow('La descripción y la ubicación son obligatorias.')
})
