import { useEffect, useState } from 'react';
import { cancelReservation, createEvent, getStats, login, logout } from './api.js';
import { euros, frDate, frDateTime, shortDay } from './format.js';

function LoginForm({ onLogin }) {
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);

  async function submit(e) {
    e.preventDefault();
    try {
      await login(password);
      onLogin();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form onSubmit={submit} className="card narrow">
      <h2>Espace administrateur</h2>
      {error && <p className="error">{error}</p>}
      <label>
        Mot de passe
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </label>
      <button type="submit">Se connecter</button>
    </form>
  );
}

function Kpi({ label, value }) {
  return (
    <div className="kpi">
      <span className="kpi-value">{value}</span>
      <span className="kpi-label">{label}</span>
    </div>
  );
}

function DailyChart({ daily }) {
  const max = Math.max(1, ...daily.map((d) => d.seats));
  return (
    <div className="columns">
      {daily.map((d) => (
        <div key={d.day} className="column" title={`${frDate(d.day)} : ${d.seats} place(s)`}>
          <span className="column-value">{d.seats > 0 ? d.seats : ''}</span>
          <div className="column-bar" style={{ height: `${(d.seats / max) * 100}%` }} />
          <span className="column-label">{shortDay(d.day)}</span>
        </div>
      ))}
    </div>
  );
}

function FillChart({ events }) {
  if (events.length === 0) return <p className="muted">Aucun événement.</p>;
  return (
    <div>
      {events.map((event) => (
        <div key={event.id} className="bar-row">
          <span className="bar-name" title={event.name}>
            {event.name}
          </span>
          <div className="bar-track">
            <div className="bar-fill" style={{ width: `${Math.min(event.fillRate, 100)}%` }} />
          </div>
          <span className="bar-value">
            {event.sold}/{event.capacity} · {event.fillRate} %
          </span>
        </div>
      ))}
    </div>
  );
}

function EventForm({ onCreated, onError }) {
  const [name, setName] = useState('');
  const [date, setDate] = useState('');
  const [capacity, setCapacity] = useState(50);
  const [price, setPrice] = useState(20);

  async function submit(e) {
    e.preventDefault();
    try {
      await createEvent({
        name,
        date,
        capacity: Number(capacity),
        priceCents: Math.round(Number(price) * 100),
      });
      setName('');
      setDate('');
      onCreated(`Événement « ${name} » créé.`);
    } catch (err) {
      onError(err);
    }
  }

  return (
    <form onSubmit={submit} className="form-grid">
      <label>
        Nom
        <input value={name} onChange={(e) => setName(e.target.value)} required />
      </label>
      <label>
        Date
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
      </label>
      <label>
        Capacité
        <input
          type="number"
          min="1"
          value={capacity}
          onChange={(e) => setCapacity(e.target.value)}
          required
        />
      </label>
      <label>
        Prix (€)
        <input
          type="number"
          min="0"
          step="0.5"
          value={price}
          onChange={(e) => setPrice(e.target.value)}
          required
        />
      </label>
      <button type="submit">Créer l'événement</button>
    </form>
  );
}

export default function Dashboard() {
  // null = vérification en cours ; false = non connecté
  const [stats, setStats] = useState(null);
  const [loggedIn, setLoggedIn] = useState(null);
  const [message, setMessage] = useState(null);

  async function refresh() {
    try {
      setStats(await getStats());
      setLoggedIn(true);
    } catch (err) {
      if (err.status === 401) {
        setLoggedIn(false);
      } else {
        setMessage({ type: 'error', text: err.message });
      }
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  function handleError(err) {
    if (err.status === 401) setLoggedIn(false);
    setMessage({ type: 'error', text: err.message });
  }

  async function cancel(id) {
    try {
      await cancelReservation(id);
      setMessage({ type: 'success', text: `Réservation n° ${id} annulée.` });
      refresh();
    } catch (err) {
      handleError(err);
    }
  }

  async function signOut() {
    await logout();
    setStats(null);
    setLoggedIn(false);
  }

  if (loggedIn === false) return <LoginForm onLogin={refresh} />;
  if (!stats) return <p className="muted">Chargement…</p>;

  return (
    <section>
      <div className="card-head">
        <h2>Tableau de bord</h2>
        <button className="secondary" onClick={signOut}>
          Se déconnecter
        </button>
      </div>
      {message && <p className={message.type}>{message.text}</p>}

      <div className="kpis">
        <Kpi label="Chiffre d'affaires" value={euros(stats.revenueCents)} />
        <Kpi label="Places vendues" value={stats.seatsSold} />
        <Kpi label="Taux de remplissage" value={`${stats.fillRate} %`} />
        <Kpi label="Réservations" value={stats.reservationCount} />
        <Kpi label="Événements" value={stats.eventCount} />
      </div>

      <div className="card">
        <h3>Places réservées par jour (14 derniers jours)</h3>
        <DailyChart daily={stats.daily} />
      </div>

      <div className="card">
        <h3>Remplissage par événement</h3>
        <FillChart events={stats.byEvent} />
      </div>

      <div className="card">
        <div className="card-head">
          <h3>Dernières réservations</h3>
          <a href="/api/admin/export" target="_blank" rel="noreferrer">
            Exporter (JSON)
          </a>
        </div>
        {stats.recent.length === 0 ? (
          <p className="muted">Aucune réservation.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>N°</th>
                <th>Date</th>
                <th>Événement</th>
                <th>Client</th>
                <th>Places</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {stats.recent.map((r) => (
                <tr key={r.id}>
                  <td>{r.id}</td>
                  <td>{frDateTime(r.createdAt)}</td>
                  <td>{r.eventName}</td>
                  <td>{r.customerEmail}</td>
                  <td>{r.seats}</td>
                  <td>
                    <button className="secondary" onClick={() => cancel(r.id)}>
                      Annuler
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="card">
        <h3>Nouvel événement</h3>
        <EventForm
          onCreated={(text) => {
            setMessage({ type: 'success', text });
            refresh();
          }}
          onError={handleError}
        />
      </div>
    </section>
  );
}
