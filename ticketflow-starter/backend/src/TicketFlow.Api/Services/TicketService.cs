using Microsoft.EntityFrameworkCore;
using TicketFlow.Api.Data;

namespace TicketFlow.Api.Services;

public record EventView(int Id, string Name, DateOnly Date, int Capacity, int PriceCents, int SeatsLeft);

public record ReservationResult(bool Success, string Message);

public record EventStats(int Id, string Name, DateOnly Date, int Capacity, int Sold, int FillRate, int RevenueCents);

public record DailyStat(DateOnly Day, int Seats);

public record RecentReservation(int Id, DateTime CreatedAt, string EventName, string CustomerEmail, int Seats);

public record DashboardStats(
    int EventCount,
    int ReservationCount,
    int SeatsSold,
    int RevenueCents,
    int FillRate,
    List<EventStats> ByEvent,
    List<DailyStat> Daily,
    List<RecentReservation> Recent);

public class TicketService(TicketFlowContext db, IConfiguration config)
{
    public async Task<List<EventView>> ListEventsAsync()
    {
        var events = await db.Events.OrderBy(e => e.Date).ToListAsync();
        var result = new List<EventView>();
        foreach (var ev in events)
        {
            var reserved = 0;
            var reservations = await db.Reservations.Where(r => r.EventId == ev.Id).ToListAsync();
            foreach (var r in reservations)
            {
                reserved = reserved + r.Seats;
            }
            var seatsLeft = ev.Capacity - reserved;
            result.Add(new EventView(ev.Id, ev.Name, ev.Date, ev.Capacity, ev.PriceCents, seatsLeft));
        }
        return result;
    }

    public async Task<ReservationResult> ReserveAsync(int eventId, string email, int seats)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        var maxSeats = config.GetValue("Tickets:MaxSeatsPerReservation", 6);
        if (ev != null)
        {
            if (seats > 0)
            {
                if (seats <= maxSeats)
                {
                    if (email.Contains('@'))
                    {
                        if (email.Split('@')[1].Contains('.'))
                        {
                            var reserved = 0;
                            var reservations = await db.Reservations.Where(r => r.EventId == ev.Id).ToListAsync();
                            foreach (var r in reservations)
                            {
                                reserved = reserved + r.Seats;
                            }
                            var seatsLeft = ev.Capacity - reserved;
                            if (seats <= seatsLeft)
                            {
                                db.Reservations.Add(new Reservation
                                {
                                    EventId = ev.Id,
                                    CustomerEmail = email,
                                    Seats = seats,
                                    CreatedAt = DateTime.UtcNow,
                                });
                                await db.SaveChangesAsync();
                                return new ReservationResult(true, $"{seats} place(s) réservée(s) pour « {ev.Name} ».");
                            }
                            else
                            {
                                return new ReservationResult(false, "Plus assez de places.");
                            }
                        }
                        else
                        {
                            return new ReservationResult(false, "Adresse e-mail invalide.");
                        }
                    }
                    else
                    {
                        return new ReservationResult(false, "Adresse e-mail invalide.");
                    }
                }
                else
                {
                    return new ReservationResult(false, "Trop de places demandées.");
                }
            }
            else
            {
                return new ReservationResult(false, "Nombre de places invalide.");
            }
        }
        else
        {
            return new ReservationResult(false, "Événement introuvable.");
        }
    }

    public async Task<Event> CreateEventAsync(string name, DateOnly date, int capacity, int priceCents)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Le nom est obligatoire.");
        }
        if (capacity <= 0)
        {
            throw new ArgumentException("La capacité doit être un entier positif.");
        }
        if (priceCents < 0)
        {
            throw new ArgumentException("Le prix ne peut pas être négatif.");
        }
        var ev = new Event { Name = name.Trim(), Date = date, Capacity = capacity, PriceCents = priceCents };
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return ev;
    }

    public async Task<bool> CancelAsync(int reservationId)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);
        if (reservation == null)
        {
            return false;
        }
        db.Reservations.Remove(reservation);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardStats> GetStatsAsync(DateOnly today)
    {
        var events = await db.Events.OrderBy(e => e.Date).ToListAsync();
        var reservations = await db.Reservations.OrderBy(r => r.Id).ToListAsync();

        var byEvent = events.Select(ev =>
        {
            var sold = reservations.Where(r => r.EventId == ev.Id).Sum(r => r.Seats);
            var fillRate = (int)Math.Round(sold * 100.0 / ev.Capacity);
            return new EventStats(ev.Id, ev.Name, ev.Date, ev.Capacity, sold, fillRate, sold * ev.PriceCents);
        }).ToList();

        var daily = Enumerable.Range(0, 14)
            .Select(i => today.AddDays(i - 13))
            .Select(day => new DailyStat(
                day,
                reservations.Where(r => DateOnly.FromDateTime(r.CreatedAt) == day).Sum(r => r.Seats)))
            .ToList();

        var names = events.ToDictionary(e => e.Id, e => e.Name);
        var recent = reservations
            .OrderByDescending(r => r.Id)
            .Take(10)
            .Select(r => new RecentReservation(
                r.Id, r.CreatedAt, names.GetValueOrDefault(r.EventId, "?"), r.CustomerEmail, r.Seats))
            .ToList();

        var totalCapacity = byEvent.Sum(e => e.Capacity);
        var totalSold = byEvent.Sum(e => e.Sold);
        var totalFillRate = totalCapacity == 0 ? 0 : (int)Math.Round(totalSold * 100.0 / totalCapacity);

        return new DashboardStats(
            events.Count,
            reservations.Count,
            totalSold,
            byEvent.Sum(e => e.RevenueCents),
            totalFillRate,
            byEvent,
            daily,
            recent);
    }
}
