namespace BonusesApp.Server.ViewModels.Bonuses;

public class BonusesVM
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string Name { get; set; }
    public int TotalCounter { get; set; }
    public int CurrentCounter { get; set; }
    public int Setting { get; set; }
}