// Cliente de la API. Guarda la sesión y la renueva con el token que devuelve
// cada respuesta autenticada (X-Session-Token, RNF-06).
const STORAGE_KEY = 'riesgos.session'

function loadSession() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY))
  } catch {
    return null
  }
}

let session = loadSession()
const listeners = new Set()

export function getSession() {
  return session
}

export function setSession(next) {
  session = next
  try {
    if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Sin almacenamiento disponible la sesión vive solo en memoria.
  }
  listeners.forEach((listener) => listener(next))
}

export function onSessionChange(listener) {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

export class ApiError extends Error {
  constructor(status, message) {
    super(message)
    this.status = status
  }
}

const DEFAULT_MESSAGES = {
  401: 'Tu sesión expiró. Volvé a iniciar sesión.',
  403: 'No tenés permiso para hacer esto.',
  404: 'No se encontró lo que buscabas.',
}

async function request(path, { method = 'GET', body } = {}) {
  const headers = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (session) headers.Authorization = `Bearer ${session.token}`

  const response = await fetch(`/api${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const refreshed = response.headers.get('X-Session-Token')
  if (refreshed && session) setSession({ ...session, token: refreshed })
  if (response.status === 401 && session) setSession(null)

  const data = await response.json().catch(() => null)
  if (!response.ok) {
    throw new ApiError(response.status, data?.error ?? DEFAULT_MESSAGES[response.status] ?? 'Ocurrió un error inesperado.')
  }
  return data
}

export const api = {
  register: (email, password) => request('/auth/register', { method: 'POST', body: { email, password } }),
  async login(email, password) {
    const data = await request('/auth/login', { method: 'POST', body: { email, password } })
    setSession({ token: data.token, email: data.email, role: data.role })
    return data
  },
  logout: () => setSession(null),
  createReport: (report) => request('/reports', { method: 'POST', body: report }),
  myReports: () => request('/reports/mine'),
  allReports: (page, size) => request(`/reports?page=${page}&size=${size}`),
  getReport: (id) => request(`/reports/${id}`),
  changeStatus: (id, change) => request(`/reports/${id}/status`, { method: 'PATCH', body: change }),
}
