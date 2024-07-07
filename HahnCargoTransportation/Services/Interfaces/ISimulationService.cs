namespace HahnCargoTransportation.Services.Interfaces
{
    public interface ISimulationService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
