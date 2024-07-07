using HahnCargoTransportation.Models;

namespace HahnCargoTransportation.Services.Interfaces
{
    public interface IGridService
    {
        Task<GridData> GetGridDataAsync();
    }
}
