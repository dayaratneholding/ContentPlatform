using Carter;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newsletter.Reporting.Api.Database;
using Newsletter.Reporting.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddMassTransit((busConfigaration) =>
{
    busConfigaration.SetKebabCaseEndpointNameFormatter();
    busConfigaration.AddConsumers(typeof(Program).Assembly);
    busConfigaration.UsingRabbitMq((context, configarator) =>
    {
        configarator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), h =>
        {
            h.Username(builder.Configuration["MessageBroker:Username"]!);
            h.Password(builder.Configuration["MessageBroker:Password"]!);
        });
        configarator.ExchangeType = "FanOut";
        configarator.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.MapCarter();

app.UseHttpsRedirection();

app.Run();