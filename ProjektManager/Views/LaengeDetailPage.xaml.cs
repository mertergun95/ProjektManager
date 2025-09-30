using ProjektManager.Models;
using ProjektManager.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Diagnostics;

namespace ProjektManager.Views
{
    public partial class LaengeDetailPage : UserControl
    {
        private MainWindow _main;
        private Laenge _laenge;
        private List<Leistung> _ausgewaehlteLeistungen = new List<Leistung>();

        public LaengeDetailPage(MainWindow main, Laenge laenge)
        {
            InitializeComponent();
            _main = main;
            _laenge = laenge;

            double kmVon = _laenge.Leistungen.FirstOrDefault()?.KmVon ?? 0;
            double kmBis = _laenge.Leistungen.LastOrDefault()?.KmBis ?? 0;
            LaengeTitel.Text = $"Länge: {laenge.Bezeichnung} (km {kmVon:F3} – {kmBis:F3})";

            ErzeugeLeistungsbloecke();
            ErzeugeNavigationButtons();
        }

        private void ErzeugeNavigationButtons()
        {
            var prevButton = new Button
            {
                Content = "←",
                Width = 40,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 60, 0, 0) // Offset için üstten 60px ekledim
            };
            prevButton.Click += (s, e) => WechselZuVorherigerLaenge();

            var nextButton = new Button
            {
                Content = "→",
                Width = 40,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 60, 10, 0) // Offset için üstten 60px ekledim
            };
            nextButton.Click += (s, e) => WechselZuNaechsterLaenge();

            // 🧠 Butonları sabit Grid.Row=0'a ekliyoruz
            Grid.SetRow(prevButton, 0);
            Grid.SetRow(nextButton, 0);

