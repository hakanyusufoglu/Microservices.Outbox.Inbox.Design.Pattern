using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Models.Contexts;
using Order.Api.Models.Entities;
using Order.Api.ViewModels;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);

    });
});

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlServer")));


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVm model, OrderDbContext context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order.Api.Models.Entities.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId,
        }).ToList(),
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = model.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId
        }).ToList()
    };

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEvent}"));
    await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);

});

app.Run();
