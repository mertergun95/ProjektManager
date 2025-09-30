using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProjektManager.Models;
using System.IO;

namespace ProjektManager.Data
{
    public static class ProjektSpeicher
    {
        private static readonly string SpeicherPfad = "projekte.json";

        public static void Speichern(List<Projekt> projekte)
        {
            var json = JsonSerializer.Serialize(projekte, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SpeicherPfad, json);
        }

        public static List<Projekt> Laden()
        {
            if (!File.Exists(SpeicherPfad))
                return new List<Projekt>();

            var json = File.ReadAllText(SpeicherPfad);
            return JsonSerializer.Deserialize<List<Projekt>>(json) ?? new List<Projekt>();
        }
    }
}