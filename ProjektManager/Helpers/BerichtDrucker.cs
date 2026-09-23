using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ProjektManager.Helpers
{
    /// <summary>
    /// Erstellt einen gestalteten, druckfähigen Projektbericht (Kopfzeile, Kennzahlen-Übersicht,
    /// Fortschritt nach Leistungsart, je Länge eine übersichtliche Tabelle) und zeigt den
    /// Windows-Druckdialog an. Über "Microsoft Print to PDF" lässt sich der Bericht so auch ohne
    /// zusätzliche PDF-Bibliothek als Datei speichern.
    /// </summary>
    public static class BerichtDrucker
    {
        private static readonly Brush TextDunkel = new SolidColorBrush(Color.FromRgb(0x1E, 0x24, 0x30));
        private static readonly Brush TextGrau = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
        private static readonly Brush Akzent = new SolidColorBrush(Color.FromRgb(0x2F, 0x6F, 0xED));
        private static readonly Brush Erfolg = new SolidColorBrush(Color.FromRgb(0x1E, 0x8E, 0x5A));
        private static readonly Brush LinieHell = new SolidColorBrush(Color.FromRgb(0xE1, 0xE4, 0xEA));
        private static readonly Brush ZeileWechsel = new SolidColorBrush(Color.FromRgb(0xF7, 0xF8, 0xFA));

        public static void DruckeProjektBericht(Projekt projekt)
        {
            var printDialog = new PrintDialog();
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
                PagePadding = new Thickness(44, 40, 44, 40),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                Foreground = TextDunkel
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
            var eyebrow = new Paragraph(new Run("PROJEKTBERICHT"))
            {
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Foreground = Akzent,
                Margin = new Thickness(0, 0, 0, 2)
            };

            var titel = new Paragraph(new Run(projekt.Name))
            {
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var datum = new Paragraph(new Run($"Erstellt am {DateTime.Now:dd.MM.yyyy} um {DateTime.Now:HH:mm} Uhr"))
            {
                FontSize = 10,
                Foreground = TextGrau,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var linie = new BlockUIContainer(new Border { Height = 2, Background = Akzent, Margin = new Thickness(0, 0, 0, 22) });

            var section = new Section();
            section.Blocks.Add(eyebrow);
            section.Blocks.Add(titel);
            section.Blocks.Add(datum);
            section.Blocks.Add(linie);
            return section;
        }

        private static Block ErzeugeKennzahlenLeiste(List<Leistung> alleLeistungen)
        {
            var (gesamtMeter, erledigtMeter, abgerechnetMeter, erledigtProzent, abgerechnetProzent) = VisualBuilder.BerechneGesamt(alleLeistungen);

            var raster = new Grid();
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            raster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var karte1 = ErzeugeKennzahlKarte($"{gesamtMeter:N0} m", "Gesamtlänge", null, TextDunkel, istErsteKarte: true);
            var karte2 = ErzeugeKennzahlKarte($"{erledigtProzent:F0} %", "Erledigt", $"{erledigtMeter:N0} m", Erfolg, istErsteKarte: false);
            var karte3 = ErzeugeKennzahlKarte($"{abgerechnetProzent:F0} %", "Abgerechnet", $"{abgerechnetMeter:N0} m", Akzent, istErsteKarte: false);

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
                Margin = new Thickness(0, 0, 0, 28),
                Child = raster
            };

            return new BlockUIContainer(rahmen);
        }

        private static Border ErzeugeKennzahlKarte(string wert, string label, string? unterzeile, Brush wertFarbe, bool istErsteKarte)
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

        private static IEnumerable<Block> ErzeugeGruppenTabelle(List<Leistung> alleLeistungen)
        {
            yield return ErzeugeAbschnittsTitel("Fortschritt nach Leistungsart");

            var tabelle = NeueTabelle(new[] { 3.0, 1.0, 1.0, 1.6 });
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
                zeile.Cells.Add(ErzeugeZelle($"{BaueTextBalken(prozent)}  {prozent:F0}%", TextAlignment.Left, monospace: true));
                datenGruppe.Rows.Add(zeile);
                gerade = !gerade;
            }
            tabelle.RowGroups.Add(datenGruppe);

            yield return tabelle;
            yield return new Paragraph { Margin = new Thickness(0, 0, 0, 10) };
        }

        private static IEnumerable<Block> ErzeugeLaengenAbschnitt(Laenge laenge)
        {
            var (gesamtMeter, erledigtMeter, _, erledigtProzent, _) = VisualBuilder.BerechneGesamt(laenge.Leistungen);

            yield return ErzeugeAbschnittsTitel($"{laenge.Bezeichnung}  ·  {erledigtMeter:N0} von {gesamtMeter:N0} m erledigt ({erledigtProzent:F0} %)");

            var tabelle = NeueTabelle(new[] { 2.4, 0.9, 0.8, 0.8, 0.8, 1.3 });

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

        private static TableCell ErzeugeZelle(string text, TextAlignment ausrichtung = TextAlignment.Left, bool monospace = false)
        {
            var paragraph = new Paragraph(new Run(text)) { TextAlignment = ausrichtung };
            if (monospace) paragraph.FontFamily = new FontFamily("Consolas");

            return new TableCell(paragraph)
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

        /// <summary>Text-Fortschrittsbalken (z.B. "■■■■■■■□□□□□"), damit der Fortschritt auch beim Drucken/Export als PDF ohne UI-Elemente sichtbar bleibt.</summary>
        private static string BaueTextBalken(double prozent, int laenge = 14)
        {
            int gefuellt = (int)Math.Round(Math.Clamp(prozent, 0, 100) / 100.0 * laenge);
            return new string('■', gefuellt) + new string('□', laenge - gefuellt);
        }
    }
}
