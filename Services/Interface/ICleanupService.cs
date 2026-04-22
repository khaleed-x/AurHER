namespace AurHER.Services.Interfaces
{
    public interface ICleanupService
    {
        Task<int> CleanupStaleOrdersAsync(int timeoutMinutes = 20);
    }
}