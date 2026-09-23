using System.Data;
using System.Globalization;
using System.IO;
using ExcelDataReader;

namespace ProjektManager.Helpers
{
    public static class ExcelReader
    {
        public static DataTable LeseExcel(string filePath, int skipRows = 1)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

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

        /// <summary>
        /// Parst eine Excel-Zelle als Zahl, unabhängig von der System-Kultur.
        /// Erkennt deutsches ("1.234,56"), einfaches deutsches ("12,345") und
        /// invariantes ("12.345") Zahlenformat anhand der vorhandenen Trennzeichen.
        /// </summary>
        public static double ParseDouble(object? value)
        {
            if (value == null) return 0;

            string s = value.ToString()?.Trim() ?? string.Empty;
            if (s.Length == 0) return 0;

            s = s.Replace("km", "", StringComparison.OrdinalIgnoreCase).Trim();

            bool hatKomma = s.Contains(',');
            bool hatPunkt = s.Contains('.');

            if (hatKomma && hatPunkt)
            {
                // Das zuletzt stehende Zeichen ist das Dezimaltrennzeichen.
                s = s.LastIndexOf(',') > s.LastIndexOf('.')
                    ? s.Replace(".", "").Replace(",", ".")
                    : s.Replace(",", "");
            }
            else if (hatKomma)
            {
                s = s.Replace(",", ".");
            }

            double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
            return result;
        }
    }
}
