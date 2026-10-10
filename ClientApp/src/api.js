async function request(path, options) {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })
  const text = await res.text()
  let body = text
  try { body = text ? JSON.parse(text) : null } catch { /* plain-text error */ }
  if (!res.ok) throw new Error(typeof body === 'string' ? body : `Request failed (${res.status})`)
  return body
}

const post = (path, body) => request(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined })

export const listBuildings = () => request('/api/buildings')
export const getBuilding = (id) => request(`/api/buildings/${id}`)
export const createBuilding = (name, address) => post('/api/buildings', { name, address })
export const addApartment = (buildingId, name) => post(`/api/buildings/${buildingId}/apartments`, { name })
export const addRoom = (apartmentId, name, monthlyPrice) => post(`/api/apartments/${apartmentId}/rooms`, { name, monthlyPrice })
export const rentRoom = (roomId, tenant) => post(`/api/rooms/${roomId}/rent`, { tenant })
export const releaseRoom = (roomId) => post(`/api/rooms/${roomId}/release`)
