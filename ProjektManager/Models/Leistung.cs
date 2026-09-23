namespace ProjektManager.Models
{
    public class Leistung
    {
        public int Id { get; set; }
        public double KmVon { get; set; }
        public double KmBis { get; set; }
        public string Bahnseite { get; set; } = string.Empty;
        public string Leistungsbeschreibung { get; set; } = string.Empty;
        public string Anmerkung { get; set; } = string.Empty;
        public string Anmerkung2 { get; set; } = string.Empty;
        public double? LaengeMeter { get; set; }

        public bool IstFertiggestellt { get; set; }
        public bool IstAbgerechnet { get; set; }

        public string Notiz { get; set; } = string.Empty;
    }
}
