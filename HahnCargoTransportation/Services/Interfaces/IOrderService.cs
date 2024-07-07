using HahnCargoTransportation.Models;

namespace HahnCargoTransportation.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAvailableOrdersAsync();
        Task AcceptOrderAsync(int orderId);
        Task<List<Order>> GetAllAcceptedOrdersAsync();
    }
}
