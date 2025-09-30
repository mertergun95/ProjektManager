using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ProjektManager.Models;

namespace ProjektManager.Views
{
    public partial class LSTVisualisierungPage : UserControl
    {
        private readonly List<LSTKabel> _kabelListe;
        private double _scale;
        private readonly double _kabelHoehe = 24;
        private readonly double _zeilenabstand = 40;
        private readonly double _minBarTextBreite = 100;

        public LSTVisualisierungPage(List<LSTKabel> kabelListe)
        {
            InitializeComponent();
            _kabelListe = kabelListe;
            this.Loaded += (s, e) => ZeichneVisualisierung();
        }

        private void ZeichneAdaptiveRuler(double minM)
        {
            var kmDict = new Dictionary<double, int>();
            foreach (var k in _kabelListe)
            {
                if (!kmDict.ContainsKey(k.KmVon)) kmDict[k.KmVon] = 0;
                if (!kmDict.ContainsKey(k.KmBis)) kmDict[k.KmBis] = 0;
                kmDict[k.KmVon]++;
                kmDict[k.KmBis]++;
            }

            var relevanteKms = kmDict
                .Where(x => x.Value >= 2)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();

            double lastDrawnX = double.MinValue;
            double minDistancePx = 60;

            foreach (var km in relevanteKms)
            {
                double x = (km - minM) * _scale;
                if (x - lastDrawnX < minDistancePx)
                    continue;

                var label = new TextBlock
                {
                    Text = (km / 1000.0).ToString("0.000") + " km",
                    FontSize = 10,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(label, x - 20);
                Canvas.SetTop(label, 0);
                KabelZeichenPanel.Children.Add(label);

                var line = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 15,
                    Y2 = KabelZeichenPanel.Height,
                    Stroke = Brushes.LightSlateGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection() { 2, 4 }
                };
                KabelZeichenPanel.Children.Add(line);

                lastDrawnX = x;
            }
        }

        private void ZeichneVisualisierung()
        {
            KabelZeichenPanel.Children.Clear();
            if (_kabelListe == null || _kabelListe.Count == 0) return;

            double minM = _kabelListe.Min(k => k.KmVon);
            double sichtbareBreite = this.ActualWidth > 0 ? this.ActualWidth : 1200;

            int minLength = _kabelListe.Min(k => k.LängeSoll > 0 ? k.LängeSoll : int.MaxValue);
            int maxLength = _kabelListe.Max(k => k.LängeSoll);

            _scale = Math.Min(
                sichtbareBreite / (maxLength + 300),
                _minBarTextBreite / Math.Max(minLength, 1)
            );

            KabelZeichenPanel.Width = sichtbareBreite;

            for (int i = 0; i < _kabelListe.Count; i++)
            {
                var k = _kabelListe[i];

                double xStart = (k.KmVon - minM) * _scale;
                double breite = Math.Max(_minBarTextBreite, k.LängeSoll * _scale);
                double y = 50 + i * _zeilenabstand;

                // 🔷 Arka plan boyama
                Brush backgroundBrush = k.IstAbgerechnet
                    ? Brushes.Green
                    : (k.VerlegtMeter > 0
                        ? new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop(Colors.Blue, 0),
                                new GradientStop(Colors.Blue, k.VerlegtMeter / k.LängeSoll),
                                new GradientStop(Colors.LightGray, k.VerlegtMeter / k.LängeSoll),
                                new GradientStop(Colors.LightGray, 1)
                            }
                        }
                        : Brushes.LightGray);

                var container = new Border
                {
                    Width = breite,
                    Height = _kabelHoehe,
                    Background = backgroundBrush,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    ToolTip = new ToolTip
                    {
                        Content = $"{k.KabelNr} ({k.LängeSoll} m)\n" +
                                  $"Typ: {k.Kabeltyp}\n" +
                                  $"Querschnitt: {k.Kabelquerschnitt}\n" +
                                  $"Von: {k.VonPunkt} ({k.KmVonRaw} m)\n" +
                                  $"Bis: {k.BisPunkt} ({k.KmBisRaw} m)\n" +
                                  $"Trommel: {k.Trommelnummer}\n" +
                                  $"Bestell: {k.Bestelllaenge ?? 0} m"
                    },
                    Child = new TextBlock
                    {
                        Text = $"{k.KabelNr} ({k.LängeSoll} m)",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        TextWrapping = TextWrapping.NoWrap,
                        Foreground = Brushes.Black
                    }
                };

                // Sağ tık menüsü
                var contextMenu = new ContextMenu();

                var verlegtItem = new MenuItem { Header = "Kabel verlegt" };
                verlegtItem.Click += (s, e) =>
                {
                    var dialog = new KabelVerlegenWindow((int)k.LängeSoll, k.VerlegtMeter);
                    dialog.Owner = Window.GetWindow(this);
                    if (dialog.ShowDialog() == true)
                    {
                        k.VerlegtMeter = dialog.VerlegtMeter;
                        k.VerlegeDatum = DateTime.Now;
                        ZeichneVisualisierung();
                    }
                };

                var abrechItem = new MenuItem { Header = "Kabel abgerechnet" };
                abrechItem.Click += (s, e) =>
                {
                    k.IstAbgerechnet = true;
                    k.VerlegtMeter = k.LängeSoll; // otomatik tamamlandı
                    ZeichneVisualisierung();
                };

                contextMenu.Items.Add(verlegtItem);
                contextMenu.Items.Add(abrechItem);
                container.ContextMenu = contextMenu;

                Canvas.SetLeft(container, xStart);
                Canvas.SetTop(container, y);
                KabelZeichenPanel.Children.Add(container);

                // VonPunkt
                if (!string.IsNullOrWhiteSpace(k.VonPunkt))
                {
                    var vonLabel = new TextBlock
                    {
                        Text = k.VonPunkt,
                        FontSize = 9,
                        Foreground = Brushes.DarkSlateGray
                    };
                    Canvas.SetLeft(vonLabel, xStart);
                    Canvas.SetTop(vonLabel, y - 12);
                    KabelZeichenPanel.Children.Add(vonLabel);
                }

                // BisPunkt
                if (!string.IsNullOrWhiteSpace(k.BisPunkt))
                {
                    var bisLabel = new TextBlock
                    {
                        Text = k.BisPunkt,
                        FontSize = 9,
                        Foreground = Brushes.DarkSlateGray
                    };
                    Canvas.SetLeft(bisLabel, xStart + breite - 30);
                    Canvas.SetTop(bisLabel, y - 12);
                    KabelZeichenPanel.Children.Add(bisLabel);
                }
            }

            KabelZeichenPanel.Height = _kabelListe.Count * _zeilenabstand + 100;
            ZeichneAdaptiveRuler(minM);
        }
    }
}
