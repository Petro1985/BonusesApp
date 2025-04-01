namespace BonusesApp.Core.Services.Account.Interfaces;

public interface IUserIdAccessor
{
    string? GetCurrentUserId();
}