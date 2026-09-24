namespace StructAI.Model;

public class CicSalesProjection
{
    public List<CicPriceItem> PriceItems { get; set; } = new();

    public int ProjectionYear { get; set; }

}

public class CicPriceItem
{
    public const int WORK_WEEKS_PER_YEAR = 48;
    public const int WORK_MONTHS_PER_YEAR = 12;
    public const int WORK_WEEKS_PER_MONTH = 4;

    public string Name { get; set; } = "";
    public double Price { get; set; }
    public double ProfitFactor { get; set; } = 0.4;
    public bool Perennial { get; set; }
    public int Units { get; set; } = 1;
    public CicPeriod Period { get; set; } = CicPeriod.Week;
    public bool PeriodLocked { get; set; }
    public double UnitsPerWeek { get; set; } = 1;
    public double UnitsPerMonth { get; set; } = WORK_WEEKS_PER_MONTH;
    public double UnitsPerYear { get; set; } = WORK_WEEKS_PER_YEAR;

    public double AnnualUnitCount =>
        (UnitsPerWeek * WORK_WEEKS_PER_YEAR) +
        (UnitsPerMonth * WORK_MONTHS_PER_YEAR) +
        UnitsPerYear;

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
