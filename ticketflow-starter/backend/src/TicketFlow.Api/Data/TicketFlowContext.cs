using Microsoft.EntityFrameworkCore;

namespace TicketFlow.Api.Data;

public class TicketFlowContext(DbContextOptions<TicketFlowContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
}
