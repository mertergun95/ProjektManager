using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjektManager.Views
{
    public partial class ProjektVisualisierungPage : UserControl
    {
        private readonly MainWindow _main;
        private readonly Projekt _projekt;

        public ProjektVisualisierungPage(MainWindow main, Projekt projekt)
        {
            InitializeComponent();
            _main = main;
            _projekt = projekt;

            ProjektTitel.Text = $"Projekt: {_projekt.Name}";

            ErzeugeLaengeBloecke();
            ErzeugeProjektZusammenfassung();
        }

        private void ErzeugeLaengeBloecke()
        {
            LaengeBar.Children.Clear();
            KmBar.Children.Clear();

            const double fixedWidth = 180;
            const double blockMargin = 2;
            double currentOffset = 20;

            LaengeBar.Children.Add(new Border { Width = 20, Height = 0, Background = Brushes.Transparent });

            for (int i = 0; i < _projekt.Laengen.Count; i++)
            {
                var laenge = _projekt.Laengen[i];

                var block = new Border
                {
                    Width = fixedWidth,
                    Height = 60,
                    Background = laenge.KabelVerlegt ? VisualBuilder.Success : VisualBuilder.Accent,
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(blockMargin),
                    Child = new TextBlock
                    {
                        Text = laenge.Bezeichnung,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.SemiBold,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = laenge
                };

                block.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount != 2) return;
                    var gewaehlteLaenge = (Laenge)((Border)s!).Tag;
                    _main.AktuellesProjekt = _projekt;
                    _main.ZeigeSeite(new LaengeDetailPage(_main, gewaehlteLaenge));
                };

                LaengeBar.Children.Add(block);

                if (i == 0)
                {
                    double kmVon = laenge.Leistungen.FirstOrDefault()?.KmVon ?? 0;
                    var kmBlock = VisualBuilder.ErzeugeKmMarker($"{kmVon:F3} km");
                    Canvas.SetLeft(kmBlock, currentOffset);
                    KmBar.Children.Add(kmBlock);
                }

                double kmBis = laenge.Leistungen.LastOrDefault()?.KmBis ?? 0;
                currentOffset += fixedWidth + blockMargin * 2;

                var kmBisBlock = VisualBuilder.ErzeugeKmMarker($"{kmBis:F3} km");
                Canvas.SetLeft(kmBisBlock, currentOffset);
                KmBar.Children.Add(kmBisBlock);
            }

            LaengeBar.Children.Add(new Border { Width = 20, Height = 0, Background = Brushes.Transparent });
        }

        private void ErzeugeProjektZusammenfassung()
        {
            ProjektZusammenfassungPanel.Children.Clear();

            var alleLeistungen = _projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            ProjektZusammenfassungPanel.Children.Add(VisualBuilder.ErzeugeZusammenfassung(alleLeistungen));
        }
    }
}
