using BonusesApp.Core.Extensions;
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
            search = search.ToLower();
            query = query
                .Where(x => 
                    EF.Functions.Like(x.PhoneNumber, $"%{search}%") ||
                    EF.Functions.Like(x.Name.ToLower(), $"%{search}%"));
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

    public async Task SetSettingToAll(int settingValue, CancellationToken cancellationToken = default)
    {
        var allBonuses = await appContext.Bonuses.ToListAsync(cancellationToken);
        foreach (var bonus in allBonuses)
        {
            bonus.Setting = settingValue;
        }
        appContext.UpdateRange(allBonuses);
        await appContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GiveBonusesAsync(int id, CancellationToken cancellationToken)
    {
        var bonuses = await appContext.Bonuses
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (bonuses == null) return;

        bonuses.CurrentCount -= bonuses.Setting;
        appContext.Update(bonuses);
        await appContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BonusesImportResult> ImportBonusesAsync(
        IReadOnlyList<BonusesImportItem> rows,
        CancellationToken cancellationToken = default)
    {
        var existing = await appContext.Bonuses
            .OrderBy(x => x.Name)
            .ThenBy(x => x.PhoneNumber)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var pending = existing
            .GroupBy(x => NormalizePhone(x.PhoneNumber))
            .ToDictionary(group => group.Key, group => new Queue<BonusesEntity>(group));

        var result = new BonusesImportResult();

        foreach (var row in rows)
        {
            var key = NormalizePhone(row.PhoneNumber);
            if (pending.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                ApplyImport(queue.Dequeue(), row);
                result.Updated++;
                continue;
            }

            appContext.Bonuses.Add(new BonusesEntity
            {
                PhoneNumber = row.PhoneNumber,
                Name = row.Name,
                Comment = row.Comment,
                TotalCount = row.TotalCount,
                CurrentCount = row.CurrentCount,
                Setting = row.Setting
            });
            result.Created++;
        }

        await appContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static void ApplyImport(BonusesEntity entity, BonusesImportItem row)
    {
        entity.PhoneNumber = row.PhoneNumber;
        entity.Name = row.Name;
        entity.Comment = row.Comment;
        entity.TotalCount = row.TotalCount;
        entity.CurrentCount = row.CurrentCount;
        entity.Setting = row.Setting;
    }

    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return "";
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '8')
        {
            digits = "7" + digits[1..];
        }

        if (digits.Length == 10)
        {
            digits = "7" + digits;
        }

        return digits.Length > 0 ? digits : phone.Trim().ToLowerInvariant();
    }
}