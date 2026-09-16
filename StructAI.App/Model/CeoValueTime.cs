// E:\work\TQ\Kepler\StructAI\StructAI.App\Model\CeoValueTime.cs
namespace StructAI.Model;

public class CeoValueTime
{
    // Inputs
    public double HourlyRate { get; set; }
    public double HoursPerWeek { get; set; }
    public double AdminHours { get; set; }
    public double InterruptionsPerDay { get; set; }
    public double MinutesLostPerInterruption { get; set; }

    // Computed outputs
    public double WeeklyCostAdmin { get; set; }
    public double WeeklyCostInterruptions { get; set; }
    public double TotalWeeklyLoss { get; set; }
    public double AnnualLoss { get; set; }
}
