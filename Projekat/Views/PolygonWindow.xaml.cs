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
    /// Interaction logic for PolygonWindow.xaml
    /// </summary>
    public partial class PolygonWindow : Window
    {
        private int _conture = 0;

        public PolygonWindow()
        {
            InitializeComponent();
            FillColor.ItemsSource = typeof(Brushes).GetProperties();
            BorderColor.ItemsSource = typeof(Brushes).GetProperties();
        }

        public PolygonWindow(int conture, Brush fill, Brush border)
        {
            InitializeComponent();
            FillColor.ItemsSource = typeof(Brushes).GetProperties();
            BorderColor.ItemsSource = typeof(Brushes).GetProperties();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            MainWindow.Polygon = new PolygonShape(Math.Abs(_conture),
                (Brush)(FillColor.SelectedItem as PropertyInfo)?.GetValue(null, null),
                (Brush)(BorderColor.SelectedItem as PropertyInfo)?.GetValue(null, null), true);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Ellipse.Condition = false;
            this.Close();
        }

        private bool Validate()
        {
            if (!Int32.TryParse(ContureLine.Text, out _conture)) return false;
            if (String.IsNullOrEmpty(FillColor.Text)) return false;
            if (String.IsNullOrEmpty(BorderColor.Text)) return false;

            return true;
        }
    }
}
