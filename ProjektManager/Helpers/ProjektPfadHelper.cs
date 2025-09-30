using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace ProjektManager.Helpers // DİKKAT: BU namespace senin projenin adıyla aynı olmalı!
{
    public static class ProjektPfadHelper
    {
        private static readonly string BaseOrdner = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "OneDrive - Klefenz GmbH",
            "ProjektManagerProgramm");

        // LST klasör ve index
        public static string LSTProjektOrdner => Path.Combine(BaseOrdner, "LST_Projekte");
        public static string LSTIndexDatei => Path.Combine(LSTProjektOrdner, "lst_projekte_index.json");

        // LWL klasör ve index
        public static string LWLProjektOrdner => Path.Combine(BaseOrdner, "LWL_Projekte");
        public static string LWLIndexDatei => Path.Combine(LWLProjektOrdner, "lwl_projekte_index.json");

        public static void StelleVerzeichnisseSicher(string unterordner)
        {
            string pfad = Path.Combine(BaseOrdner, unterordner);
            if (!Directory.Exists(pfad))
                Directory.CreateDirectory(pfad);

            string indexPfad = Path.Combine(pfad, unterordner.ToLower() + "_index.json");
            if (!File.Exists(indexPfad))
                File.WriteAllText(indexPfad, "[]");
        }
    }
}
