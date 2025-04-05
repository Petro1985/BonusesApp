using BonusesApp.Core.Infrastructure;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Bonuses.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BonusesApp.Core.Services.Bonuses;

public class BonusService(ApplicationDbContext appContext) : IBonusService
{
    public async Task<List<BonusesEntity>> GetBonusesAsync(int offset = 0, int limit = 0, string? search = null, CancellationToken cancellationToken = default)
    {
        IQueryable<BonusesEntity> query = appContext.Bonuses; 
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.Like(x.PhoneNumber, $"%{search}%"));
        }
        
        var bonuses = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        return bonuses;
    }

    public async Task AddBonusesAsync(BonusesEntity newBonuses, CancellationToken cancellationToken = default)
    {
        appContext.Bonuses.Add(newBonuses);
        await appContext.SaveChangesAsync(cancellationToken);
    }
}