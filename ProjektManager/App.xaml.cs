using ProjektManager.Helpers;
using System.Windows;

namespace ProjektManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        LadeTheme(EinstellungenHelper.Laden().DunkelModus);

        base.OnStartup(e);
    }

    /// <summary>
    /// Lädt die Farbpalette + Styles als EINE zusammenhängende Ressourcendatei, bevor irgendein
    /// Fenster erzeugt wird. Bewusst NICHT als separate Colors-/Styles-Dateien, die erst zur
    /// Laufzeit per Code zusammengeführt werden: eine zur Laufzeit aus mehreren Quell-Dateien
    /// zusammengebaute ResourceDictionary löst verschachtelte StaticResource-Verweise (z.B. eine
    /// Farbe innerhalb eines DropShadowEffect oder eines ControlTemplate-Triggers) nicht korrekt
    /// auf, wenn Farbe und Style in unterschiedlichen Dateien stehen – das führt zu einer
    /// XamlParseException beim ersten Rendern ("Ressource ... kann nicht gefunden werden").
    /// Innerhalb EINER Datei funktioniert dieselbe Verschachtelung dagegen problemlos.
    ///
    /// Ein Theme-Wechsel zur Laufzeit wird bewusst nicht unterstützt (dafür müssten alle
    /// StaticResource-Bindungen zu DynamicResource werden), stattdessen wird nach einem Wechsel
    /// ein Neustart der App empfohlen.
    /// </summary>
    public static void LadeTheme(bool dunkelModus)
    {
        var pfad = dunkelModus ? "Themes/Theme.Dark.xaml" : "Themes/Theme.Light.xaml";
        Current.Resources = new ResourceDictionary { Source = new Uri(pfad, UriKind.Relative) };
    }
}
