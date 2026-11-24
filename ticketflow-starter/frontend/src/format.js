export function euros(cents) {
  return (cents / 100).toLocaleString('fr-FR', { style: 'currency', currency: 'EUR' });
}

// "2027-01-15" -> "15/01/2027"
export function frDate(isoDay) {
  const [year, month, day] = isoDay.slice(0, 10).split('-');
  return `${day}/${month}/${year}`;
}

// "2027-01-15" -> "15/01"
export function shortDay(isoDay) {
  const [, month, day] = isoDay.slice(0, 10).split('-');
  return `${day}/${month}`;
}

export function frDateTime(isoDateTime) {
  return new Date(isoDateTime).toLocaleString('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
