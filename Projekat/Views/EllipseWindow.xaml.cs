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
    /// Interaction logic for EllipseWindow.xaml
    /// </summary>
    public partial class EllipseWindow : Window
    {
        private double _radiusX = 0, _radiusY = 0;
        private int _conture = 0;

        public EllipseWindow()
        {
            InitializeComponent();
            FillColor.ItemsSource = typeof(Brushes).GetProperties();
            BorderColor.ItemsSource = typeof(Brushes).GetProperties();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            MainWindow.Ellipse = new EllipseShape(_radiusX, _radiusY, _conture,
                (Brush) (FillColor.SelectedItem as PropertyInfo)?.GetValue(null, null),
                (Brush) (BorderColor.SelectedItem as PropertyInfo)?.GetValue(null, null), true);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Ellipse.Condition = false;

            this.Close();
        }

        private bool Validate()
        {
            if (!Double.TryParse(RadiusX.Text, out _radiusX)) return false;

            if (!Double.TryParse(RadiusY.Text, out _radiusY)) return false;

            if (!Int32.TryParse(ContureLine.Text, out _conture)) return false;

            if (String.IsNullOrEmpty(FillColor.Text)) return false;

            if (String.IsNullOrEmpty(BorderColor.Text)) return false;

            return true;
        }
    }
}
