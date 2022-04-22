using System;
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

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            MainWindow._color = (Brush)(Colors.SelectedItem as PropertyInfo)?.GetValue(null, null);
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow._color = null;
            this.Close();
        }

        private bool Validate()
        {
            return !String.IsNullOrEmpty(Colors.Text);
        }
    }
}
