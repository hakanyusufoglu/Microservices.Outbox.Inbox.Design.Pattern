namespace Stock.Service.Models.Entities
{
    public class OrderInbox
    {
        public Guid IdempotentToken { get; set; }
        //İşlemler yapıldı mı yapılmadı mı?
        public bool Processed { get; set; }
        public string Payload { get; set; }

    }
}
