using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ProjektManager.Helpers
{
    /// <summary>
    /// Erstellt einen gestalteten, druckfähigen Projektbericht im Querformat: eine Kopfzeile,
    /// darunter eine zweispaltige "Dashboard"-Zeile (Kennzahlen-Karten links, Fortschritt nach
    /// Leistungsart mit echten Balken rechts) – das nutzt die Breite des Querformats sinnvoll aus
    /// statt nur ein Hochformat-Layout zu strecken – und darunter je Länge eine breite,
    /// übersichtliche Tabelle. Zeigt den Windows-Druckdialog an; über "Microsoft Print to PDF"
    /// lässt sich der Bericht so auch ohne zusätzliche PDF-Bibliothek als Datei speichern.
    /// </summary>
    public static class BerichtDrucker
    {
        private static readonly Brush TextDunkel = new SolidColorBrush(Color.FromRgb(0x1E, 0x24, 0x30));
        private static readonly Brush TextGrau = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
        private static readonly Brush Akzent = new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
        private static readonly Brush Erfolg = new SolidColorBrush(Color.FromRgb(0x1E, 0x8E, 0x5A));
        private static readonly Brush LinieHell = new SolidColorBrush(Color.FromRgb(0xE1, 0xE4, 0xEA));
        private static readonly Brush ZeileWechsel = new SolidColorBrush(Color.FromRgb(0xF7, 0xF8, 0xFA));
        private static readonly Brush TrackHell = new SolidColorBrush(Color.FromRgb(0xEC, 0xEE, 0xF1));

        public static void DruckeProjektBericht(Projekt projekt)
        {
            var printDialog = new PrintDialog();

            // Der Bericht ist für Querformat gestaltet (breite Dashboard-Zeile, luftige Tabellen);
            // das wird als Vorauswahl im Druckdialog gesetzt, der Benutzer kann es dort noch ändern.
            try { printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape; }
            catch { /* falls der Drucker Querformat nicht unterstützt, Standard belassen */ }

            if (printDialog.ShowDialog() != true) return;

            var dokument = ErzeugeDokument(projekt);
            IDocumentPaginatorSource quelle = dokument;
            quelle.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            printDialog.PrintDocument(quelle.DocumentPaginator, $"Projektbericht - {projekt.Name}");
        }

        private static FlowDocument ErzeugeDokument(Projekt projekt)
        {
            var dokument = new FlowDocument
            {
                PagePadding = new Thickness(48, 34, 48, 34),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                Foreground = TextDunkel
            };

            dokument.Blocks.Add(ErzeugeKopfbereich(projekt));

            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            dokument.Blocks.Add(ErzeugeDashboardZeile(alleLeistungen));

            foreach (var laenge in projekt.Laengen)
            {
                dokument.Blocks.AddRange(ErzeugeLaengenAbschnitt(laenge));
            }

            return dokument;
        }

        private static Block ErzeugeKopfbereich(Projekt projekt)
        {
            var kopf = new Grid();
            kopf.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            kopf.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var linksStack = new StackPanel();
            linksStack.Children.Add(new TextBlock { Text = "PROJEKTBERICHT", FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = Akzent, Margin = new Thickness(0, 0, 0, 2) });
            linksStack.Children.Add(new TextBlock { Text = projekt.Name, FontSize = 26, FontWeight = FontWeights.Bold, Foreground = TextDunkel });
            Grid.SetColumn(linksStack, 0);

            var datumText = new TextBlock
            {
                Text = $"Erstellt am {DateTime.Now:dd.MM.yyyy}\num {DateTime.Now:HH:mm} Uhr",
                FontSize = 10,
                Foreground = TextGrau,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Grid.SetColumn(datumText, 1);

            kopf.Children.Add(linksStack);
            kopf.Children.Add(datumText);

            var wrapper = new StackPanel();
            wrapper.Children.Add(kopf);
            wrapper.Children.Add(new Border { Height = 2, Background = Akzent, Margin = new Thickness(0, 12, 0, 20) });

            return new BlockUIContainer(wrapper);
        }

        /// <summary>
        /// Zweispaltige Dashboard-Zeile für die Breite des Querformats: links drei gestapelte
        /// Kennzahlen-Karten, rechts der Fortschritt je Leistungsart mit echten Balken.
        /// </summary>
        private static Block ErzeugeDashboardZeile(List<Leistung> alleLeistungen)
        {
            var raster = new Grid();
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) });

            var kennzahlen = ErzeugeKennzahlenSpalte(alleLeistungen);
            Grid.SetColumn(kennzahlen, 0);

            var gruppen = ErzeugeGruppenKarte(alleLeistungen);
            Grid.SetColumn(gruppen, 2);

            raster.Children.Add(kennzahlen);
            raster.Children.Add(gruppen);

            return new BlockUIContainer(new Border { Margin = new Thickness(0, 0, 0, 26), Child = raster });
        }

        private static Border ErzeugeKennzahlenSpalte(List<Leistung> alleLeistungen)
        {
            var (gesamtMeter, erledigtMeter, abgerechnetMeter, erledigtProzent, abgerechnetProzent) = VisualBuilder.BerechneGesamt(alleLeistungen);

            var stack = new StackPanel();
            stack.Children.Add(ErzeugeKennzahlKarte("Gesamtlänge", $"{gesamtMeter:N0} m", null, TextDunkel, istErsteKarte: true));
            stack.Children.Add(ErzeugeKennzahlKarte("Erledigt", $"{erledigtProzent:F0} %", $"{erledigtMeter:N0} m", Erfolg, istErsteKarte: false));
            stack.Children.Add(ErzeugeKennzahlKarte("Abgerechnet", $"{abgerechnetProzent:F0} %", $"{abgerechnetMeter:N0} m", Akzent, istErsteKarte: false));

            return new Border { BorderBrush = LinieHell, BorderThickness = new Thickness(1), Padding = new Thickness(18, 4, 18, 4), Child = stack };
        }

        private static Border ErzeugeKennzahlKarte(string label, string wert, string? unterzeile, Brush wertFarbe, bool istErsteKarte)
        {
            var zeile = new Grid { Margin = new Thickness(0, 14, 0, 14) };
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock { Text = label.ToUpperInvariant(), FontSize = 10.5, FontWeight = FontWeights.SemiBold, Foreground = TextGrau, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(labelText, 0);

            var wertStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
            wertStack.Children.Add(new TextBlock { Text = wert, FontSize = 22, FontWeight = FontWeights.Bold, Foreground = wertFarbe, HorizontalAlignment = HorizontalAlignment.Right });
            if (unterzeile != null)
                wertStack.Children.Add(new TextBlock { Text = unterzeile, FontSize = 9.5, Foreground = TextGrau, HorizontalAlignment = HorizontalAlignment.Right });
            Grid.SetColumn(wertStack, 1);

            zeile.Children.Add(labelText);
            zeile.Children.Add(wertStack);

            return new Border { BorderBrush = LinieHell, BorderThickness = new Thickness(0, istErsteKarte ? 0 : 1, 0, 0), Child = zeile };
        }

        private static Border ErzeugeGruppenKarte(List<Leistung> alleLeistungen)
        {
            var inhalt = new StackPanel();
            inhalt.Children.Add(new TextBlock { Text = "FORTSCHRITT NACH LEISTUNGSART", FontSize = 10.5, FontWeight = FontWeights.SemiBold, Foreground = TextGrau, Margin = new Thickness(0, 0, 0, 10) });

            foreach (var gruppe in VisualBuilder.BerechneGruppen(alleLeistungen).OrderByDescending(g => g.Gesamt))
            {
                double prozent = gruppe.Gesamt > 0 ? gruppe.Erledigt / gruppe.Gesamt * 100 : 0;
                string einheit = gruppe.IstStueck ? "Stück" : "m";
                inhalt.Children.Add(ErzeugeGruppenZeile(gruppe.Titel, $"{gruppe.Erledigt:F0} / {gruppe.Gesamt:F0} {einheit}", prozent));
            }

            return new Border { BorderBrush = LinieHell, BorderThickness = new Thickness(1), Padding = new Thickness(18, 14, 18, 14), Child = inhalt };
        }

        private static Grid ErzeugeGruppenZeile(string titel, string mengeText, double prozent)
        {
            var zeile = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.2, GridUnitType.Star) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });

            var titelText = new TextBlock { Text = titel, FontSize = 10, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            Grid.SetColumn(titelText, 0);

            var balkenSpur = new Border { Background = TrackHell, CornerRadius = new CornerRadius(4), Height = 10, Margin = new Thickness(10, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            var balkenFuellung = new Border
            {
                Background = prozent >= 100 ? Erfolg : Akzent,
                CornerRadius = new CornerRadius(4),
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = Math.Max(4, Math.Clamp(prozent, 0, 100)),
                Margin = new Thickness(10, 0, 0, 0)
            };
            var balkenGrid = new Grid();
            balkenGrid.Children.Add(balkenSpur);
            balkenGrid.Children.Add(balkenFuellung);
            Grid.SetColumn(balkenGrid, 1);

            var mengeTextBlock = new TextBlock { Text = mengeText, FontSize = 9.5, Foreground = TextGrau, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(mengeTextBlock, 2);

            var prozentText = new TextBlock { Text = $"{prozent:F0} %", FontSize = 10, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(prozentText, 3);

            zeile.Children.Add(titelText);
            zeile.Children.Add(balkenGrid);
            zeile.Children.Add(mengeTextBlock);
            zeile.Children.Add(prozentText);
            return zeile;
        }

        private static IEnumerable<Block> ErzeugeLaengenAbschnitt(Laenge laenge)
        {
            var (gesamtMeter, erledigtMeter, _, erledigtProzent, _) = VisualBuilder.BerechneGesamt(laenge.Leistungen);

            yield return ErzeugeAbschnittsTitel($"{laenge.Bezeichnung}  ·  {erledigtMeter:N0} von {gesamtMeter:N0} m erledigt ({erledigtProzent:F0} %)");

            var tabelle = NeueTabelle(new[] { 3.2, 1.0, 0.8, 0.8, 0.8, 1.5 });

            var kopfGruppe = new TableRowGroup();
            kopfGruppe.Rows.Add(ErzeugeTabellenKopf("Beschreibung", "Bahnseite", "Km von", "Km bis", "Meter", "Status"));
            tabelle.RowGroups.Add(kopfGruppe);

            var datenGruppe = new TableRowGroup();
            bool gerade = false;
            foreach (var l in laenge.Leistungen.OrderBy(l => l.KmVon))
            {
                var zeile = new TableRow { Background = gerade ? ZeileWechsel : Brushes.Transparent };
                zeile.Cells.Add(ErzeugeZelle(l.Leistungsbeschreibung));
                zeile.Cells.Add(ErzeugeZelle(l.Bahnseite));
                zeile.Cells.Add(ErzeugeZelle(l.KmVon.ToString("F3"), TextAlignment.Right));
                zeile.Cells.Add(ErzeugeZelle(l.KmBis.ToString("F3"), TextAlignment.Right));
                zeile.Cells.Add(ErzeugeZelle((l.LaengeMeter ?? 0).ToString("F0"), TextAlignment.Right));
                zeile.Cells.Add(ErzeugeStatusZelle(l));
                datenGruppe.Rows.Add(zeile);
                gerade = !gerade;
            }
            tabelle.RowGroups.Add(datenGruppe);

            yield return tabelle;
            yield return new Paragraph { Margin = new Thickness(0, 0, 0, 20) };
        }

        private static Paragraph ErzeugeAbschnittsTitel(string text)
        {
            return new Paragraph(new Run(text))
            {
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(0, 0, 0, 4),
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }

        private static Table NeueTabelle(double[] spaltenAnteile)
        {
            var tabelle = new Table { CellSpacing = 0, FontSize = 9.5 };
            foreach (var anteil in spaltenAnteile)
                tabelle.Columns.Add(new TableColumn { Width = new GridLength(anteil, GridUnitType.Star) });
            return tabelle;
        }

        private static TableRow ErzeugeTabellenKopf(params string[] titel)
        {
            var zeile = new TableRow();
            foreach (var t in titel)
            {
                zeile.Cells.Add(new TableCell(new Paragraph(new Run(t.ToUpperInvariant())) { FontWeight = FontWeights.Bold, FontSize = 9 })
                {
                    Padding = new Thickness(6, 4, 6, 6),
                    BorderBrush = TextDunkel,
                    BorderThickness = new Thickness(0, 0, 0, 1.5),
                    Foreground = TextGrau
                });
            }
            return zeile;
        }

        private static TableCell ErzeugeZelle(string text, TextAlignment ausrichtung = TextAlignment.Left)
        {
            return new TableCell(new Paragraph(new Run(text)) { TextAlignment = ausrichtung })
            {
                Padding = new Thickness(6, 5, 6, 5),
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }

        private static TableCell ErzeugeStatusZelle(Leistung leistung)
        {
            string text;
            Brush farbe;

            if (leistung.IstAbgerechnet)
            {
                text = leistung.AufmassNummer.HasValue ? $"Abgerechnet (Aufmaß {leistung.AufmassNummer})" : "Abgerechnet";
                farbe = Akzent;
            }
            else if (leistung.IstFertiggestellt)
            {
                text = "Erledigt";
                farbe = Erfolg;
            }
            else
            {
                text = "Offen";
                farbe = TextGrau;
            }

            return new TableCell(new Paragraph(new Run(text)) { Foreground = farbe, FontWeight = FontWeights.SemiBold })
            {
                Padding = new Thickness(6, 5, 6, 5),
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }
    }
}
