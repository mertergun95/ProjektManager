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
            var projekte = LeseProjektDatei(pfad);

            if (projekte.Count > 0)
                return projekte;

            var legacyPfad = ProjektPfadHelper.LegacyProjekteDateiPfad;
            var legacyProjekte = LeseProjektDatei(legacyPfad);

            if (legacyProjekte.Count > 0)
            {
                Speichern(legacyProjekte);
                ProjektPfadHelper.TryDeleteLegacyFile(legacyPfad);
                return legacyProjekte;
            }

            return projekte;
        }

        private static List<Projekt> LeseProjektDatei(string pfad)
        {
            if (!File.Exists(pfad))
                return new List<Projekt>();

            var json = File.ReadAllText(pfad);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Projekt>();

            return JsonSerializer.Deserialize<List<Projekt>>(json) ?? new List<Projekt>();
        }
    }
}