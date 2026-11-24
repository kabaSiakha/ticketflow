using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicketFlow.Api.Data;
using TicketFlow.Api.Services;

namespace TicketFlow.Api.Tests;

// Les tests utilisent une base en mémoire : PostgreSQL n'est pas nécessaire.
public class TicketServiceTests
{
    private static TicketService CreateService()
    {
        var options = new DbContextOptionsBuilder<TicketFlowContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tickets:MaxSeatsPerReservation"] = "6",
            })
            .Build();
        return new TicketService(new TicketFlowContext(options), config);
    }

    [Fact]
    public async Task Created_event_is_listed_with_all_seats_left()
    {
        var service = CreateService();

        await service.CreateEventAsync("Concert de rentrée", new DateOnly(2027, 1, 15), 50, 2500);
        var events = await service.ListEventsAsync();

        var ev = Assert.Single(events);
        Assert.Equal("Concert de rentrée", ev.Name);
        Assert.Equal(50, ev.SeatsLeft);
    }

    [Fact]
    public async Task Event_with_zero_capacity_is_rejected()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateEventAsync("Salle vide", new DateOnly(2027, 1, 15), 0, 0));
    }

    [Fact]
    public async Task Stats_are_empty_without_events()
    {
        var service = CreateService();

        var stats = await service.GetStatsAsync(new DateOnly(2027, 1, 15));

        Assert.Equal(0, stats.EventCount);
        Assert.Equal(0, stats.FillRate);
        Assert.Equal(14, stats.Daily.Count);
    }
}
