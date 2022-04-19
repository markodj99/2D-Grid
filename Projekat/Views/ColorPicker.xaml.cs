using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace Projekat.Views
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public ColorPicker()
        {
            InitializeComponent();
            Colors.ItemsSource = typeof(Brushes).GetProperties();
        }

        private void Colors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow._color = (Brush)(Colors.SelectedItem as PropertyInfo)?.GetValue(null, null);
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
