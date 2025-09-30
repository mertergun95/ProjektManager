using System;
using System.Collections.Generic;
using System.Linq;
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
using ProjektManager.Models;
using System.IO;

namespace ProjektManager.Views
{
    public partial class LaengeDetailWindow : Window
    {
        private Laenge _laenge;

        public LaengeDetailWindow(Laenge laenge)
        {
            InitializeComponent();
            _laenge = laenge;

            TitelText.Text = $"Länge: {_laenge.Bezeichnung} (km {_laenge.KmVon} – {_laenge.KmBis})";
            ErzeugeLeistungsbloecke();
        }

        private void ErzeugeLeistungsbloecke()
        {
            // Toplam uzunluk
            double gesamtLaenge = _laenge.Leistungen.Sum(l => l.LaengeMeter ?? 0);
            double barPixelGenislik = 1000; // bar toplam genişliği
            double minBreite = 40;

            // Hepsi sıfır mı kontrolü
            bool hepsiSifir = _laenge.Leistungen.All(l => (l.LaengeMeter ?? 0) <= 0);
            int sayi = _laenge.Leistungen.Count;

            foreach (var leistung in _laenge.Leistungen)
            {
                double laengeMeter = leistung.LaengeMeter ?? 0;

                double pixelBreite;
                if (hepsiSifir || gesamtLaenge == 0)
                {
                    pixelBreite = Math.Max(minBreite, barPixelGenislik / sayi);
                }
                else if (laengeMeter <= 0)
                {
                    pixelBreite = minBreite;
                }
                else
                {
                    double oran = laengeMeter / gesamtLaenge;
                    pixelBreite = Math.Max(minBreite, oran * barPixelGenislik);
                }

                var block = new Border
                {
                    Width = pixelBreite,
                    Height = 60,
                    Background = Brushes.LightGreen,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(3),
                    Child = new TextBlock
                    {
                        Text = leistung.Leistungsbeschreibung,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    ToolTip = $"Bahnseite: {leistung.Bahnseite}\nLänge: {(int)laengeMeter} m\nAnmerkung: {leistung.Anmerkung}\nAnmerkung2: {leistung.Anmerkung2}"

                };

                LeistungsBar.Children.Add(block);
            }
        }


    }
}
