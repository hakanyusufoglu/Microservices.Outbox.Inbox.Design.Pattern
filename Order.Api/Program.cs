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

    #region order'� veritaban�na kaydettikten sonra evente g�nderilme i�lemi (Outbox Pattern Olmad��� senaryo)

    // (veri g�venli�inde soruna neden olabilir. ��nk� dual write i�lemi var veri taban�na kaydettikten sonra message bus ile ileti�im kurulmas� veri i�lenemeyecek. Bu durumu �nlemek ad�na outbox design pattern kullan�lacak) Bu region yorum sat�r�na al�nd� ��nk� outbox design pattern kullan�cak. Yorum sat�rl� kodlar�n b�rak�lmas� best practise de�ildir ve kod okunabilirli�ini azalt�r.



    ////Dual write to RabbitMq
    //var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEvent}"));
    //await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);


    #endregion

    //Outbox Pattern - Veri g�venli�i i�in veriyi fiziksel olarak db'ye kaydettikten sonra eventi g�nderme i�lemi ger�ekle�tirilecek.
    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = orderCreatedEvent.GetType().Name,
        IdempotentToken = idempotentToken // idempotent token de�eri eklendi
    };

    await context.OrderOutboxes.AddAsync(orderOutbox);
    await context.SaveChangesAsync();

});

app.Run();