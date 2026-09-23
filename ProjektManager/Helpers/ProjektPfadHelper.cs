using System;
using System.IO;

namespace ProjektManager.Helpers
{
    public static class ProjektPfadHelper
    {
        private static readonly string BaseOrdner = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "OneDrive - Klefenz GmbH",
            "Gleisbau",
            "ProjektManagerProgramm");

        private static readonly string OldBaseOrdner = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "OneDrive - Klefenz GmbH",
            "ProjektManagerProgramm");

        public static string ProjekteDateiPfad
        {
            get
            {
                Directory.CreateDirectory(BaseOrdner);
                return Path.Combine(BaseOrdner, "projekte.json");
            }
        }

        public static string LegacyProjekteDateiPfad => Path.Combine(OldBaseOrdner, "projekte.json");

        public static void TryDeleteLegacyFile(string pfad)
        {
            try
            {
                if (File.Exists(pfad))
                {
                    File.Delete(pfad);
                }
            }
            catch
            {
                // Ignored: fehlende Berechtigungen sollen die Migration nicht stoppen.
            }
        }
    }
}
