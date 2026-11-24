import { useState } from 'react';
import Dashboard from './Dashboard.jsx';
import Shop from './Shop.jsx';

export default function App() {
  const [view, setView] = useState('shop');

  return (
    <>
      <header>
        <span className="brand">TicketFlow</span>
        <nav>
          <button className={view === 'shop' ? 'tab active' : 'tab'} onClick={() => setView('shop')}>
            Billetterie
          </button>
          <button
            className={view === 'admin' ? 'tab active' : 'tab'}
            onClick={() => setView('admin')}
          >
            Tableau de bord
          </button>
        </nav>
      </header>
      <main>{view === 'shop' ? <Shop /> : <Dashboard />}</main>
    </>
  );
}
