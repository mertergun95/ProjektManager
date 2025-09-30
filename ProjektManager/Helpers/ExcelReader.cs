using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using ProjektManager.Models;

namespace ProjektManager.Helpers
{
    public static class ExcelReader
    {
        public static DataTable LeseExcel(string filePath, int skipRows = 1)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var config = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = false
                    }
                };

                var dataSet = reader.AsDataSet(config);
                var table = dataSet.Tables[0];

                for (int i = 0; i < skipRows; i++)
                {
                    if (table.Rows.Count > 0)
                        table.Rows.RemoveAt(0);
                }

                return table;
            }
        }

        public static List<LSTKabel> LeseLSTKabel(string excelPfad)
        {
            var liste = new List<LSTKabel>();
            var dt = LeseExcel(excelPfad);

            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    var kabelNr = row[1]?.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(kabelNr)) continue;

                    var kabel = new LSTKabel
                    {
                        Kabeltyp = row[0]?.ToString().Trim(),
                        KabelNr = kabelNr,
                        Kabelquerschnitt = row[2]?.ToString().Trim(),
                        VonPunkt = row[4]?.ToString().Trim(),
                        KmVon = TryParseDouble(row[5]),    // doğrudan metre
                        KmBis = TryParseDouble(row[7]),
                        KmVonRaw = row[5]?.ToString().Trim(),
                        KmBisRaw = row[7]?.ToString().Trim(),


                        BisPunkt = row[6]?.ToString().Trim(),
                        
                        LängeSoll = (int)TryParseDouble(row[8]),
                        Trommelnummer = row[11]?.ToString().Trim(),
                        Bestelllaenge = int.TryParse(row[9]?.ToString(), out var best) ? best : null,
                        VerlegeDatum = DateTime.TryParse(row[10]?.ToString(), out var date) ? date : (DateTime?)null,
                        Bemerkung = row[15]?.ToString()?.Trim()
                    };

                    liste.Add(kabel);
                }
                catch
                {
                    // Hatalı satır atlanır
                    continue;
                }
            }

            return liste;
        }

        private static double TryParseDouble(object value)
        {
            if (value == null) return 0;

            string s = value.ToString().Trim();

            // "1,688" gibi ise → "1688" yap
            if (s.Contains(",") && s.Count(c => c == ',') == 1 && !s.Contains("."))
            {
                s = s.Replace(",", ""); // 1,688 → 1688
            }

            // Almanca'da 1.688,00 olabilir → hepsini temizle
            s = s.Replace(".", "").Replace(",", "").Replace("km", "").Trim();

            double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result);

            return result;
        }


    }
}
