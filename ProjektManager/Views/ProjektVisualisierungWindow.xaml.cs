using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ProjektManager.Models;
using System.Linq;
using System.Windows.Shapes;

namespace ProjektManager.Views
{
    public partial class ProjektVisualisierungPage : UserControl
    {
        private MainWindow _main;          // Ana pencere referansı
        private Projekt _projekt;          // Görselleştirilecek proje verisi

        // Constructor: Sayfa oluşturulurken ana pencere ve proje verisi alınır
        public ProjektVisualisierungPage(MainWindow main, Projekt projekt)
        {
            InitializeComponent();
            _main = main;
            _projekt = projekt;

            // Sayfanın başlığını projenin adıyla güncelle
            ProjektTitel.Text = $"Projekt: {_projekt.Name}";

            // Längelerin (uzunluk bloklarının) ve kilometre gösterimlerinin oluşturulması
            ErzeugeLaengeBloecke();
            ErzeugeProjektZusammenfassung();

        }

        // Ana uzunluk ve kilometre bloklarını oluşturan metod
        private void ErzeugeLaengeBloecke()
        {
            LaengeBar.Children.Clear();
            KmBar.Children.Clear();

            double fixedWidth = 180;
            double currentOffset = 20;

            var spacerStart = new Border { Width = 20, Height = 0, Background = Brushes.Transparent };
            LaengeBar.Children.Add(spacerStart);

            for (int i = 0; i < _projekt.Laengen.Count; i++)
            {
                var laenge = _projekt.Laengen[i];

                var block = new Border
                {
                    Width = fixedWidth,
                    Height = 60,
                    Background = laenge.KabelVerlegt ? Brushes.LightGreen : Brushes.LightBlue,
                    BorderBrush = Brushes.DarkGray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    Child = new TextBlock
                    {
                        Text = laenge.Bezeichnung,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Tag = laenge // 👈 Länge nesnesini tag olarak kaydet
                };

                block.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        var gewaehlteLaenge = (Laenge)((Border)s).Tag;
                        _main.AktuellesProjekt = _projekt; // 👈 doğru şekilde set et
                        _main.ZeigeSeite(new LaengeDetailPage(_main, gewaehlteLaenge));
                    }
                };

                LaengeBar.Children.Add(block);

                if (i == 0)
                {
                    double kmVon = laenge.Leistungen.FirstOrDefault()?.KmVon ?? 0;
                    var kmBlock = ErzeugeKmBlock($"{kmVon:F3} km");
                    Canvas.SetLeft(kmBlock, currentOffset);
                    KmBar.Children.Add(kmBlock);
                }

                double kmBis = laenge.Leistungen.LastOrDefault()?.KmBis ?? 0;
                double blockMargin = 2;
                currentOffset += fixedWidth + blockMargin * 2;

                var kmBisBlock = ErzeugeKmBlock($"{kmBis:F3} km");
                Canvas.SetLeft(kmBisBlock, currentOffset);
                KmBar.Children.Add(kmBisBlock);
            }

            var spacerEnd = new Border { Width = 20, Height = 0, Background = Brushes.Transparent };
            LaengeBar.Children.Add(spacerEnd);
        }

        private StackPanel ErzeugeKmBlock(string text)
        {
            var kmText = new TextBlock
            {
                Text = text,
                LayoutTransform = new RotateTransform(-90),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(-35, 0, 0, 5)
            };

            var arrow = new Polygon
            {
                Points = new PointCollection { new Point(5, 0), new Point(0, 10), new Point(10, 10) },
                Fill = Brushes.Black,
                Width = 10,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(-35, 0, 0, 0)
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 40
            };

            stack.Children.Add(kmText);
            stack.Children.Add(arrow);
            return stack;
        }

        private void ErzeugeProjektZusammenfassung()
        {
            ProjektZusammenfassungPanel.Children.Clear();

            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 500
            };

            decimal gesamtMeter = _projekt.Laengen
                .SelectMany(l => l.Leistungen)
                .Sum(l => (decimal)(l.LaengeMeter ?? 0));

            decimal erledigtMeter = _projekt.Laengen
                .SelectMany(l => l.Leistungen)
                .Where(l => l.IstFertiggestellt)
                .Sum(l => (decimal)(l.LaengeMeter ?? 0));

            decimal abgerechnetMeter = _projekt.Laengen
                .SelectMany(l => l.Leistungen)
                .Where(l => l.IstAbgerechnet)
                .Sum(l => (decimal)(l.LaengeMeter ?? 0));

            double erledigtProzent = gesamtMeter > 0 ? (double)(erledigtMeter / gesamtMeter) * 100 : 0;
            double abgerechnetProzent = gesamtMeter > 0 ? (double)(abgerechnetMeter / gesamtMeter) * 100 : 0;

