namespace BonusesApp.Server.ViewModels.Bonuses;

public class AddBonusesRequest
{
    public string PhoneNumber { get; set; }
    public string Name { get; set; }
    public int TotalCounter { get; set; }
    public int CurrentCounter { get; set; }
    public int Setting { get; set; }
    
    public string? Comment { get; set; }
}