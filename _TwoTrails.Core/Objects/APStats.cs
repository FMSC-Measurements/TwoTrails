namespace TwoTrails.Core
{
    public class APStats
    {
        public double Area { get; }
        public double Perimeter { get; }
        public double LinePerimeter { get; }

        public APStats(double area = 0, double perimeter = 0, double linePerimeter = 0)
        {
            this.Area = area;
            this.Perimeter = perimeter;
            this.LinePerimeter = linePerimeter;
        }
    }
}