            // Donut gösterimleri
            var donutContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            donutContainer.Children.Add(ErzeugeDonut(erledigtProzent, "Erledigt", Brushes.SeaGreen));
            donutContainer.Children.Add(ErzeugeDonut(abgerechnetProzent, "Abgerechnet", Brushes.SteelBlue));

            container.Children.Add(donutContainer);

            // Pozisyon türlerine göre grup özetleri
            var gruppen = _projekt.Laengen
                .SelectMany(l => l.Leistungen)
                .GroupBy(l => BestimmeGruppe(l.Leistungsbeschreibung))
                .Select(g =>
                {
                    var istStueck = g.All(l => (l.LaengeMeter ?? 0) == 0);

                    double gesamt = istStueck
                        ? g.Count()
                        : g.Sum(l => l.LaengeMeter ?? 0);

                    double erledigt = istStueck
                        ? g.Count(l => l.IstFertiggestellt)
                        : g.Where(l => l.IstFertiggestellt).Sum(l => l.LaengeMeter ?? 0);

                    return new
                    {
                        Beschreibung = g.Key,
                        Gesamt = gesamt,
                        Erledigt = erledigt,
                        IstStueck = istStueck
                    };
                });


            foreach (var item in gruppen)
            {
                var titel = new TextBlock
                {
                    Text = $"• {item.Beschreibung}",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 2)
                };
                container.Children.Add(titel);

                var grid = new Grid { Height = 30, Margin = new Thickness(0, 5, 0, 10) };

                var border = new Border
                {
                    CornerRadius = new CornerRadius(10),
                    Background = Brushes.LightGray,
                    Height = 30
                };

                var pb = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = item.IstStueck ? (item.Erledigt + 1) : item.Gesamt,
                    Value = item.Erledigt,
                    Foreground = Brushes.SeaGreen,
                    Background = Brushes.Transparent,
                    Height = 30
                };

                var text = new TextBlock
                {
                    Text = item.IstStueck
                        ? $"{item.Erledigt} / {item.Gesamt} Stück"
                        : $"{item.Erledigt:F0} m / {item.Gesamt:F0} m",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };

                grid.Children.Add(border);
                grid.Children.Add(pb);
                grid.Children.Add(text);
                container.Children.Add(grid);
            }

            ProjektZusammenfassungPanel.Children.Add(container);
        }
        private string BestimmeGruppe(string beschreibung)
        {
            beschreibung = beschreibung.ToLower();

            if (beschreibung.Contains("vorh. kk") || beschreibung.Contains("vorh. bkk"))
                return "vorh. BKK";

            if (beschreibung.Contains("vorh. querung") || beschreibung.Contains("vorh. gq") || beschreibung.Contains("vorh. gleisquerung"))
                return "vorh. GQ";

            if (beschreibung.Contains("vorh. sq") || beschreibung.Contains("vorh. straßenquerung") || beschreibung.Contains("vorh. rohranlage"))
                return "vorh. Rohranlage";

            if (beschreibung.Contains("vam"))
                return "VAM";

            return beschreibung; // Diğerleri olduğu gibi
        }

        private StackPanel ErzeugeDonut(double prozent, string titel, Brush farbe)
        {
            prozent = Math.Round(prozent, 1);
            if (prozent >= 100) prozent = 99.999;  // kritik satır

            double angle = prozent * 360.0 / 100.0;
            double radius = 50;
            double center = radius + 5;

            Point startPoint = new Point(center, center - radius);

            double endX = center + radius * Math.Sin(angle * Math.PI / 180);
            double endY = center - radius * Math.Cos(angle * Math.PI / 180);
            Point endPoint = new Point(endX, endY);

            bool isLargeArc = angle > 180;

            var arcSegment = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = isLargeArc,
                SweepDirection = SweepDirection.Clockwise
            };

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                Segments = new PathSegmentCollection { arcSegment },
                IsClosed = false
            };

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            var path = new System.Windows.Shapes.Path
            {
                Stroke = farbe,
                StrokeThickness = 10,
                Data = geometry,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = radius * 2 + 10,
                Height = radius * 2 + 10
            };

            var backgroundCircle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.LightGray,
                StrokeThickness = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var donutGrid = new Grid
            {
                Width = radius * 2 + 10,
                Height = radius * 2 + 10
            };
            donutGrid.Children.Add(backgroundCircle);
            donutGrid.Children.Add(path);
            donutGrid.Children.Add(new TextBlock
            {
                Text = $"{prozent:F0} %",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            var titelText = new TextBlock
            {
                Text = titel,
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(donutGrid);
            stack.Children.Add(titelText);

            return stack;
        }


    }
}
