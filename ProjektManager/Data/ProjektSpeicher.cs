using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjektManager.Models;
using System.IO;
using ProjektManager.Helpers;

namespace ProjektManager.Data
{
    public static class ProjektSpeicher
    {
        private const int MaxBackups = 20;

        private static string SpeicherPfad => ProjektPfadHelper.ProjekteDateiPfad;

        public static void Speichern(List<Projekt> projekte)
        {
            var pfad = SpeicherPfad;
            var verzeichnis = Path.GetDirectoryName(pfad);
            if (!string.IsNullOrEmpty(verzeichnis))
            {
                Directory.CreateDirectory(verzeichnis);
            }

            if (File.Exists(pfad))
            {
                ErstelleBackup(pfad, verzeichnis);
            }

            var json = JsonSerializer.Serialize(projekte, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(pfad, json);
        }

        /// <summary>
        /// Legt vor dem Überschreiben eine zeitgestempelte Kopie der bisherigen projekte.json
        /// im Unterordner "Backups" an, damit ein beschädigtes Speichern oder ein versehentliches
        /// Löschen nicht sofort zum vollständigen Datenverlust führt. Ein Fehler beim Backup darf
        /// das eigentliche Speichern nicht verhindern.
        /// </summary>
        private static void ErstelleBackup(string pfad, string? verzeichnis)
        {
            try
            {
                if (string.IsNullOrEmpty(verzeichnis)) return;

                var backupOrdner = Path.Combine(verzeichnis, "Backups");
                Directory.CreateDirectory(backupOrdner);

                var zeitstempel = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPfad = Path.Combine(backupOrdner, $"projekte_{zeitstempel}.json");
                File.Copy(pfad, backupPfad, overwrite: true);

                var alteBackups = Directory.GetFiles(backupOrdner, "projekte_*.json")
                    .OrderByDescending(f => f)
                    .Skip(MaxBackups);

                foreach (var datei in alteBackups)
                {
                    try { File.Delete(datei); } catch { /* nicht kritisch */ }
                }
            }
            catch
            {
                // Ein fehlgeschlagenes Backup darf das eigentliche Speichern nicht verhindern.
            }
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