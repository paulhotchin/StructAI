namespace StructAI.Model;

public class CicSalesProjection
{
    public List<CicPriceItem> PriceItems { get; set; } = new();

    public int ProjectionYear { get; set; }

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
