namespace TicketFlow.Api.Data;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly Date { get; set; }
    public int Capacity { get; set; }
    public int PriceCents { get; set; }
    public List<Reservation> Reservations { get; set; } = [];
}

public class Reservation
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string CustomerEmail { get; set; } = "";
    public int Seats { get; set; }
    public DateTime CreatedAt { get; set; }
}
