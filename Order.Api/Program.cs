using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Models.Contexts;
using Order.Api.Models.Entities;
using Order.Api.ViewModels;
using Shared;
using Shared.Events;
using System.Text.Json;

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
    //Dual write to database
    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();
    var idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        IdempotentToken = idempotentToken,
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

    #region order'ý veritabanýna kaydettikten sonra evente gönderilme iþlemi (Outbox Pattern Olmadýðý senaryo)

    // (veri güvenliðinde soruna neden olabilir. Çünkü dual write iþlemi var veri tabanýna kaydettikten sonra message bus ile iletiþim kurulmasý veri iþlenemeyecek. Bu durumu önlemek adýna outbox design pattern kullanýlacak) Bu region yorum satýrýna alýndý çünkü outbox design pattern kullanýcak. Yorum satýrlý kodlarýn býrakýlmasý best practise deðildir ve kod okunabilirliðini azaltýr.



    ////Dual write to RabbitMq
    //var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEvent}"));
    //await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);


    #endregion

    //Outbox Pattern - Veri güvenliði için veriyi fiziksel olarak db'ye kaydettikten sonra eventi gönderme iþlemi gerçekleþtirilecek.
    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = orderCreatedEvent.GetType().Name,
        IdempotentToken = idempotentToken // idempotent token deðeri eklendi
    };

    await context.OrderOutboxes.AddAsync(orderOutbox);
    await context.SaveChangesAsync();

});

app.Run();