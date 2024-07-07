namespace HahnCargoTransportation.Models
{
    public class Edge
    {
        public int Id { get; set; }
        public int Cost { get; set; }
        public TimeSpan Time { get; set; }
    }

    public class EdgesResponse
    {
        public List<Edge> Edges { get; set; }
    }
}
