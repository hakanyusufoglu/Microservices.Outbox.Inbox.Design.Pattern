using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.Service.Consumers;
using Stock.Service.Models.Contexts;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StockDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlServer")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);

        _configure.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEvent, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

var host = builder.Build();
host.Run();
