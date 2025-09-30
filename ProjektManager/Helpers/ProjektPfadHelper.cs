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

        // LST klasör ve index
        public static string LSTProjektOrdner => Path.Combine(BaseOrdner, "LST_Projekte");
        public static string LSTIndexDatei => Path.Combine(LSTProjektOrdner, "lst_projekte_index.json");

        public static string LegacyLSTProjektOrdner => Path.Combine(OldBaseOrdner, "LST_Projekte");
        public static string LegacyLSTIndexDatei => Path.Combine(LegacyLSTProjektOrdner, "lst_projekte_index.json");

        // LWL klasör ve index
        public static string LWLProjektOrdner => Path.Combine(BaseOrdner, "LWL_Projekte");
        public static string LWLIndexDatei => Path.Combine(LWLProjektOrdner, "lwl_projekte_index.json");

        public static string LegacyLWLProjektOrdner => Path.Combine(OldBaseOrdner, "LWL_Projekte");
        public static string LegacyLWLIndexDatei => Path.Combine(LegacyLWLProjektOrdner, "lwl_projekte_index.json");

        // Eski property isimleriyle uyumluluk
        public static string LST_Projekte_Ordner => LSTProjektOrdner;
        public static string LST_IndexDatei => LSTIndexDatei;
        public static string LWL_Projekte_Ordner => LWLProjektOrdner;
        public static string LWL_IndexDatei => LWLIndexDatei;

        public static string LegacyProjektDatei(string unterordner, string projektName)
        {
            return Path.Combine(Path.Combine(OldBaseOrdner, unterordner), projektName + ".json");
        }

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

        public static void StelleVerzeichnisseSicher(string unterordner)
        {
            Directory.CreateDirectory(BaseOrdner);

            string pfad = Path.Combine(BaseOrdner, unterordner);
            if (!Directory.Exists(pfad))
                Directory.CreateDirectory(pfad);

            string indexPfad = Path.Combine(pfad, unterordner.ToLower() + "_index.json");
            if (!File.Exists(indexPfad))
                File.WriteAllText(indexPfad, "[]");
        }
    }
}
