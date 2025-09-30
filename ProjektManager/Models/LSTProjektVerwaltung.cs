using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ProjektManager.Models;

public static class LSTProjektVerwaltung
{
    public static List<LSTProjekt> Projekte { get; private set; } = new List<LSTProjekt>();

    private static string SpeicherPfad => "lst_projekte.json";

    public static void Laden()
    {
        if (File.Exists(SpeicherPfad))
        {
            var json = File.ReadAllText(SpeicherPfad);
            Projekte = JsonConvert.DeserializeObject<List<LSTProjekt>>(json) ?? new List<LSTProjekt>();
        }
    }

    public static void Speichern()
    {
        var json = JsonConvert.SerializeObject(Projekte, Formatting.Indented);
        File.WriteAllText(SpeicherPfad, json);
    }
}
