namespace StructAI.Model;

public class CicSalesProjection
{
    public List<CicPriceItem> PriceItems { get; set; } = new()
    {
        new() { Name = "Assessment", Price = 545, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "Debrief", Price = 780, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV & Debrief", Price = 1325, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV pilot of 3", Price = 1635, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV pilot of 6", Price = 3000, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 }
    };

    public double AssessmentPrice { get; set; }
    public double DebriefPrice { get; set; }
    public double AdvDebriefPrice { get; set; }
    public double AdvPilotThreePrice { get; set; }
    public double AdvPilotSixPrice { get; set; }
    public double MonthlyCoachingPrice { get; set; }
    public double InHouseAiMonthlyPrice { get; set; }
    public int ProjectionYear { get; set; }

    public void SyncLegacyPrices()
    {
        var prices = PriceItems
            .Select(item => item.Price)
            .ToList();

        if (prices.Count > 0) AssessmentPrice = prices[0];
        if (prices.Count > 1) DebriefPrice = prices[1];
        if (prices.Count > 2) AdvDebriefPrice = prices[2];
        if (prices.Count > 3) AdvPilotThreePrice = prices[3];
        if (prices.Count > 4) AdvPilotSixPrice = prices[4];
    }
}

public class CicPriceItem
{
    public string Name { get; set; } = "";
    public double Price { get; set; }
    public int UnitsPerWeek { get; set; } = 1;
    public int UnitsPerMonth { get; set; } = 4;
    public int UnitsPerYear { get; set; } = 48;

    public int AnnualUnitCount =>
        (UnitsPerWeek * 52) +
        (UnitsPerMonth * 12) +
        UnitsPerYear;

    public double AnnualRevenue => Price * AnnualUnitCount;
}
