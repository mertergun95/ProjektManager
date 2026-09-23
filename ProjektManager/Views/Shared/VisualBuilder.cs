using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ProjektManager.Models;

namespace ProjektManager.Views.Shared
{
    /// <summary>
    /// Wiederverwendbare Bausteine für die (bewusst code-behind gezeichneten) Visualisierungen:
    /// Donut-Diagramme, Km-Marker und die Fortschritts-Zusammenfassung. Wird sowohl von der
    /// Projekt- als auch von der Länge-Ansicht verwendet, damit es nur eine Implementierung gibt.
    /// </summary>
    public static class VisualBuilder
    {
        public static Brush Accent => Res("AccentBrush");
        public static Brush Success => Res("SuccessBrush");
        public static Brush TextPrimary => Res("TextPrimaryBrush");
        public static Brush TextSecondary => Res("TextSecondaryBrush");
        public static Brush Track => Res("TrackBrush");
        public static Brush Surface => Res("SurfaceBrush");
        public static Brush Border => Res("BorderBrush");
        public static Brush Danger => Res("DangerBrush");

        private static Brush Res(string key) => (Brush)Application.Current.Resources[key];

        /// <summary>Zeichnet ein Donut-Diagramm mit Prozentanzeige in der Mitte.</summary>
        public static StackPanel ErzeugeDonut(double prozent, string? titel, Brush farbe, double radius = 46, double strokeDicke = 9)
        {
            prozent = Math.Round(prozent, 1);
            if (prozent > 99.9) prozent = 99.9;

            double angle = prozent * 360.0 / 100.0;
            double center = radius + strokeDicke / 2;
            var startPoint = new Point(center, center - radius);
            double endX = center + radius * Math.Sin(angle * Math.PI / 180);
            double endY = center - radius * Math.Cos(angle * Math.PI / 180);

            var arcSegment = new ArcSegment
            {
                Point = new Point(endX, endY),
                Size = new Size(radius, radius),
                IsLargeArc = angle > 180,
                SweepDirection = SweepDirection.Clockwise
            };

            var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
            figure.Segments.Add(arcSegment);
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            double size = radius * 2 + strokeDicke;

            var path = new Path
            {
                Stroke = farbe,
                StrokeThickness = strokeDicke,
                Data = geometry,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = size,
                Height = size
            };

            var hintergrund = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Track,
                StrokeThickness = strokeDicke,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var donutGrid = new Grid { Width = size, Height = size };
            donutGrid.Children.Add(hintergrund);
            donutGrid.Children.Add(path);
            donutGrid.Children.Add(new TextBlock
            {
                Text = $"{prozent:F0} %",
                FontWeight = FontWeights.Bold,
                FontSize = radius >= 40 ? 14 : 10,
                Foreground = TextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(10), HorizontalAlignment = HorizontalAlignment.Center };
            stack.Children.Add(donutGrid);

            if (!string.IsNullOrEmpty(titel))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = titel,
                    FontSize = 12,
                    Foreground = TextSecondary,
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            return stack;
        }

        /// <summary>Rotierte Km-Beschriftung mit Pfeilmarker für die Kilometer-Leiste.</summary>
        public static StackPanel ErzeugeKmMarker(string text)
        {
            var kmText = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = TextPrimary,
                LayoutTransform = new RotateTransform(-90),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(-35, 0, 0, 5)
            };

            var pfeil = new Polygon
            {
                Points = new PointCollection { new Point(5, 0), new Point(0, 10), new Point(10, 10) },
                Fill = TextSecondary,
                Width = 10,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(-35, 0, 0, 0)
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical, Width = 40 };
            stack.Children.Add(kmText);
            stack.Children.Add(pfeil);
            return stack;
        }

        /// <summary>Eine Titel- + Fortschrittsbalken-Zeile für eine Gruppe von Leistungen.</summary>
        public static FrameworkElement ErzeugeFortschrittsZeile(string titel, double erledigt, double gesamt, bool istStueck)
        {
            var container = new StackPanel();

            container.Children.Add(new TextBlock
            {
                Text = titel,
                FontWeight = FontWeights.SemiBold,
                Foreground = TextPrimary,
                Margin = new Thickness(0, 10, 0, 4)
            });

            var grid = new Grid { Height = 28, Margin = new Thickness(0, 0, 0, 4) };

            grid.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = Track,
                Height = 28
            });

            grid.Children.Add(new ProgressBar
            {
                Minimum = 0,
                Maximum = istStueck ? erledigt + 1 : Math.Max(gesamt, 0.0001),
                Value = erledigt,
                Foreground = Success,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Height = 28
            });

            grid.Children.Add(new TextBlock
            {
                Text = istStueck ? $"{erledigt:F0} / {gesamt:F0} Stück" : $"{erledigt:F0} m / {gesamt:F0} m",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Foreground = TextPrimary
            });

            container.Children.Add(grid);
            return container;
        }

        /// <summary>
        /// Baut die komplette Fortschritts-Zusammenfassung (Erledigt-/Abgerechnet-Donuts + je Gruppe
        /// eine Fortschrittszeile) für eine Menge von Leistungen. <paramref name="gruppierung"/> erlaubt
        /// es, mehrere Leistungsbeschreibungen zu einer Sammelgruppe zusammenzufassen (z.B. auf Projektebene);
        /// ohne Angabe wird exakt nach Leistungsbeschreibung gruppiert.
        /// </summary>
        public static StackPanel ErzeugeZusammenfassung(IReadOnlyCollection<Leistung> leistungen, Func<string, string>? gruppierung = null)
        {
            var wurzel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch };

            decimal gesamtMeter = leistungen.Sum(l => (decimal)(l.LaengeMeter ?? 0));
            decimal erledigtMeter = leistungen.Where(l => l.IstFertiggestellt).Sum(l => (decimal)(l.LaengeMeter ?? 0));
            decimal abgerechnetMeter = leistungen.Where(l => l.IstAbgerechnet).Sum(l => (decimal)(l.LaengeMeter ?? 0));

            double erledigtProzent = gesamtMeter > 0 ? (double)(erledigtMeter / gesamtMeter) * 100 : 0;
            double abgerechnetProzent = gesamtMeter > 0 ? (double)(abgerechnetMeter / gesamtMeter) * 100 : 0;

            var donutZeile = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            };
            donutZeile.Children.Add(ErzeugeDonut(erledigtProzent, "Erledigt", Success));
            donutZeile.Children.Add(ErzeugeDonut(abgerechnetProzent, "Abgerechnet", Accent));
            wurzel.Children.Add(donutZeile);

            var gruppen = leistungen
                .GroupBy(l => gruppierung?.Invoke(l.Leistungsbeschreibung) ?? l.Leistungsbeschreibung)
                .Select(g =>
                {
                    bool istStueck = g.All(l => (l.LaengeMeter ?? 0) == 0);
                    double gesamt = istStueck ? g.Count() : g.Sum(l => l.LaengeMeter ?? 0);
                    double erledigt = istStueck
                        ? g.Count(l => l.IstFertiggestellt)
                        : g.Where(l => l.IstFertiggestellt).Sum(l => l.LaengeMeter ?? 0);
                    return (Titel: g.Key, Gesamt: gesamt, Erledigt: erledigt, IstStueck: istStueck);
                });

            foreach (var gruppe in gruppen)
            {
                wurzel.Children.Add(ErzeugeFortschrittsZeile($"• {gruppe.Titel}", gruppe.Erledigt, gruppe.Gesamt, gruppe.IstStueck));
            }

            return wurzel;
        }
    }
}
