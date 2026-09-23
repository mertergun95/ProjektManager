namespace ProjektManager.Models
{
    public class Projekt
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProjektPfad { get; set; } = string.Empty;
        public List<Laenge> Laengen { get; set; } = new();
    }
}
