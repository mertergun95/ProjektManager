using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ProjektManager.Helpers
{
    /// <summary>
    /// Erstellt einen druckbaren Projektbericht (Übersicht + je Länge eine Leistungstabelle) und
    /// zeigt den Windows-Druckdialog an. Über "Microsoft Print to PDF" lässt sich der Bericht so
    /// auch ohne zusätzliche PDF-Bibliothek als Datei speichern.
    /// </summary>
    public static class BerichtDrucker
    {
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
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Segoe UI")
            };

            dokument.Blocks.Add(new Paragraph(new Run($"Projektbericht: {projekt.Name}"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold
            });

            dokument.Blocks.Add(new Paragraph(new Run($"Erstellt am {DateTime.Now:dd.MM.yyyy HH:mm}"))
            {
                FontSize = 11,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            dokument.Blocks.Add(new BlockUIContainer(VisualBuilder.ErzeugeZusammenfassung(alleLeistungen))
            {
                Margin = new Thickness(0, 0, 0, 20)
            });

            foreach (var laenge in projekt.Laengen)
            {
                dokument.Blocks.Add(new Paragraph(new Run(laenge.Bezeichnung))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 16, 0, 6)
                });

                dokument.Blocks.Add(ErzeugeLeistungsTabelle(laenge.Leistungen));
            }

            return dokument;
        }

        private static Table ErzeugeLeistungsTabelle(IEnumerable<Leistung> leistungen)
        {
            var tabelle = new Table { CellSpacing = 0, FontSize = 10 };

            string[] kopfzeile = { "Beschreibung", "Bahnseite", "Km von", "Km bis", "Meter", "Status" };
            foreach (var _ in kopfzeile)
                tabelle.Columns.Add(new TableColumn());

            var kopfGruppe = new TableRowGroup();
            var kopfZeile = new TableRow { Background = Brushes.LightGray };
            foreach (var titel in kopfzeile)
                kopfZeile.Cells.Add(ErzeugeZelle(titel, fett: true));
            kopfGruppe.Rows.Add(kopfZeile);
            tabelle.RowGroups.Add(kopfGruppe);

            var datenGruppe = new TableRowGroup();
            foreach (var l in leistungen)
            {
                var zeile = new TableRow();
                zeile.Cells.Add(ErzeugeZelle(l.Leistungsbeschreibung));
                zeile.Cells.Add(ErzeugeZelle(l.Bahnseite));
                zeile.Cells.Add(ErzeugeZelle(l.KmVon.ToString("F3")));
                zeile.Cells.Add(ErzeugeZelle(l.KmBis.ToString("F3")));
                zeile.Cells.Add(ErzeugeZelle((l.LaengeMeter ?? 0).ToString("F0")));

                string status = l.IstAbgerechnet
                    ? $"Abgerechnet{(l.AufmassNummer.HasValue ? $" (Aufmaß {l.AufmassNummer})" : "")}"
                    : l.IstFertiggestellt ? "Erledigt" : "Offen";
                zeile.Cells.Add(ErzeugeZelle(status));

                datenGruppe.Rows.Add(zeile);
            }
            tabelle.RowGroups.Add(datenGruppe);

            return tabelle;
        }

        private static TableCell ErzeugeZelle(string text, bool fett = false)
        {
            return new TableCell(new Paragraph(new Run(text)) { FontWeight = fett ? FontWeights.Bold : FontWeights.Normal })
            {
                Padding = new Thickness(4),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }
    }
}
