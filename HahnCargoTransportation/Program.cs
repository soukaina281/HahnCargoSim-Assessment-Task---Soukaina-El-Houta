using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services;
using HahnCargoTransportation.Services.Interfaces;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services
builder.Services.AddHttpClient("HahnApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7115"); 
});
builder.Services.AddSingleton<IGridService, GridService>();
builder.Services.AddSingleton<ITransporterService, TransporterService>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddSingleton<ConcurrentQueue<Order>>();
builder.Services.AddHostedService<RabbitMqService>();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
