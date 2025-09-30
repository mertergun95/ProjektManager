using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjektManager.Models;
using System.Linq;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Input;
using Newtonsoft.Json;
using ProjektManager.Helpers;
using System.IO;



namespace ProjektManager.Views
{
    public partial class ProjektUebersichtPage : UserControl
    {
        private MainWindow _main;
        private List<Projekt> _projekte;

        public ProjektUebersichtPage(MainWindow main, List<Projekt> projekte)
        {
            InitializeComponent();
            _main = main;
            _projekte = projekte;

            // LWL paneli üst kısma dinamik ekleniyor
            var lwlScroll = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Content = lwlPanel
            };

            // Grid’in üstüne (ilk sıraya) ekleyelim
            Grid.SetRow(lwlScroll, 0);
            ((Grid)this.Content).Children.Add(lwlScroll);

            ErzeugeProjektKarten();
            LadeLSTProjekte(); // 📊 LST projeleri yükle
            LadeLWLProjekte(); // 📦 LWL projeleri yükle
        }

        private void ErzeugeProjektKarten()
        {
            ProjektCardPanel.Children.Clear();

            foreach (var projekt in _projekte)
            {
                var card = ErzeugeProjektCard(projekt);
                ProjektCardPanel.Children.Add(card);
            }
        }

        public Projekt AusgewaehltesProjekt { get; private set; }