            LayoutRoot.Children.Add(prevButton);
            LayoutRoot.Children.Add(nextButton);
        }

        private void WechselZuVorherigerLaenge()
        {
            var projekt = _main.AktuellesProjekt;
            if (projekt == null) return;

            int index = projekt.Laengen.IndexOf(_laenge);
            if (index > 0)
            {
                var vorherige = projekt.Laengen[index - 1];
                _main.ZeigeSeite(new LaengeDetailPage(_main, vorherige));
            }
        }

        private void WechselZuNaechsterLaenge()
        {
            var projekt = _main.AktuellesProjekt;
            if (projekt == null) return;

            int index = projekt.Laengen.IndexOf(_laenge);
            if (index < projekt.Laengen.Count - 1)
            {
                var naechste = projekt.Laengen[index + 1];
                _main.ZeigeSeite(new LaengeDetailPage(_main, naechste));
            }
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

        private void SpeichereAktuellesProjekt()
        {
            ProjektSpeicher.Speichern((_main.DataContext as ViewModels.MainViewModel)?.Projekte?.ToList() ?? new());
        }

        private void ÜberprüfeUndAktualisiereLeistung(Leistung geaenderteLeistung)
        {
            var alleLeistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();
            var andereLeistungen = alleLeistungen.Where(l => l != geaenderteLeistung).ToList();

            // Länge sınırlarını sadece diğer bloklara göre belirle
            double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
            double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);


            // Hatalı giriş kontrolü
            if (geaenderteLeistung.KmVon < laengeMin || geaenderteLeistung.KmBis > laengeMax)
            {
                MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Güncellenmiş metraj
            geaenderteLeistung.LaengeMeter = (geaenderteLeistung.KmBis - geaenderteLeistung.KmVon) * 1000;

            // Çakışan blokları temizle veya daralt
            foreach (var block in andereLeistungen.ToList())
            {
                bool ueberschneidung = block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmVon;

                if (!ueberschneidung) continue;

                // Tamamen içine aldıysa sil
                if (block.KmVon >= geaenderteLeistung.KmVon && block.KmBis <= geaenderteLeistung.KmBis)
                {
                    _laenge.Leistungen.Remove(block);
                }
                else if (block.KmVon < geaenderteLeistung.KmVon && block.KmBis > geaenderteLeistung.KmVon)
                {
                    // Soldan kesildi
                    block.KmBis = geaenderteLeistung.KmVon;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
                else if (block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmBis)
                {
                    // Sağdan kesildi
                    block.KmVon = geaenderteLeistung.KmBis;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
            }

            // Listeyi güncelle
            var neuSortiert = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();
            _laenge.Leistungen = neuSortiert;

            // Boşluk kontrolü: önceki ile bu blok arasında boşluk varsa
            int index = _laenge.Leistungen.IndexOf(geaenderteLeistung);
            if (index > 0)
            {
                var vorheriger = _laenge.Leistungen[index - 1];
                if (Math.Abs(geaenderteLeistung.KmVon - vorheriger.KmBis) > 0.001)
                {
                    _laenge.Leistungen.Add(new Leistung
                    {
                        KmVon = vorheriger.KmBis,
                        KmBis = geaenderteLeistung.KmVon,
                        Leistungsbeschreibung = "Leer",
                        LaengeMeter = (geaenderteLeistung.KmVon - vorheriger.KmBis) * 1000
                    });
                }
            }

            // Boşluk kontrolü: bu blok ile sonraki arasında
            if (index < _laenge.Leistungen.Count - 1)
            {
                var nachfolgend = _laenge.Leistungen[index + 1];
                if (Math.Abs(nachfolgend.KmVon - geaenderteLeistung.KmBis) > 0.001)
                {
                    _laenge.Leistungen.Add(new Leistung
                    {
                        KmVon = geaenderteLeistung.KmBis,
                        KmBis = nachfolgend.KmVon,
                        Leistungsbeschreibung = "Leer",
                        LaengeMeter = (nachfolgend.KmVon - geaenderteLeistung.KmBis) * 1000
                    });
                }
            }

            // Listeyi tekrar sırala, kaydet ve görseli yenile
            _laenge.Leistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();
            SpeichereAktuellesProjekt();
            ErzeugeLeistungsbloecke();
        }

        private void ErzeugeLeistungsbloecke()
        {
            LeistungsBar.Children.Clear();
            KmBar.Children.Clear();

            var fontFamily = new FontFamily("Segoe UI");
            double fontSize = 12;
            double padding = 20;
            double margin = 3;

            var minWidths = new List<double>();
            var laengen = _laenge.Leistungen.AsEnumerable();

            if (CheckBoxNurErledigt != null && CheckBoxNurErledigt.IsChecked == true)
                laengen = laengen.Where(l => l.IstFertiggestellt);

            if (CheckBoxNurAbgerechnet != null && CheckBoxNurAbgerechnet.IsChecked == true)
                laengen = laengen.Where(l => l.IstAbgerechnet);

            // 💥 Burası ŞART
            var laengenList = laengen.ToList();




            foreach (var leistung in laengen)
            {
                leistung.LaengeMeter = (leistung.KmBis - leistung.KmVon) * 1000;
            }

            double toplamLaenge = laengen.Sum(l => l.LaengeMeter ?? 0);
            bool hepsiSifir = laengen.All(l => (l.LaengeMeter ?? 0) <= 0);

            foreach (var leistung in laengen)
            {
                var formattedText = new FormattedText(
                    leistung.Leistungsbeschreibung,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    fontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);

                double width = formattedText.Width + padding;
                minWidths.Add(width);
            }

            double toplamMinGenislik = minWidths.Sum();
            double kalanAlan = Math.Max(0, 1000 - toplamMinGenislik);
            double currentOffset = 0;

            var spacerStart = new Border { Width = 20, Height = 0, Background = Brushes.Transparent };
            LeistungsBar.Children.Add(spacerStart);

            for (int i = 0; i < laengenList.Count; i++)
            {
                var leistung = laengenList[i];
                double metraj = (leistung.KmBis - leistung.KmVon) * 1000;

                double ekAlan = (toplamLaenge > 0 && !hepsiSifir) ? (metraj / toplamLaenge) * kalanAlan : 0;
                double toplamGenislik = minWidths[i] + ekAlan;
                double disGenislik = toplamGenislik + margin * 2;

                var stackPanel = new StackPanel();

                var description = new TextBlock
                {
                    Text = leistung.Leistungsbeschreibung,
                    TextWrapping = TextWrapping.NoWrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stackPanel.Children.Add(description);

                if (metraj > 0)
                {
                    var meterText = new TextBlock
                    {
                        Text = $"{(int)metraj} m",
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    };
                    stackPanel.Children.Add(meterText);
                }

                var border = new Border
                {
                    Width = toplamGenislik,
                    Height = 60,
                    Background = leistung.IstFertiggestellt ? Brushes.LightGreen : Brushes.SkyBlue,
                    BorderBrush = _ausgewaehlteLeistungen.Contains(leistung) ? Brushes.OrangeRed : Brushes.Gray,
                    BorderThickness = new Thickness(_ausgewaehlteLeistungen.Contains(leistung) ? 2 : 1),
                    Margin = new Thickness(margin)
                };

                var tooltipPanel = new StackPanel();

                tooltipPanel.Children.Add(new TextBlock { Text = $"🚆 Bahnseite: {leistung.Bahnseite}" });
                tooltipPanel.Children.Add(new TextBlock { Text = $"📏 Länge: {(int)metraj} m" });
                tooltipPanel.Children.Add(new TextBlock { Text = $"🗒️ Anmerkung: {leistung.Anmerkung}", Foreground = Brushes.Gray, FontStyle = FontStyles.Italic });
                tooltipPanel.Children.Add(new TextBlock { Text = $"📌 Anmerkung2: {leistung.Anmerkung2}", Foreground = Brushes.Gray, FontStyle = FontStyles.Italic });

                if (!string.IsNullOrWhiteSpace(leistung.Notiz))
                {
                    tooltipPanel.Children.Add(new TextBlock { Text = $"📝 Notiz: {leistung.Notiz}", Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
                }

                border.ToolTip = tooltipPanel;

                border.MouseLeftButtonDown += (s, e) =>
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (_ausgewaehlteLeistungen.Contains(leistung))
                            _ausgewaehlteLeistungen.Remove(leistung);
                        else
                            _ausgewaehlteLeistungen.Add(leistung);
                    }
                    else
                    {
                        _ausgewaehlteLeistungen.Clear();
                        _ausgewaehlteLeistungen.Add(leistung);
                    }
                    ErzeugeLeistungsbloecke();
                };

                border.MouseRightButtonDown += (s, e) =>
                {
                    var contextMenu = new ContextMenu();

                    var bearbeitenItem = new MenuItem { Header = "✏️ Bearbeiten" };
                    bearbeitenItem.Click += (se, ev) =>
                    {
                        var dialog = new LeistungBearbeitenWindow(leistung);
                        if (dialog.ShowDialog() == true)
                        {
                            double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
                            double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);

                            if (leistung.KmVon < laengeMin || leistung.KmBis > laengeMax)
                            {
                                MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            ÜberprüfeUndAktualisiereLeistung(leistung);
                        }
                    };
                    contextMenu.Items.Add(bearbeitenItem);

                    var notizItem = new MenuItem { Header = "📝 Notiz hinzufügen / bearbeiten" };
                    notizItem.Click += (se, ev) =>
                    {
                        var dialog = new NotizEingabeWindow(leistung.Notiz);
                        if (dialog.ShowDialog() == true)
                        {
                            leistung.Notiz = dialog.EingetrageneNotiz;
                            SpeichereAktuellesProjekt();
                            ErzeugeLeistungsbloecke();
                        }
                    };
                    contextMenu.Items.Add(notizItem);

                    var aktuelleAuswahl = _ausgewaehlteLeistungen.Contains(leistung)
                        ? _ausgewaehlteLeistungen
                        : new List<Leistung> { leistung };

                    if (aktuelleAuswahl.Count >= 1)
                    {
                        var erledigtMulti = new MenuItem { Header = "✅/❌ Als erledigt umschalten" };
                        erledigtMulti.Click += (se, ev) =>
                        {
                            foreach (var l in aktuelleAuswahl)
                                l.IstFertiggestellt = !l.IstFertiggestellt;

                            SpeichereAktuellesProjekt();
                            ErzeugeLeistungsbloecke();
                        };
                        contextMenu.Items.Add(erledigtMulti);

                        var abgerechnetMulti = new MenuItem { Header = "💰/🚫 Abrechnung umschalten" };
                        abgerechnetMulti.Click += (se, ev) =>
                        {
                            foreach (var l in aktuelleAuswahl)
                                l.IstAbgerechnet = !l.IstAbgerechnet;

                            SpeichereAktuellesProjekt();
                            ErzeugeLeistungsbloecke();
                        };
                        contextMenu.Items.Add(abgerechnetMulti);
                    }

                    border.ContextMenu = contextMenu;
                };

                var container = new Grid();
                container.Children.Add(stackPanel);

                if (leistung.IstAbgerechnet)
                {
                    var badge = new Ellipse
                    {
                        Width = 22,
                        Height = 22,
                        Fill = Brushes.Red,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 1,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 4, 4)
                    };

                    var badgeText = new TextBlock
                    {
                        Text = "€",
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 8, 8)
                    };

                    container.Children.Add(badge);
                    container.Children.Add(badgeText);
                }

                if (!string.IsNullOrWhiteSpace(leistung.Notiz))
                {
                    var notizText = new TextBlock
                    {
                        Text = "❗",
                        Foreground = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(6, 0, 0, 4)
                    };

                    container.Children.Add(notizText);
                }

                border.Child = container;

                LeistungsBar.Children.Add(border);

                if (leistung == laengen.FirstOrDefault())
                {
                    var kmVonBlock = ErzeugeKmBlock($"{leistung.KmVon:F3} km");
                    Canvas.SetLeft(kmVonBlock, currentOffset + margin + 15);
                    KmBar.Children.Add(kmVonBlock);
                }

                var kmBisBlock = ErzeugeKmBlock($"{leistung.KmBis:F3} km");
                Canvas.SetLeft(kmBisBlock, currentOffset + disGenislik + margin + 15);
                KmBar.Children.Add(kmBisBlock);

                currentOffset += disGenislik;
            }

            var spacerEnd = new Border { Width = 20, Height = 0, Background = Brushes.Transparent };
            LeistungsBar.Children.Add(spacerEnd);

            KmBar.Width = currentOffset + 40;
            LeistungsBar.Width = currentOffset + 40;

            ErzeugeZusammenfassung();
        }

        private class LeistungsZusammenfassung
        {
            private Laenge _laenge;

            public LeistungsZusammenfassung(Laenge laenge)
            {
                _laenge = laenge;
            }

            public string Beschreibung { get; set; }
            public double Gesamt { get; set; }
            public double Erledigt { get; set; }
            public bool IstStueck => Gesamt == 0;

            public string AnzeigeText
            {
                get
                {
                    if (IstStueck)
                    {
                        var gesamtStueck = _laenge.Leistungen
                            .Where(l => l.Leistungsbeschreibung == Beschreibung && (l.LaengeMeter ?? 0) == 0)
                            .Count();

                        return gesamtStueck == 0 ? "— Stück" : $"{Erledigt} / {gesamtStueck} Stück";
                    }
                    return $"{Erledigt:F0} m / {Gesamt:F0} m";
                }
            }
        }

        private void ErzeugeZusammenfassung()
        {
            ZusammenfassungPanel.Children.Clear();

            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 500, // Sabit genişlik
                Margin = new Thickness(0, 10, 0, 20)
            };

            // Donutlar için toplam metre hesaplamaları
            decimal gesamtMeter = _laenge.Leistungen.Sum(l => (decimal)(l.LaengeMeter ?? 0));
            decimal erledigtMeter = _laenge.Leistungen.Where(l => l.IstFertiggestellt).Sum(l => (decimal)(l.LaengeMeter ?? 0));
            decimal abgerechnetMeter = _laenge.Leistungen.Where(l => l.IstAbgerechnet).Sum(l => (decimal)(l.LaengeMeter ?? 0));

            // Yüzde hesaplamaları
            double erledigtProzent = gesamtMeter > 0 ? (double)(erledigtMeter / gesamtMeter) * 100 : 0;
            double abgerechnetProzent = gesamtMeter > 0 ? (double)(abgerechnetMeter / gesamtMeter) * 100 : 0;

            // Debugging - Yüzdeleri kontrol et
            Debug.WriteLine($"Erledigt Meter: {erledigtMeter}");
            Debug.WriteLine($"Abgerechnet Meter: {abgerechnetMeter}");
            Debug.WriteLine($"Erledigt Prozent: {erledigtProzent}");
            Debug.WriteLine($"Abgerechnet Prozent: {abgerechnetProzent}");


            var donutContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            donutContainer.Children.Add(ErzeugeDonut(erledigtProzent, "Erledigt", Brushes.SeaGreen));
            donutContainer.Children.Add(ErzeugeDonut(abgerechnetProzent, "Abgerechnet", Brushes.SteelBlue));

            // Özet ekranına ekliyoruz
            ZusammenfassungPanel.Children.Add(donutContainer);

            // 1. Kabelverlegung kutusu
            var kabelBox = new CheckBox
            {
                Content = "Kabel verlegt",
                IsChecked = _laenge.KabelVerlegt,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            kabelBox.Checked += (s, e) => { _laenge.KabelVerlegt = true; SpeichereAktuellesProjekt(); };
            kabelBox.Unchecked += (s, e) => { _laenge.KabelVerlegt = false; SpeichereAktuellesProjekt(); };
            ZusammenfassungPanel.Children.Add(kabelBox);

            // 2. Gruplandırma
            var gruppen = _laenge.Leistungen
                .GroupBy(l => l.Leistungsbeschreibung)
                .Select(g =>
                {
                    var istStueck = g.All(l => (l.LaengeMeter ?? 0) == 0);

                    double gesamt = istStueck
                        ? g.Count()
                        : g.Sum(l => l.LaengeMeter ?? 0);

                    double erledigt = istStueck
                        ? g.Count(l => l.IstFertiggestellt)
                        : g.Where(l => l.IstFertiggestellt).Sum(l => l.LaengeMeter ?? 0);

                    return new LeistungsZusammenfassung(_laenge)
                    {
                        Beschreibung = g.Key,
                        Gesamt = gesamt,
                        Erledigt = erledigt
                    };

                })
                .ToList();

            foreach (var item in gruppen)
            {
                var titel = new TextBlock
                {
                    Text = $"• {item.Beschreibung}",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 2)
                };
                ZusammenfassungPanel.Children.Add(titel);

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
                    Text = item.AnzeigeText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };

                grid.Children.Add(border);
                grid.Children.Add(pb);
                grid.Children.Add(text);
                ZusammenfassungPanel.Children.Add(grid);
            }
        }

        private StackPanel ErzeugeDonut(double prozent, string titel, Brush farbe)
        {
            prozent = Math.Round(prozent, 1);
            prozent = prozent > 99.9 ? 99.9 : prozent;

            double angle = prozent * 360.0 / 100.0;
            double radius = 50;
            double center = radius + 5;

            // Başlangıç noktası (üst orta)
            Point startPoint = new Point(center, center - radius);

            // Yayın bitiş noktası
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


        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ErzeugeLeistungsbloecke(); // filtreyi yeniden uygula
        }

        private Point BerechneDonutPunkt(double prozent)
        {
            double winkel = 360 * (prozent / 100);
            double radians = (winkel - 90) * Math.PI / 180;
            double x = 50 + 50 * Math.Cos(radians);
            double y = 50 + 50 * Math.Sin(radians);
            return new Point(x, y);
        }


    }
}
