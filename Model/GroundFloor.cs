namespace StructAI.Model;

public class GroundFloor {
    public Site Site { get; set; } = new();
    public Slab Slab { get; set; } = new();
}

public class Site {
    public double RealNorthDegrees { get; set; }
    public List<Point> Boundary { get; set; } = new();
}

public class Slab {
    public double Thickness { get; set; }
    public List<Point> Perimeter { get; set; } = new();
}

public class Point {
    public double X { get; set; }
    public double Y { get; set; }
}
