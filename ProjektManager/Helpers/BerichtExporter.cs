using ClosedXML.Excel;
using ProjektManager.Models;

namespace ProjektManager.Helpers
{
    /// <summary>Exportiert ein Projekt als Excel-Bericht (eine Zeile je Leistung).</summary>
    public static class BerichtExporter
    {
        public static void ExportiereAlsExcel(Projekt projekt, string zielPfad)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Bericht");

            var spalten = new[]
            {
                "Länge", "Leistungsbeschreibung", "Bahnseite", "Km von", "Km bis",
                "Länge (m)", "Erledigt", "Abgerechnet", "Aufmaß-Nr.", "Notiz"
            };

            for (int i = 0; i < spalten.Length; i++)
                sheet.Cell(1, i + 1).Value = spalten[i];

            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF0F4");

            int zeile = 2;
            foreach (var laenge in projekt.Laengen)
            {
                foreach (var l in laenge.Leistungen)
                {
                    sheet.Cell(zeile, 1).Value = laenge.Bezeichnung;
                    sheet.Cell(zeile, 2).Value = l.Leistungsbeschreibung;
                    sheet.Cell(zeile, 3).Value = l.Bahnseite;
                    sheet.Cell(zeile, 4).Value = l.KmVon;
                    sheet.Cell(zeile, 5).Value = l.KmBis;
                    sheet.Cell(zeile, 6).Value = l.LaengeMeter ?? 0;
                    sheet.Cell(zeile, 7).Value = l.IstFertiggestellt ? "Ja" : "Nein";
                    sheet.Cell(zeile, 8).Value = l.IstAbgerechnet ? "Ja" : "Nein";
                    sheet.Cell(zeile, 9).Value = l.AufmassNummer?.ToString() ?? "";
                    sheet.Cell(zeile, 10).Value = l.Notiz;

                    if (l.IstFertiggestellt)
                        sheet.Range(zeile, 1, zeile, spalten.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#E4F6ED");

                    zeile++;
                }
            }

            sheet.Columns().AdjustToContents();
            sheet.SheetView.FreezeRows(1);

            workbook.SaveAs(zielPfad);
        }
    }
}
