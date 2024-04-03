using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
        public Guid IdempotentToken { get; set; }
    }
}
