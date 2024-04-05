namespace Stock.Service.Models.Entities
{
    public class OrderInbox
    {
        public int Id { get; set; }
        //İşlemler yapıldı mı yapılmadı mı?
        public bool Processed { get; set; }
        public string Payload { get; set; }

    }
}
