namespace ProjektManager.Models
{
    public class Laenge
    {
        public int Id { get; set; }
        public string Bezeichnung { get; set; } = string.Empty;
        public List<Leistung> Leistungen { get; set; } = new();
        public bool KabelVerlegt { get; set; }
    }
}
