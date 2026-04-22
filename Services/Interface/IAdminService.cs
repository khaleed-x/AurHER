using AurHER.DTOs.Admin;

namespace AurHER.Services.Interfaces
{
    public interface IAdminService
    {
        Task<LoginResultDto> LoginAsync(LoginDto login);
        Task<AdminDashboardDto> GetDashboardStatsAsync();

    }
}