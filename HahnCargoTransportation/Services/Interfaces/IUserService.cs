using HahnCargoTransportation.Models;

namespace HahnCargoTransportation.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(string username, string password = "Hahn");
        Task<int> GetCoinAmountAsync();
        string GetJwtToken();
    }
}
