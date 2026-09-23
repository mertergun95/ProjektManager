using System.IO;
using System.Reflection;

namespace ProjektManager.Helpers
{
    /// <summary>
    /// Da die App nur als geteiltes exe/Ordner (ohne Installer) verteilt wird, gibt es keine
    /// automatische Update-Installation. Stattdessen: legt derjenige, der eine neue Version
    /// baut, eine "version.txt" (1. Zeile: Versionsnummer, optionale 2. Zeile: Hinweistext,
    /// z.B. wo die neue exe liegt) im gemeinsamen OneDrive-Basisordner ab, macht diese Klasse
    /// beim Start alle anderen Sitzungen darauf aufmerksam, dass eine neuere Version verfügbar ist.
    /// </summary>
    public static class VersionHelper
    {
        public static string AktuelleVersion =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

        public static string? PruefeAufNeuereVersion()
        {
            try
            {
                var pfad = Path.Combine(ProjektPfadHelper.BasisOrdner, "version.txt");
                if (!File.Exists(pfad)) return null;

                var zeilen = File.ReadAllLines(pfad);
                if (zeilen.Length == 0) return null;

                var verfuegbareVersion = zeilen[0].Trim();
                if (!Version.TryParse(verfuegbareVersion, out var verfuegbar) ||
                    !Version.TryParse(AktuelleVersion, out var aktuell) ||
                    verfuegbar <= aktuell)
                {
                    return null;
                }

                var hinweis = zeilen.Length > 1 ? zeilen[1].Trim() : "";
                return string.IsNullOrEmpty(hinweis)
                    ? $"Eine neuere Version ({verfuegbareVersion}) ist verfügbar."
                    : $"Eine neuere Version ({verfuegbareVersion}) ist verfügbar. {hinweis}";
            }
            catch
            {
                return null;
            }
        }
    }
}
