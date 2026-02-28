namespace BonusesApp.Core.Models.Bonuses;

public class BonusesEntity : BaseEntity
{
    public string PhoneNumber { get; set; }
    
    public string Name { get; set; }
    
    public string? Comment { get; set; }

    public int TotalCount { get; set; }
    
    public int CurrentCount { get; set; }

    public int Setting { get; set; }
}