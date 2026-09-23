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
    /// Erstellt einen gestalteten, druckfähigen Projektbericht im Querformat: Kopfzeile, drei
    /// Kennzahlen-Karten über die volle Breite, eine Tabelle "Fortschritt nach Leistungsart" mit
    /// echten Balken je Zeile, und je Länge eine breite, übersichtliche Tabelle. Alle Abschnitte
    /// mit potenziell vielen Zeilen sind als echte FlowDocument-Tabellen umgesetzt, damit sie bei
    /// Bedarf sauber über mehrere Seiten laufen, statt am Seitenende abgeschnitten zu werden.
    /// Zeigt den Windows-Druckdialog an; über "Microsoft Print to PDF" lässt sich der Bericht so
    /// auch ohne zusätzliche PDF-Bibliothek als Datei speichern.
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

            // Der Bericht ist für Querformat gestaltet (breite Tabellen, viel Platz je Spalte);
            // das wird als Vorauswahl im Druckdialog gesetzt, der Benutzer kann es dort ändern.
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
                Foreground = TextDunkel,
                // Ohne explizite ColumnWidth berechnet FlowDocument selbst eine "lesefreundliche"
                // schmale Spaltenbreite und reißt den Bericht bei breiten (Querformat-)Seiten in
                // mehrere Zeitungs-Spalten auseinander. PositiveInfinity erzwingt EINE Spalte über
                // die volle Seitenbreite.
                ColumnWidth = double.PositiveInfinity
            };

            dokument.Blocks.Add(ErzeugeKopfbereich(projekt));

            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            dokument.Blocks.Add(ErzeugeKennzahlenLeiste(alleLeistungen));
            dokument.Blocks.AddRange(ErzeugeGruppenTabelle(alleLeistungen));

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

        /// <summary>Drei Kennzahlen-Karten über die volle Seitenbreite (fixe, geringe Höhe – unkritisch für die Seitenaufteilung).</summary>
        private static Block ErzeugeKennzahlenLeiste(List<Leistung> alleLeistungen)
        {
            var (gesamtMeter, erledigtMeter, abgerechnetMeter, erledigtProzent, abgerechnetProzent) = VisualBuilder.BerechneGesamt(alleLeistungen);

            var raster = new Grid();
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var karte1 = ErzeugeKennzahlKarte("Gesamtlänge", $"{gesamtMeter:N0} m", null, TextDunkel, istErsteKarte: true);
            var karte2 = ErzeugeKennzahlKarte("Erledigt", $"{erledigtProzent:F0} %", $"{erledigtMeter:N0} m", Erfolg, istErsteKarte: false);
            var karte3 = ErzeugeKennzahlKarte("Abgerechnet", $"{abgerechnetProzent:F0} %", $"{abgerechnetMeter:N0} m", Akzent, istErsteKarte: false);

            Grid.SetColumn(karte1, 0);
            Grid.SetColumn(karte2, 1);
            Grid.SetColumn(karte3, 2);
            raster.Children.Add(karte1);
            raster.Children.Add(karte2);
            raster.Children.Add(karte3);

            var rahmen = new Border
            {
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0, 16, 0, 16),
                Margin = new Thickness(0, 0, 0, 26),
                Child = raster
            };

            return new BlockUIContainer(rahmen);
        }

        private static Border ErzeugeKennzahlKarte(string label, string wert, string? unterzeile, Brush wertFarbe, bool istErsteKarte)
        {
            var inhalt = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            inhalt.Children.Add(new TextBlock { Text = wert, FontSize = 26, FontWeight = FontWeights.Bold, Foreground = wertFarbe, HorizontalAlignment = HorizontalAlignment.Center });
            inhalt.Children.Add(new TextBlock { Text = label.ToUpperInvariant(), FontSize = 9.5, FontWeight = FontWeights.SemiBold, Foreground = TextGrau, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) });

            if (unterzeile != null)
                inhalt.Children.Add(new TextBlock { Text = unterzeile, FontSize = 9.5, Foreground = TextGrau, HorizontalAlignment = HorizontalAlignment.Center });

            return new Border
            {
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(istErsteKarte ? 0 : 1, 0, 0, 0),
                Padding = new Thickness(12, 0, 12, 0),
                Child = inhalt
            };
        }

        /// <summary>
        /// "Fortschritt nach Leistungsart" als echte, seitenübergreifend umbrechende Tabelle (kann
        /// bei vielen unterschiedlichen Leistungsarten mehrere Seiten füllen); die letzte Spalte
        /// enthält je Zeile einen echten, proportional gefüllten Balken.
        /// </summary>
        private static IEnumerable<Block> ErzeugeGruppenTabelle(List<Leistung> alleLeistungen)
        {
            yield return ErzeugeAbschnittsTitel("Fortschritt nach Leistungsart");

            var tabelle = NeueTabelle(new[] { 2.6, 1.0, 1.0, 2.4 });
            var kopfGruppe = new TableRowGroup();
            kopfGruppe.Rows.Add(ErzeugeTabellenKopf("Leistungsart", "Erledigt", "Gesamt", "Fortschritt"));
            tabelle.RowGroups.Add(kopfGruppe);

            var datenGruppe = new TableRowGroup();
            bool gerade = false;
            foreach (var gruppe in VisualBuilder.BerechneGruppen(alleLeistungen).OrderByDescending(g => g.Gesamt))
            {
                double prozent = gruppe.Gesamt > 0 ? gruppe.Erledigt / gruppe.Gesamt * 100 : 0;
                string einheit = gruppe.IstStueck ? "Stück" : "m";

                var zeile = new TableRow { Background = gerade ? ZeileWechsel : Brushes.Transparent };
                zeile.Cells.Add(ErzeugeZelle(gruppe.Titel));
                zeile.Cells.Add(ErzeugeZelle($"{gruppe.Erledigt:F0} {einheit}", TextAlignment.Right));
                zeile.Cells.Add(ErzeugeZelle($"{gruppe.Gesamt:F0} {einheit}", TextAlignment.Right));
                zeile.Cells.Add(ErzeugeBalkenZelle(prozent));
                datenGruppe.Rows.Add(zeile);
                gerade = !gerade;
            }
            tabelle.RowGroups.Add(datenGruppe);

            yield return tabelle;
            yield return new Paragraph { Margin = new Thickness(0, 0, 0, 22) };
        }

        private static TableCell ErzeugeBalkenZelle(double prozent)
        {
            double gefuellterAnteil = Math.Clamp(prozent, 0, 100);

            // Füllung als Grid-Star-Anteil statt fester Pixelbreite: die Füllung ist so immer
            // exakt "prozent" Prozent der tatsächlichen (erst beim Layout bekannten) Balkenbreite.
            var balkenInnen = new Grid();
            balkenInnen.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(gefuellterAnteil, 0.1), GridUnitType.Star) });
            balkenInnen.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Math.Max(100 - gefuellterAnteil, 0.1), GridUnitType.Star) });

            var balkenFuellung = new Border { Background = prozent >= 100 ? Erfolg : Akzent, CornerRadius = new CornerRadius(4) };
            Grid.SetColumn(balkenFuellung, 0);
            balkenInnen.Children.Add(balkenFuellung);

            var balkenSpur = new Border { Background = TrackHell, CornerRadius = new CornerRadius(4), Height = 11, Child = balkenInnen };

            var reihe = new Grid { VerticalAlignment = VerticalAlignment.Center };
            reihe.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            reihe.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(balkenSpur, 0);
            var prozentText = new TextBlock { Text = $"{prozent:F0} %", FontSize = 9.5, FontWeight = FontWeights.Bold, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(prozentText, 1);
            reihe.Children.Add(balkenSpur);
            reihe.Children.Add(prozentText);

            return new TableCell(new BlockUIContainer(reihe))
            {
                Padding = new Thickness(6, 7, 6, 7),
                BorderBrush = LinieHell,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
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
