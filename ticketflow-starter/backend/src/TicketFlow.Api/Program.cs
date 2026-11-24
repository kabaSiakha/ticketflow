using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TicketFlow.Api.Data;
using TicketFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TicketFlowContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TicketFlow")));
builder.Services.AddScoped<TicketService>();

// Session administrateur par cookie. Une API répond 401 au lieu de rediriger vers une page de connexion.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ticketflow_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Création du schéma, avec attente de la base (utile au démarrage dans Docker).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketFlowContext>();
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.EnsureCreated();
            break;
        }
        catch (Exception ex) when (attempt < 20)
        {
            app.Logger.LogWarning("Base de données indisponible (tentative {Attempt}/20) : {Message}", attempt, ex.Message);
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (TicketFlowContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ok" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

// ---------- Espace public ----------

var api = app.MapGroup("/api");

api.MapGet("/events", async (TicketService tickets) =>
    Results.Ok(await tickets.ListEventsAsync()));

api.MapGet("/events/{id:int}", async (int id, TicketService tickets) =>
{
    var events = await tickets.ListEventsAsync();
    var ev = events.FirstOrDefault(e => e.Id == id);
    return ev is null
        ? Results.NotFound(new { error = "Événement introuvable." })
        : Results.Ok(ev);
});

api.MapPost("/events/{id:int}/reservations", async (int id, ReservationRequest request, TicketService tickets) =>
{
    var result = await tickets.ReserveAsync(id, request.CustomerEmail ?? "", request.Seats);
    return result.Success
        ? Results.Ok(new { message = result.Message })
        : Results.BadRequest(new { error = result.Message });
});

api.MapPost("/login", async (LoginRequest request, IConfiguration config, HttpContext http) =>
{
    if (request.Password != config["Admin:Password"])
    {
        return Results.Json(new { error = "Mot de passe incorrect." }, statusCode: StatusCodes.Status401Unauthorized);
    }
    var identity = new ClaimsIdentity(
        [new Claim(ClaimTypes.Name, "admin")],
        CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Ok(new { message = "Connecté." });
});

api.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
});

// ---------- Espace administrateur ----------

var admin = api.MapGroup("/admin").RequireAuthorization();

admin.MapGet("/stats", async (TicketService tickets) =>
    Results.Ok(await tickets.GetStatsAsync(DateOnly.FromDateTime(DateTime.UtcNow))));

admin.MapPost("/events", async (CreateEventRequest request, TicketService tickets) =>
{
    try
    {
        var ev = await tickets.CreateEventAsync(request.Name ?? "", request.Date, request.Capacity, request.PriceCents);
        return Results.Created($"/api/events/{ev.Id}", new { id = ev.Id, name = ev.Name });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

admin.MapDelete("/reservations/{id:int}", async (int id, TicketService tickets) =>
    await tickets.CancelAsync(id)
        ? Results.NoContent()
        : Results.NotFound(new { error = "Réservation introuvable." }));

admin.MapGet("/export", async (TicketFlowContext db) =>
{
    var reservations = await db.Reservations.AsNoTracking().OrderBy(r => r.Id).ToListAsync();
    return Results.Text(JsonConvert.SerializeObject(reservations, Formatting.Indented), "application/json");
});

app.Run();

public record ReservationRequest(string? CustomerEmail, int Seats);

public record LoginRequest(string? Password);

public record CreateEventRequest(string? Name, DateOnly Date, int Capacity, int PriceCents);

public partial class Program { }
