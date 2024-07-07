using HahnCargoTransportation.Models;

namespace HahnCargoTransportation.Services.Interfaces
{
    public interface ITransporterService
    {
        Task<List<CargoTransporter>> GetTransportersAsync();
        Task BuyTransporterAsync(int nodeId);
        Task MoveTransporterAsync(int transporterId, int toNodeId);
    }
}
