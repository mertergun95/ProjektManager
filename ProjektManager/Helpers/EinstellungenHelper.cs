using System.IO;
using System.Text.Json;

namespace ProjektManager.Helpers
{
    public class Einstellungen
    {
        public bool DunkelModus { get; set; }
    }

    /// <summary>Persistiert kleine, benutzerspezifische App-Einstellungen (z.B. Dunkelmodus).</summary>
    public static class EinstellungenHelper
    {
        private static string DateiPfad => Path.Combine(ProjektPfadHelper.BasisOrdner, "einstellungen.json");

        public static Einstellungen Laden()
        {
            try
            {
                if (!File.Exists(DateiPfad)) return new Einstellungen();

                var json = File.ReadAllText(DateiPfad);
                return JsonSerializer.Deserialize<Einstellungen>(json) ?? new Einstellungen();
            }
            catch
            {
                return new Einstellungen();
            }
        }

        public static void Speichern(Einstellungen einstellungen)
        {
            try
            {
                var json = JsonSerializer.Serialize(einstellungen, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DateiPfad, json);
            }
            catch
            {
                // Ein fehlgeschlagenes Speichern der Einstellungen darf die App nicht stören.
            }
        }
    }
}
