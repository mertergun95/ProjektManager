using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class KabelVerlegenWindow : Window
    {
        private readonly int _laengeSoll;

        public double VerlegtMeter { get; private set; }

        public KabelVerlegenWindow(int laengeSoll, double bereitsVerlegt = 0)
        {
            InitializeComponent();
            _laengeSoll = laengeSoll;

            // İlk değer ayarla (önceden girilmişse)
            double initialPercent = (_laengeSoll > 0) ? (bereitsVerlegt / _laengeSoll) * 100 : 0;
            VerlegtSlider.Value = Math.Min(100, Math.Max(0, initialPercent));
        }

        private void VerlegtSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double prozent = e.NewValue;
            ProzentLabel.Text = $"{prozent:F0} %";

            double meter = (_laengeSoll * prozent) / 100.0;
            MeterBox.Text = ((int)Math.Round(meter)).ToString();
        }

        private void Uebernehmen_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MeterBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double meter))
            {
                VerlegtMeter = Math.Min(meter, _laengeSoll);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Bitte gültige Meteranzahl eingeben.");
            }
        }

        private void MeterBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
