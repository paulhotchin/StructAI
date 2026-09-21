namespace StructAI.Model;

public class CicSalesProjection
{
    public List<CicPriceItem> PriceItems { get; set; } = new()
    {
        new() { Name = "Assessment", Price = 545, ProfitFactor = 0.4, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "Debrief", Price = 780, ProfitFactor = 0.8, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV & Debrief", Price = 1325, ProfitFactor = 0.4, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV pilot of 3", Price = 1635, ProfitFactor = 0.4, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new() { Name = "ADV pilot of 6", Price = 3000, ProfitFactor = 0.4, UnitsPerWeek = 1, UnitsPerMonth = 4, UnitsPerYear = 48 },
        new()
        {
            Name = "Coaching Contract",
            Price = 1250,
            ProfitFactor = 0.8,
            Perennial = true,
            Period = CicPeriod.AprAugNov,
            Units = 1,
            UnitsPerWeek = 3d / 52d,
            UnitsPerMonth = 3d / 12d,
            UnitsPerYear = 3,
            PeriodLocked = true
        },
        new()
        {
            Name = "Client Account AI",
            Price = 6200,
            ProfitFactor = 0.8,
            Perennial = true,
            Period = CicPeriod.AprJulOct,
            Units = 1,
            UnitsPerWeek = 3d / 52d,
            UnitsPerMonth = 3d / 12d,
            UnitsPerYear = 3,
            PeriodLocked = true
        }
    };

    public double AssessmentPrice { get; set; }
    public double DebriefPrice { get; set; }
    public double AdvDebriefPrice { get; set; }
    public double AdvPilotThreePrice { get; set; }
    public double AdvPilotSixPrice { get; set; }
    public double MonthlyCoachingPrice { get; set; }
    public double InHouseAiMonthlyPrice { get; set; }
    public int ProjectionYear { get; set; }

    public void EnsureRequiredItems()
    {
        if (!PriceItems.Any(item => item.Name == "Coaching Contract"))
        {
            PriceItems.Add(new CicPriceItem
            {
                Name = "Coaching Contract",
                Price = MonthlyCoachingPrice,
                ProfitFactor = 0.8,
                Perennial = true,
                Period = CicPeriod.AprAugNov,
                Units = 1,
                UnitsPerWeek = 3d / 52d,
                UnitsPerMonth = 3d / 12d,
                UnitsPerYear = 3,
                PeriodLocked = true
            });
        }

        if (!PriceItems.Any(item => item.Name == "Client Account AI"))
        {
            PriceItems.Add(new CicPriceItem
            {
                Name = "Client Account AI",
                Price = InHouseAiMonthlyPrice,
                ProfitFactor = 0.8,
                Perennial = true,
                Period = CicPeriod.AprJulOct,
                Units = 1,
                UnitsPerWeek = 3d / 52d,
                UnitsPerMonth = 3d / 12d,
                UnitsPerYear = 3,
                PeriodLocked = true
            });
        }

        var coachingContract = PriceItems.First(item => item.Name == "Coaching Contract");
        if (coachingContract.Price == 0)
            coachingContract.Price = MonthlyCoachingPrice;

        var clientAccountAi = PriceItems.First(item => item.Name == "Client Account AI");
        if (clientAccountAi.Price == 0)
            clientAccountAi.Price = InHouseAiMonthlyPrice;

        var defaultProfitFactors = new Dictionary<string, double>
        {
            ["Assessment"] = 0.4,
            ["Debrief"] = 0.8,
            ["ADV & Debrief"] = 0.4,
            ["ADV pilot of 3"] = 0.4,
            ["ADV pilot of 6"] = 0.4,
            ["Coaching Contract"] = 0.8,
            ["Client Account AI"] = 0.8
        };

        foreach (var item in PriceItems)
        {
            if (defaultProfitFactors.TryGetValue(item.Name, out var factor))
                item.ProfitFactor = factor;

            if (item.Name is "Coaching Contract" or "Client Account AI")
                item.Perennial = true;
        }
    }

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
    public double ProfitFactor { get; set; } = 0.4;
    public bool Perennial { get; set; }
    public int Units { get; set; } = 1;
    public CicPeriod Period { get; set; } = CicPeriod.Week;
    public bool PeriodLocked { get; set; }
    public double UnitsPerWeek { get; set; } = 1;
    public double UnitsPerMonth { get; set; } = 4;
    public double UnitsPerYear { get; set; } = 48;

    public double AnnualUnitCount =>
        (UnitsPerWeek * 52) +
        (UnitsPerMonth * 12) +
        UnitsPerYear;

    public double AnnualRevenue => Price * AnnualUnitCount;

    public double WeekEquivalentFromMonth => UnitsPerMonth / 4d;
    public double WeekEquivalentFromYear => UnitsPerYear / 52d;
    public double MonthEquivalentFromWeek => UnitsPerWeek / 4d;
    public double MonthEquivalentFromYear => UnitsPerYear / 12d;
    public double YearEquivalentFromWeek => UnitsPerWeek * 52d;
    public double YearEquivalentFromMonth => UnitsPerMonth * 12d;

    public double DisplayUnitsFor(CicPeriod period) => period switch
    {
        CicPeriod.Week => UnitsPerWeek,
        CicPeriod.Month => UnitsPerMonth,
        CicPeriod.Year => UnitsPerYear,
        _ => UnitsPerWeek
    };

    public double RevenueFor(CicPeriod period) => Price * DisplayUnitsFor(period);

    public double RevenuePerWeek => RevenueFor(CicPeriod.Week);
    public double RevenuePerMonth => RevenueFor(CicPeriod.Month);
    public double RevenuePerYear => RevenueFor(CicPeriod.Year);
}

public enum CicPeriod
{
    Week,
    Month,
    Year,
    AprAugNov,
    AprJulOct
}
