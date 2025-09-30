using System;
using System.IO;

namespace ProjektManager.Helpers
{
    public static class ProjektPfadHelper
    {
        private static readonly string BaseOrdner = Path.Combine(
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

        // LST klasör ve index
        public static string LSTProjektOrdner => Path.Combine(BaseOrdner, "LST_Projekte");
        public static string LSTIndexDatei => Path.Combine(LSTProjektOrdner, "lst_projekte_index.json");

        // LWL klasör ve index
        public static string LWLProjektOrdner => Path.Combine(BaseOrdner, "LWL_Projekte");
        public static string LWLIndexDatei => Path.Combine(LWLProjektOrdner, "lwl_projekte_index.json");

        // Eski property isimleriyle uyumluluk
        public static string LST_Projekte_Ordner => LSTProjektOrdner;
        public static string LST_IndexDatei => LSTIndexDatei;
        public static string LWL_Projekte_Ordner => LWLProjektOrdner;
        public static string LWL_IndexDatei => LWLIndexDatei;

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
