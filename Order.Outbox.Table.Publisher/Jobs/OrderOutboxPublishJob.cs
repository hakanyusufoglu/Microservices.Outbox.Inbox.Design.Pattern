using MassTransit;
using Order.Outbox.Table.Publisher.Entities;
using Quartz;
using Shared.Events;
using System.Text.Json;

namespace Order.Outbox.Table.Publisher.Jobs
{
    public class OrderOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingletonDatabase.DataReaderState)
            {
                OrderOutboxSingletonDatabase.DataReaderBusy();
                List<OrderOutbox> orderOutboxes = (await OrderOutboxSingletonDatabase.QueryAsync<OrderOutbox>($@"SELECT * FROM OrderOutboxes WHERE ProcessDate IS NULL ORDER BY OccuredOn ASC")).ToList();

                foreach (var orderOutbox in orderOutboxes)
                {
                    if (orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);
                        if (orderCreatedEvent != null)
                        {
                            await publishEndpoint.Publish(orderCreatedEvent);
                            OrderOutboxSingletonDatabase.ExecuteAsync($"UPDATE OrderOutboxes SET ProcessDate = GETDATE() WHERE Id = {orderOutbox.Id}");
                        }
                    }
                }
                OrderOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order outbox table checked !");
            }
        }
    }
}
