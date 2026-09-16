// E:\work\TQ\Kepler\StructAI\StructAI.App\Model\GroundFloor.cs
namespace StructAI.Model;

public class GroundFloor {
    public Site Site { get; set; } = new();
    public Slab Slab { get; set; } = new();
}

public class Site {
    public double RealNorthDegrees { get; set; }

    // Default boundary polygon (restored)
    public List<Point> Boundary { get; set; } = new()
    {
        new Point { X = 0, Y = 0 },
        new Point { X = 14000, Y = 0 },
        // new Point { X = 14000, Y = 9500 },
        new Point { X = 0, Y = 9500 }
    };

    // Computed fields
    public double BoundaryPerimeter { get; set; }
    public double BoundaryArea { get; set; }    
}

public class Slab {
    public double Thickness { get; set; }
    public List<Point> Perimeter { get; set; } = new();
}

public class Point {
    public double X { get; set; }
    public double Y { get; set; }
}