        private StackPanel lwlPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Left
        };


        private Border ErzeugeProjektCard(Projekt projekt)
        {
            double gesamteMeter = projekt.Laengen.Sum(l => l.Leistungen.Sum(x => x.LaengeMeter ?? 0));
            double erledigtMeter = projekt.Laengen.Sum(l => l.Leistungen.Where(x => x.IstFertiggestellt).Sum(x => x.LaengeMeter ?? 0));
            double abgerechnetMeter = projekt.Laengen.Sum(l => l.Leistungen.Where(x => x.IstAbgerechnet).Sum(x => x.LaengeMeter ?? 0));

            double erledigtProzent = gesamteMeter > 0 ? (erledigtMeter / gesamteMeter) * 100 : 0;
            double abgerechnetProzent = gesamteMeter > 0 ? (abgerechnetMeter / gesamteMeter) * 100 : 0;

            var titel = new TextBlock
            {
                Text = projekt.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var laengeInfo = new TextBlock
            {
                Text = $"{projekt.Laengen.Count} Länge(n)",
                FontSize = 12,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var donuts = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            donuts.Children.Add(ErzeugeMiniDonut(erledigtProzent, Brushes.SeaGreen));
            donuts.Children.Add(ErzeugeMiniDonut(abgerechnetProzent, Brushes.SteelBlue));

            var content = new StackPanel();
            content.Children.Add(titel);
            content.Children.Add(laengeInfo);
            content.Children.Add(donuts);

            Border card = new Border
            {
                Background = Brushes.White,
                BorderBrush = (projekt == AusgewaehltesProjekt) ? Brushes.OrangeRed : Brushes.Gray,
                BorderThickness = new Thickness((projekt == AusgewaehltesProjekt) ? 2 : 1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10),
                Padding = new Thickness(10),
                Width = 280,
                Child = content,
                Cursor = Cursors.Hand
            };

            card.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    _main.ZeigeSeite(new ProjektVisualisierungPage(_main, projekt));
                    return;
                }

                // Tek tıklama: seçimi ayarla
                AusgewaehltesProjekt = projekt;
                _main.AktuellesProjekt = projekt;
                ErzeugeProjektKarten();
                _main.UpdateProjektBearbeitenUndLoeschenButtons();
            };

            return card;
        }

        private void LadeLSTProjekte()
        {
            string indexPfad = ProjektPfadHelper.LST_IndexDatei;
            List<string> projekte = new();
            bool indexAusLegacy = false;

            if (File.Exists(indexPfad))
            {
                projekte = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(indexPfad)) ?? new();
            }

            if (projekte.Count == 0 && File.Exists(ProjektPfadHelper.LegacyLSTIndexDatei))
            {
                projekte = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ProjektPfadHelper.LegacyLSTIndexDatei)) ?? new();
                indexAusLegacy = projekte.Count > 0;
            }

            var eindeutigeNamen = projekte.Distinct().ToList();
            bool dateienAusLegacy = false;

            foreach (var name in eindeutigeNamen)
            {
                string jsonPfad = Path.Combine(ProjektPfadHelper.LST_Projekte_Ordner, name + ".json");
                List<LSTKabel>? kabel = null;

                if (File.Exists(jsonPfad))
                {
                    kabel = JsonConvert.DeserializeObject<List<LSTKabel>>(File.ReadAllText(jsonPfad));
                }
                else
                {
                    string legacyPfad = ProjektPfadHelper.LegacyProjektDatei("LST_Projekte", name);
                    if (File.Exists(legacyPfad))
                    {
                        var json = File.ReadAllText(legacyPfad);
                        kabel = JsonConvert.DeserializeObject<List<LSTKabel>>(json);
                        if (kabel != null)
                        {
                            Directory.CreateDirectory(ProjektPfadHelper.LSTProjektOrdner);
                            File.WriteAllText(jsonPfad, json);
                            ProjektPfadHelper.TryDeleteLegacyFile(legacyPfad);
                            dateienAusLegacy = true;
                        }
                    }
                }

                if (kabel == null)
                    continue;

                var lstPage = new LSTVisualisierungPage(kabel);
                Button btn = new Button
                {
                    Content = "📊 LST: " + name,
                    Margin = new Thickness(5),
                    Tag = kabel
                };

                btn.Click += (s, e) =>
                {
                    _main.ZeigeSeite(lstPage);
                };

                LSTProjektPanel.Children.Add(btn); // StackPanel
            }

            if ((indexAusLegacy || dateienAusLegacy) && eindeutigeNamen.Count > 0)
            {
                Directory.CreateDirectory(ProjektPfadHelper.LSTProjektOrdner);
                File.WriteAllText(ProjektPfadHelper.LSTIndexDatei,
                    JsonConvert.SerializeObject(eindeutigeNamen, Formatting.Indented));
                ProjektPfadHelper.TryDeleteLegacyFile(ProjektPfadHelper.LegacyLSTIndexDatei);
            }
        }


        private void LadeLWLProjekte()
        {
            string indexPfad = ProjektPfadHelper.LWLIndexDatei;
            List<string> projekte = new();
            bool indexAusLegacy = false;

            if (File.Exists(indexPfad))
            {
                projekte = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(indexPfad)) ?? new();
            }

            if (projekte.Count == 0 && File.Exists(ProjektPfadHelper.LegacyLWLIndexDatei))
            {
                projekte = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ProjektPfadHelper.LegacyLWLIndexDatei)) ?? new();
                indexAusLegacy = projekte.Count > 0;
            }

            var eindeutigeNamen = projekte.Distinct().ToList();
            bool dateienAusLegacy = false;

            foreach (var name in eindeutigeNamen)
            {
                string pfad = Path.Combine(ProjektPfadHelper.LWLProjektOrdner, name + ".json");
                Projekt? projekt = null;

                if (File.Exists(pfad))
                {
                    projekt = JsonConvert.DeserializeObject<Projekt>(File.ReadAllText(pfad));
                }
                else
                {
                    string legacyPfad = ProjektPfadHelper.LegacyProjektDatei("LWL_Projekte", name);
                    if (File.Exists(legacyPfad))
                    {
                        var json = File.ReadAllText(legacyPfad);
                        projekt = JsonConvert.DeserializeObject<Projekt>(json);
                        if (projekt != null)
                        {
                            Directory.CreateDirectory(ProjektPfadHelper.LWLProjektOrdner);
                            File.WriteAllText(pfad, json);
                            ProjektPfadHelper.TryDeleteLegacyFile(legacyPfad);
                            dateienAusLegacy = true;
                        }
                    }
                }

                if (projekt == null)
                    continue;

                var lokalesProjekt = projekt;
                Button btn = new Button
                {
                    Content = "📦 LWL: " + name,
                    Margin = new Thickness(5),
                    Tag = lokalesProjekt
                };

                btn.Click += (s, e) =>
                {
                    _main.ZeigeSeite(new ProjektVisualisierungPage(_main, lokalesProjekt));
                };

                lwlPanel.Children.Add(btn);
            }

            if ((indexAusLegacy || dateienAusLegacy) && eindeutigeNamen.Count > 0)
            {
                Directory.CreateDirectory(ProjektPfadHelper.LWLProjektOrdner);
                File.WriteAllText(ProjektPfadHelper.LWLIndexDatei,
                    JsonConvert.SerializeObject(eindeutigeNamen, Formatting.Indented));
                ProjektPfadHelper.TryDeleteLegacyFile(ProjektPfadHelper.LegacyLWLIndexDatei);
            }
        }




        private Grid ErzeugeMiniDonut(double prozent, Brush farbe)
        {
            double angle = prozent * 360.0 / 100.0;
            double radius = 25;
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

            var path = new Path
            {
                Stroke = farbe,
                StrokeThickness = 8,
                Data = geometry,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = radius * 2 + 10,
                Height = radius * 2 + 10
            };

            var background = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.LightGray,
                StrokeThickness = 8,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = $"{prozent:F0} %",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var grid = new Grid
            {
                Width = radius * 2 + 10,
                Height = radius * 2 + 10,
                Margin = new Thickness(5)
            };

            grid.Children.Add(background);
            grid.Children.Add(path);
            grid.Children.Add(label);

            return grid;
        }
    }
}
