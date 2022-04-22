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

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            MainWindow.Text = new TextShape(Text.Text, _font,
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
