using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.Service.Models.Contexts;
using Stock.Service.Models.Entities;
using System.Text.Json;

namespace Stock.Service.Consumers
{
    public class OrderCreatedEventConsumer(StockDbContext stockDbContext) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            //IdempotentToken kontrolü yapıyoruz. Eğer daha önce işlenmiş bir mesaj ise işlem yapmıyoruz.
            var result = await stockDbContext.OrderInboxes.AnyAsync(i => i.IdempotentToken == context.Message.IdempotentToken);

            if (!result)
            {
                //Gelen veriyi inbox pattern gereği stock servis db'sine kaydediyoruz.
                await stockDbContext.OrderInboxes.AddAsync(new()
                {
                    Processed = false,
                    Payload = JsonSerializer.Serialize(context.Message)
                });

                await stockDbContext.SaveChangesAsync();
            }

            //Gelen veriyi işleyip stok proccessed güncellemesi yapılıyor.
            //Todo: Bu işlemi bir servise taşıyarak daha iyi bir yapıya kavuşturabiliriz.
            List<OrderInbox> orderInboxes = await stockDbContext.OrderInboxes.Where(x => x.Processed == false).ToListAsync();

            foreach (var orderInbox in orderInboxes)
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);

                await Console.Out.WriteLineAsync($"{orderCreatedEvent.IdempotentToken} order id değerine karşılık olan siparişin stok işlemleri başarıyla tamamlanmıştır");
                orderInbox.Processed = true;

                await stockDbContext.SaveChangesAsync();
            }
        }
    }
}
