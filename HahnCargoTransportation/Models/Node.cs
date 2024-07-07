namespace HahnCargoTransportation.Models
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class NodesResponse
    {
        public List<Node> Nodes { get; set; }
    }
}
