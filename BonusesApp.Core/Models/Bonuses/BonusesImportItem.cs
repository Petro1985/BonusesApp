namespace BonusesApp.Core.Models.Bonuses;

public class BonusesImportItem
{
    public string PhoneNumber { get; set; } = "";

    public string Name { get; set; } = "";

    public string? Comment { get; set; }

    public int TotalCount { get; set; }

    public int CurrentCount { get; set; }

    public int Setting { get; set; }
}

public class BonusesImportResult
{
    public int Created { get; set; }

    public int Updated { get; set; }
}
