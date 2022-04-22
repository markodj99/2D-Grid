using System.Windows.Media;

namespace Projekat.Utils
{
    public class EllipseShape
    {
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public int Conture { get; set; }
        public Brush Fill { get; set; }
        public Brush Border { get; set; }
        public bool Condition { get; set; }

        public EllipseShape() => Condition = false;

        public EllipseShape(double radiusX, double radiusY, int conture, Brush fill, Brush border, bool condition)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            Conture = conture;
            Fill = fill;
            Border = border;
            Condition = condition;
        }
    }
}