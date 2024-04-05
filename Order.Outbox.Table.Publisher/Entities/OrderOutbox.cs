namespace Order.Outbox.Table.Publisher.Entities
{
    public class OrderOutbox
    {
        public Guid IdempotentToken { get; set; }
        //Ne zaman bu event gönderildi. İlgili eventin oluşturulduğu tarih.
        public DateTime OccuredOn { get; set; }
        //Eventin işlendiği tarih. İşlenmediyse null olabilir.
        public DateTime? ProcessDate { get; set; }
        //Eventin türü örneğin OrderCreated, OrderUpdated eventleri olabilir.
        public string Type { get; set; }
        //Payload gönderilecek eventin bilgileri olacaktır.
        public string Payload { get; set; }
    }
}
