using System;
using System.Collections.Generic;
using System.Text.Json;
using ProjektManager.Models;
using System.IO;
using ProjektManager.Helpers;

namespace ProjektManager.Data
{
    public static class ProjektSpeicher
    {
        private static string SpeicherPfad => ProjektPfadHelper.ProjekteDateiPfad;

        public static void Speichern(List<Projekt> projekte)
        {
            var pfad = SpeicherPfad;
            var verzeichnis = Path.GetDirectoryName(pfad);
            if (!string.IsNullOrEmpty(verzeichnis))
            {
                Directory.CreateDirectory(verzeichnis);
            }

            var json = JsonSerializer.Serialize(projekte, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(pfad, json);
        }

        public static List<Projekt> Laden()
        {
            var pfad = SpeicherPfad;

            if (!File.Exists(pfad))
                return new List<Projekt>();

            var json = File.ReadAllText(pfad);
            return JsonSerializer.Deserialize<List<Projekt>>(json) ?? new List<Projekt>();
        }
    }
}