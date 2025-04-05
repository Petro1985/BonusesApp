using BonusesApp.Core.Infrastructure;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Bonuses.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BonusesApp.Core.Services.Bonuses;

public class BonusService(ApplicationDbContext appContext) : IBonusService
{
    public async Task<(List<BonusesEntity>, int TotalCount)> GetBonusesAsync(int offset = 0, int pageSize = 0, string? search = null, CancellationToken cancellationToken = default)
    {
        IQueryable<BonusesEntity> query = appContext.Bonuses; 
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.Like(x.PhoneNumber, $"%{search}%"));
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var bonuses = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.PhoneNumber)
            .Skip(offset * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (bonuses, totalCount);
    }

    public async Task DeleteBonusesAsync(int id, CancellationToken cancellationToken = default)
    {
        var bonusesToRemove = new BonusesEntity { Id = id };
        appContext.Attach(bonusesToRemove);
        appContext.Bonuses.Remove(bonusesToRemove);
        await appContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBonusesAsync(BonusesEntity bonuses, CancellationToken cancellationToken = default)
    {
        appContext.Attach(bonuses);
        appContext.Bonuses.Update(bonuses);
        await appContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBonusesAsync(BonusesEntity newBonuses, CancellationToken cancellationToken = default)
    {
        appContext.Bonuses.Add(newBonuses);
        await appContext.SaveChangesAsync(cancellationToken);
    }
}