// Tous les appels passent par /api : nginx (ou Vite en développement) les relaie vers le backend.
async function handle(response) {
  if (response.status === 204) return null;
  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    const error = new Error(body.error || `Erreur ${response.status}`);
    error.status = response.status;
    throw error;
  }
  return body;
}

function send(method, path, data) {
  return fetch(path, {
    method,
    credentials: 'same-origin',
    headers: data ? { 'Content-Type': 'application/json' } : {},
    body: data ? JSON.stringify(data) : undefined,
  }).then(handle);
}

// Espace public
export const listEvents = () => send('GET', '/api/events');
export const reserve = (eventId, reservation) =>
  send('POST', `/api/events/${eventId}/reservations`, reservation);

// Espace administrateur (session par cookie)
export const login = (password) => send('POST', '/api/login', { password });
export const logout = () => send('POST', '/api/logout');
export const getStats = () => send('GET', '/api/admin/stats');
export const createEvent = (event) => send('POST', '/api/admin/events', event);
export const cancelReservation = (id) => send('DELETE', `/api/admin/reservations/${id}`);
