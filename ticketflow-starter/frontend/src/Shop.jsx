import { useEffect, useState } from 'react';
import { listEvents, reserve } from './api.js';
import { euros, frDate } from './format.js';

function EventCard({ event, onReserved, onError }) {
  const [email, setEmail] = useState('');
  const [seats, setSeats] = useState(1);

  async function submit(e) {
    e.preventDefault();
    try {
      const result = await reserve(event.id, { customerEmail: email, seats: Number(seats) });
      setEmail('');
      onReserved(result.message);
    } catch (err) {
      onError(err.message);
    }
  }

  const soldOut = event.seatsLeft <= 0;

  return (
    <div className="card">
      <div className="card-head">
        <h3>{event.name}</h3>
        <span className="price">{event.priceCents === 0 ? 'Gratuit' : euros(event.priceCents)}</span>
      </div>
      <p className="muted">
        {frDate(event.date)} · {soldOut ? 'Complet' : `${event.seatsLeft} place(s) restante(s)`}
      </p>
      <form onSubmit={submit} className="inline">
        <input
          type="email"
          placeholder="Votre e-mail"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input
          type="number"
          min="1"
          max="6"
          value={seats}
          onChange={(e) => setSeats(e.target.value)}
          required
        />
        <button type="submit" disabled={soldOut}>
          Réserver
        </button>
      </form>
    </div>
  );
}

export default function Shop() {
  const [events, setEvents] = useState([]);
  const [message, setMessage] = useState(null);

  async function refresh() {
    try {
      setEvents(await listEvents());
    } catch (err) {
      setMessage({ type: 'error', text: `API injoignable : ${err.message}` });
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  return (
    <section>
      <h2>Événements à venir</h2>
      {message && <p className={message.type}>{message.text}</p>}
      {events.length === 0 && <p className="muted">Aucun événement pour le moment.</p>}
      {events.map((event) => (
        <EventCard
          key={event.id}
          event={event}
          onReserved={(text) => {
            setMessage({ type: 'success', text });
            refresh();
          }}
          onError={(text) => setMessage({ type: 'error', text })}
        />
      ))}
    </section>
  );
}
