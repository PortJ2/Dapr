using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PubSub1;
using PubSub1.DAL;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add database connection
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PubSub1", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PubSub1"));

app.UseCloudEvents();
app.MapSubscribeHandler();
app.UseHttpsRedirection();

app.MapPost("/weatherforecast", [Topic("pubsub", "weatherstatus")] async (WeatherForecast[] forecast, [FromServices] DaprClient daprClient, ApplicationDbContext _context) =>
{
    foreach (var status in forecast)
    {
        WeatherForecast newForecast = new()
        {
            Date = status.Date,
            Summary = status.Summary,
            TemperatureC = status.TemperatureC
        };
        _context.WeatherForecasts.Add(newForecast);
    }


    await _context.SaveChangesAsync();
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast");

app.MapGet("/requestforecast", async () =>
{
    using var client = new DaprClientBuilder().Build();
    await client.PublishEventAsync("pubsub", "weatherreport", "requestForecast");
    return Results.Ok("Request Submitted");
}).WithName("RequestForecast");

app.Run();