using BonusesApp.Core.Models.Bonuses;

namespace BonusesApp.Core.Services.Bonuses.Interfaces;

public interface IBonusService
{
    /// <summary>
    /// Получение бонусов клиентов
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="pageSize"></param>
    /// <param name="search"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(List<BonusesEntity>, int TotalCount)> GetBonusesAsync(int offset = 0, int pageSize = 0, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавление нового клиента
    /// </summary>
    /// <param name="newBonuses"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddBonusesAsync(BonusesEntity newBonuses, CancellationToken cancellationToken = default);
}