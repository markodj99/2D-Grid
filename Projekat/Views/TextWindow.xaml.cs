using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Projekat.Utils;

namespace Projekat.Views
{
    /// <summary>
    /// Interaction logic for TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    { 
        private double _font = 0;

        public TextWindow()
        {
            InitializeComponent();
            Foreground.ItemsSource = typeof(Brushes).GetProperties();
            Background.ItemsSource = typeof(Brushes).GetProperties();
        }

        public TextWindow(string text, double font, string foreground, string background)
        {
            InitializeComponent();
            Foreground.ItemsSource = typeof(Brushes).GetProperties();
            Background.ItemsSource = typeof(Brushes).GetProperties();

            Text.Text = text;
            Text.IsReadOnly = true;

            Font.Text = font.ToString();

            Foreground.Text = foreground.ToString();
            Background.Text = background.ToString();

            BrushConverter converter = new BrushConverter();
            var colors = (from p in typeof(Brushes).GetProperties()
                select ((Brush)converter.ConvertFromString(p.Name))?.Clone().ToString()).ToList();

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].Equals(foreground)) Foreground.SelectedItem = Foreground.Items[i];
                if (colors[i].Equals(background)) Background.SelectedItem = Background.Items[i];
            }
        }


        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            MainWindow.Text = new TextShape(Text.Text, Math.Abs(_font),
                (Brush)(Foreground.SelectedItem as PropertyInfo)?.GetValue(null, null),
                (Brush)(Background.SelectedItem as PropertyInfo)?.GetValue(null, null), true);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Ellipse.Condition = false;
            this.Close();
        }

        private bool Validate()
        {
            if (String.IsNullOrEmpty(Text.Text)) return false;
            if (!Double.TryParse(Font.Text, out _font)) return false;
            if (String.IsNullOrEmpty(Foreground.Text)) return false;
            if (String.IsNullOrEmpty(Background.Text)) return false;

            return true;
        }
    }
}
