using BonusesApp.Core.Models.Bonuses;

namespace BonusesApp.Core.Services.Bonuses.Interfaces;

public interface IBonusService
{
    Task<List<BonusesEntity>> GetBonusesAsync(int offset = 0, int limit = 0, string? search = null, CancellationToken cancellationToken = default);
}