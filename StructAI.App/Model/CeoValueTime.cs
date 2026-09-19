// E:\work\TQ\Kepler\StructAI\StructAI.App\Model\CeoValueTime.cs
namespace StructAI.Model;

public class CeoValueTime
{
    public double WeeksPerYear { get; set; }
    public double HoursPerWeek { get; set; }
    public double AnnualGrowthPercent { get; set; }

    public double AnnualHours { get; set; }
    public double AnnualValue { get; set; }
    public double AnnualValueWithGrowth { get; set; }
}
