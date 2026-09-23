namespace ProjektManager.Helpers
{
    /// <summary>
    /// Legt fest, aus welcher (0-basierten) Excel-Spalte jedes Leistungsfeld gelesen wird.
    /// Die Standardwerte entsprechen dem bisher fest einprogrammierten Spaltenlayout, damit
    /// bestehende Excel-Vorlagen ohne Änderung weiter funktionieren; über den
    /// Spaltenzuordnungs-Dialog kann der Benutzer sie bei Bedarf an ein abweichendes Layout anpassen.
    /// </summary>
    public class ExcelSpaltenZuordnung
    {
        public int KmVon { get; set; } = 1;
        public int KmBis { get; set; } = 2;
        public int Bahnseite { get; set; } = 3;
        public int Beschreibung { get; set; } = 4;
        public int Anmerkung { get; set; } = 5;
        public int LaengeMeter { get; set; } = 7;
        public int Anmerkung2A { get; set; } = 8;
        public int Anmerkung2B { get; set; } = 9;

        public ExcelSpaltenZuordnung Kopie() => new()
        {
            KmVon = KmVon,
            KmBis = KmBis,
            Bahnseite = Bahnseite,
            Beschreibung = Beschreibung,
            Anmerkung = Anmerkung,
            LaengeMeter = LaengeMeter,
            Anmerkung2A = Anmerkung2A,
            Anmerkung2B = Anmerkung2B
        };
    }
}
