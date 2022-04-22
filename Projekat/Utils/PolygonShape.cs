using System.Windows.Media;

namespace Projekat.Utils
{
    public class PolygonShape
    {
        public int Conture { get; set; }
        public Brush Fill { get; set; }
        public Brush Border { get; set; }
        public bool Condition { get; set; }

        public PolygonShape() => Condition = false;

        public PolygonShape(int conture, Brush fill, Brush border, bool condition)
        {
            Conture = conture;
            Fill = fill;
            Border = border;
            Condition = condition;
        }
    }
}