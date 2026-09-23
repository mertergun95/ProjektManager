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
    /// Fügt die Farbpalette (hell/dunkel) und die Styles zusammen, bevor irgendein Fenster
    /// erzeugt wird. Ein Theme-Wechsel zur Laufzeit wird bewusst nicht unterstützt (dafür
    /// müssten alle StaticResource-Bindungen zu DynamicResource werden), stattdessen wird nach
    /// einem Wechsel ein Neustart der App empfohlen.
    /// </summary>
    public static void LadeTheme(bool dunkelModus)
    {
        var farbPfad = dunkelModus ? "Themes/Colors.Dark.xaml" : "Themes/Colors.Light.xaml";

        var ressourcen = new ResourceDictionary();
        ressourcen.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(farbPfad, UriKind.Relative) });
        ressourcen.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/Styles.xaml", UriKind.Relative) });

        Current.Resources = ressourcen;
    }
}
