namespace Order.Api.ViewModels
{
    public class CreateOrderItemVm
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
