namespace HahnCargoTransportation.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int OriginNodeId { get; set; }
        public int TargetNodeId { get; set; }
        public int Load { get; set; }
        public int Value { get; set; }
        public string deliveryDateUtc { get; set; }
        public string expirationDateUtc { get; set; }
    }
}
