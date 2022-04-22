using System.Windows.Media;

namespace Projekat.Utils
{
    public class TextShape
    {
        public string Text { get; set; }
        public double Font { get; set; }
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public bool Condition { get; set; }

        public TextShape() => Condition = false;

        public TextShape(string text, double font, Brush foreground, Brush background, bool condition)
        {
            Text = text;
            Font = font;
            Foreground = foreground;
            Background = background;
            Condition = condition;
        }
    }
}