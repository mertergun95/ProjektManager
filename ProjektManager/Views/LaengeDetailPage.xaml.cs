using ProjektManager.Data;
using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.Views
{
    public partial class LaengeDetailPage : UserControl
    {
        private readonly MainWindow _main;
        private readonly Laenge _laenge;
        private readonly List<Leistung> _ausgewaehlteLeistungen = new();

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
            var iconStyle = (Style)Application.Current.Resources["IconButtonStyle"];

            var prevButton = new Button
            {
                Content = "←",
                Style = iconStyle,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 6, 0, 0)
            };
            prevButton.Click += (s, e) => WechselZuLaenge(-1);

            var nextButton = new Button
            {
                Content = "→",
                Style = iconStyle,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 6, 0, 0)
            };
            nextButton.Click += (s, e) => WechselZuLaenge(1);

            Grid.SetRow(prevButton, 1);
            Grid.SetRow(nextButton, 1);

            LayoutRoot.Children.Add(prevButton);
            LayoutRoot.Children.Add(nextButton);
        }

        private void WechselZuLaenge(int richtung)
        {
            var projekt = _main.AktuellesProjekt;
            if (projekt == null) return;

            int index = projekt.Laengen.IndexOf(_laenge);
            int neuerIndex = index + richtung;
            if (index < 0 || neuerIndex < 0 || neuerIndex >= projekt.Laengen.Count) return;

            _main.ZeigeSeite(new LaengeDetailPage(_main, projekt.Laengen[neuerIndex]));
        }

        private void SpeichereAktuellesProjekt()
        {
            ProjektSpeicher.Speichern(_main.AlleProjekte);
        }

        private void ÜberprüfeUndAktualisiereLeistung(Leistung geaenderteLeistung)
        {
            var andereLeistungen = _laenge.Leistungen.Where(l => l != geaenderteLeistung).ToList();

            double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
            double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);

            if (geaenderteLeistung.KmVon < laengeMin || geaenderteLeistung.KmBis > laengeMax)
            {
                MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            geaenderteLeistung.LaengeMeter = (geaenderteLeistung.KmBis - geaenderteLeistung.KmVon) * 1000;

            foreach (var block in andereLeistungen.ToList())
            {
                bool ueberschneidung = block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmVon;
                if (!ueberschneidung) continue;

                if (block.KmVon >= geaenderteLeistung.KmVon && block.KmBis <= geaenderteLeistung.KmBis)
                {
                    _laenge.Leistungen.Remove(block);
                }
                else if (block.KmVon < geaenderteLeistung.KmVon && block.KmBis > geaenderteLeistung.KmVon)
                {
                    block.KmBis = geaenderteLeistung.KmVon;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
                else if (block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmBis)
                {
                    block.KmVon = geaenderteLeistung.KmBis;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
            }

            _laenge.Leistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();

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

            _laenge.Leistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();
            SpeichereAktuellesProjekt();
            ErzeugeLeistungsbloecke();
        }

        private void ErzeugeLeistungsbloecke()
        {
            LeistungsBar.Children.Clear();
            KmBar.Children.Clear();

            const double fontSize = 12;
            const double padding = 20;
            const double margin = 3;

            IEnumerable<Leistung> leistungen = _laenge.Leistungen;

            if (CheckBoxNurErledigt.IsChecked == true)
                leistungen = leistungen.Where(l => l.IstFertiggestellt);

            if (CheckBoxNurAbgerechnet.IsChecked == true)
                leistungen = leistungen.Where(l => l.IstAbgerechnet);

            var leistungenListe = leistungen.ToList();

            foreach (var leistung in leistungenListe)
                leistung.LaengeMeter = (leistung.KmBis - leistung.KmVon) * 1000;

            var minWidths = leistungenListe
                .Select(l => new FormattedText(
                    l.Leistungsbeschreibung,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1).Width + padding)
                .ToList();

            double toplamLaenge = leistungenListe.Sum(l => l.LaengeMeter ?? 0);
            bool hepsiSifir = leistungenListe.All(l => (l.LaengeMeter ?? 0) <= 0);

            double toplamMinGenislik = minWidths.Sum();
            double kalanAlan = Math.Max(0, 1000 - toplamMinGenislik);
            double currentOffset = 0;

            LeistungsBar.Children.Add(new Border { Width = 20, Height = 0, Background = Brushes.Transparent });

            for (int i = 0; i < leistungenListe.Count; i++)
            {
                var leistung = leistungenListe[i];
                double metraj = (leistung.KmBis - leistung.KmVon) * 1000;

                double ekAlan = (toplamLaenge > 0 && !hepsiSifir) ? (metraj / toplamLaenge) * kalanAlan : 0;
                double toplamGenislik = minWidths[i] + ekAlan;
                double disGenislik = toplamGenislik + margin * 2;

                var border = ErzeugeLeistungsBlock(leistung, toplamGenislik, margin, metraj);
                LeistungsBar.Children.Add(border);

                if (i == 0)
                {
                    var kmVonBlock = VisualBuilder.ErzeugeKmMarker($"{leistung.KmVon:F3} km");
                    Canvas.SetLeft(kmVonBlock, currentOffset + margin + 15);
                    KmBar.Children.Add(kmVonBlock);
                }

                var kmBisBlock = VisualBuilder.ErzeugeKmMarker($"{leistung.KmBis:F3} km");
                Canvas.SetLeft(kmBisBlock, currentOffset + disGenislik + margin + 15);
                KmBar.Children.Add(kmBisBlock);

                currentOffset += disGenislik;
            }

            LeistungsBar.Children.Add(new Border { Width = 20, Height = 0, Background = Brushes.Transparent });

            KmBar.Width = currentOffset + 40;
            LeistungsBar.Width = currentOffset + 40;

            ErzeugeZusammenfassung();
        }

        private Border ErzeugeLeistungsBlock(Leistung leistung, double breite, double margin, double metraj)
        {
            var beschreibung = new TextBlock
            {
                Text = leistung.Leistungsbeschreibung,
                TextWrapping = TextWrapping.NoWrap,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(beschreibung);

            if (metraj > 0)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"{(int)metraj} m",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            bool istAusgewaehlt = _ausgewaehlteLeistungen.Contains(leistung);

            var border = new Border
            {
                Width = breite,
                Height = 60,
                CornerRadius = new CornerRadius(8),
                Background = leistung.IstFertiggestellt ? VisualBuilder.Success : VisualBuilder.Accent,
                BorderBrush = istAusgewaehlt ? VisualBuilder.Danger : Brushes.Transparent,
                BorderThickness = new Thickness(istAusgewaehlt ? 2 : 0),
                Margin = new Thickness(margin),
                Cursor = Cursors.Hand,
                ToolTip = ErzeugeTooltip(leistung, metraj)
            };

            var container = new Grid();
            container.Children.Add(stackPanel);

            if (leistung.IstAbgerechnet)
            {
                container.Children.Add(new Border
                {
                    Width = 20,
                    Height = 20,
                    CornerRadius = new CornerRadius(10),
                    Background = VisualBuilder.Danger,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 4, 4),
                    Child = new TextBlock
                    {
                        Text = "€",
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = 11,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(leistung.Notiz))
            {
                container.Children.Add(new TextBlock
                {
                    Text = "!",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(8, 0, 0, 2)
                });
            }

            border.Child = container;

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

            border.ContextMenu = ErzeugeKontextMenu(leistung, border);

            return border;
        }

        private static ToolTip ErzeugeTooltip(Leistung leistung, double metraj)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = $"Bahnseite: {leistung.Bahnseite}" });
            panel.Children.Add(new TextBlock { Text = $"Länge: {(int)metraj} m" });

            if (!string.IsNullOrWhiteSpace(leistung.Anmerkung))
                panel.Children.Add(new TextBlock { Text = $"Anmerkung: {leistung.Anmerkung}", Foreground = Brushes.Gray, FontStyle = FontStyles.Italic });

            if (!string.IsNullOrWhiteSpace(leistung.Anmerkung2))
                panel.Children.Add(new TextBlock { Text = $"Anmerkung: {leistung.Anmerkung2}", Foreground = Brushes.Gray, FontStyle = FontStyles.Italic });

            if (!string.IsNullOrWhiteSpace(leistung.Notiz))
                panel.Children.Add(new TextBlock { Text = $"Notiz: {leistung.Notiz}", Foreground = Brushes.Red, FontWeight = FontWeights.Bold });

            return new ToolTip { Content = panel };
        }

        private ContextMenu ErzeugeKontextMenu(Leistung leistung, Border border)
        {
            var contextMenu = new ContextMenu();

            var bearbeitenItem = new MenuItem { Header = "Bearbeiten" };
            bearbeitenItem.Click += (se, ev) =>
            {
                var dialog = new LeistungBearbeitenWindow(leistung) { Owner = Window.GetWindow(this) };
                if (dialog.ShowDialog() != true) return;

                double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
                double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);

                if (leistung.KmVon < laengeMin || leistung.KmBis > laengeMax)
                {
                    MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ÜberprüfeUndAktualisiereLeistung(leistung);
            };
            contextMenu.Items.Add(bearbeitenItem);

            var notizItem = new MenuItem { Header = "Notiz hinzufügen / bearbeiten" };
            notizItem.Click += (se, ev) =>
            {
                var dialog = new NotizEingabeWindow(leistung.Notiz) { Owner = Window.GetWindow(this) };
                if (dialog.ShowDialog() != true) return;

                leistung.Notiz = dialog.EingetrageneNotiz;
                SpeichereAktuellesProjekt();
                ErzeugeLeistungsbloecke();
            };
            contextMenu.Items.Add(notizItem);

            var auswahl = _ausgewaehlteLeistungen.Contains(leistung) ? _ausgewaehlteLeistungen : new List<Leistung> { leistung };

            var erledigtItem = new MenuItem { Header = "Als erledigt umschalten" };
            erledigtItem.Click += (se, ev) =>
            {
                foreach (var l in auswahl) l.IstFertiggestellt = !l.IstFertiggestellt;
                SpeichereAktuellesProjekt();
                ErzeugeLeistungsbloecke();
            };
            contextMenu.Items.Add(erledigtItem);

            var abgerechnetItem = new MenuItem { Header = "Abrechnung umschalten" };
            abgerechnetItem.Click += (se, ev) =>
            {
                foreach (var l in auswahl) l.IstAbgerechnet = !l.IstAbgerechnet;
                SpeichereAktuellesProjekt();
                ErzeugeLeistungsbloecke();
            };
            contextMenu.Items.Add(abgerechnetItem);

            return contextMenu;
        }

        private void ErzeugeZusammenfassung()
        {
            ZusammenfassungPanel.Children.Clear();

            var kabelBox = new CheckBox
            {
                Content = "Kabel verlegt",
                IsChecked = _laenge.KabelVerlegt,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            };
            kabelBox.Checked += (s, e) => { _laenge.KabelVerlegt = true; SpeichereAktuellesProjekt(); };
            kabelBox.Unchecked += (s, e) => { _laenge.KabelVerlegt = false; SpeichereAktuellesProjekt(); };
            ZusammenfassungPanel.Children.Add(kabelBox);

            ZusammenfassungPanel.Children.Add(VisualBuilder.ErzeugeZusammenfassung(_laenge.Leistungen));
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ErzeugeLeistungsbloecke();
        }
    }
}
