using ProjektManager.Data;
using ProjektManager.Helpers;
using ProjektManager.Models;
using ProjektManager.ViewModels;
using ProjektManager.Views;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProjektManager
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Stack<UserControl> _pageHistory = new();
        private readonly DispatcherTimer _statusTimer;

        public MainViewModel ViewModel => _viewModel;
        public Projekt? AktuellesProjekt { get; set; }
        public List<Projekt> AlleProjekte => _viewModel.Projekte.ToList();

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            _statusTimer.Tick += (s, e) =>
            {
                _statusTimer.Stop();
                StatusBorder.Visibility = Visibility.Collapsed;
            };

            DunkelModusButton.Content = EinstellungenHelper.Laden().DunkelModus ? "☀" : "🌙";

            ZeigeUebersicht();

            var updateHinweis = VersionHelper.PruefeAufNeuereVersion();
            if (updateHinweis != null)
                ZeigeStatus(updateHinweis, dauerhaft: true, istHinweis: true);
        }

        public void ZeigeSeite(UserControl seite)
        {
            if (MainContent.Content != null)
                _pageHistory.Push((UserControl)MainContent.Content);

            MainContent.Content = seite;
            BackButton.Visibility = _pageHistory.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Zeigt die Projektübersicht als Startpunkt der Navigation an und leert dabei die
        /// Verlaufs-Historie, damit nach Anlegen/Bearbeiten/Löschen eines Projekts kein veralteter
        /// (z.B. bereits gelöschter) Übersichts-Snapshot über den Zurück-Button erreichbar bleibt.
        /// </summary>
        public void ZeigeUebersicht()
        {
            _pageHistory.Clear();
            MainContent.Content = new ProjektUebersichtPage(this, AlleProjekte);
            BackButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Speichert alle Projekte und zeigt kurz eine Bestätigung in der Statusleiste an. Warnt
        /// vorher, falls die Datei seit dem letzten Laden/Speichern DIESER Sitzung von außen
        /// verändert wurde (z.B. durch eine andere, gleichzeitig laufende Sitzung auf der
        /// gemeinsamen OneDrive-Datei) – ein Speichern würde diese fremden Änderungen sonst
        /// stillschweigend überschreiben.
        /// </summary>
        public void SpeichernUndBestaetigen()
        {
            if (ProjektSpeicher.WurdeExternGeaendert())
            {
                var ergebnis = MessageBox.Show(
                    "Die Projektdatei wurde zwischenzeitlich von einer anderen Sitzung geändert " +
                    "(z.B. auf einem anderen Rechner). Wenn Sie jetzt speichern, gehen die dortigen " +
                    "Änderungen verloren.\n\nTrotzdem überschreiben?",
                    "Änderungskonflikt", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (ergebnis != MessageBoxResult.Yes)
                {
                    ZeigeStatus("Nicht gespeichert (Konflikt)");
                    return;
                }
            }

            ProjektSpeicher.Speichern(AlleProjekte);
            ZeigeStatus("Gespeichert");
        }

        /// <param name="dauerhaft">Wenn true, blendet sich die Leiste nicht automatisch aus und zeigt einen Schließen-Button.</param>
        /// <param name="istHinweis">Wenn true, wird die Leiste als neutraler Hinweis statt als grüne Erfolgsmeldung eingefärbt.</param>
        public void ZeigeStatus(string text, bool dauerhaft = false, bool istHinweis = false)
        {
            StatusText.Text = text;
            StatusBorder.Background = (Brush)Resources[istHinweis ? "AccentSoftBrush" : "SuccessSoftBrush"];
            StatusText.Foreground = (Brush)Resources[istHinweis ? "AccentBrush" : "SuccessBrush"];
            StatusSchliessenButton.Visibility = dauerhaft ? Visibility.Visible : Visibility.Collapsed;
            StatusBorder.Visibility = Visibility.Visible;

            _statusTimer.Stop();
            if (!dauerhaft) _statusTimer.Start();
        }

        private void StatusSchliessen_Click(object sender, RoutedEventArgs e)
        {
            StatusBorder.Visibility = Visibility.Collapsed;
        }

        private void DunkelModus_Click(object sender, RoutedEventArgs e)
        {
            var einstellungen = EinstellungenHelper.Laden();
            einstellungen.DunkelModus = !einstellungen.DunkelModus;
            EinstellungenHelper.Speichern(einstellungen);

            var ergebnis = MessageBox.Show(
                $"Der {(einstellungen.DunkelModus ? "Dunkelmodus" : "Hellmodus")} wird nach einem Neustart aktiv.\n\nJetzt neu starten?",
                "Neustart erforderlich", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (ergebnis != MessageBoxResult.Yes) return;

            var exePfad = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePfad))
                Process.Start(exePfad);

            Application.Current.Shutdown();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pageHistory.Count > 0)
                MainContent.Content = _pageHistory.Pop();

            BackButton.Visibility = _pageHistory.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NeuesProjekt_Click(object sender, RoutedEventArgs e)
        {
            var nameWindow = new ProjektNameEingabeWindow { Owner = this };
            if (nameWindow.ShowDialog() != true) return;

            var importWindow = new ProjektImportWindow();
            if (importWindow.ShowDialog() == true && importWindow.ErgebnisProjekt != null)
            {
                var neuesProjekt = importWindow.ErgebnisProjekt;
                neuesProjekt.Name = nameWindow.ProjektName;
                _viewModel.Projekte.Add(neuesProjekt);

                SpeichernUndBestaetigen();
                ZeigeUebersicht();
            }
        }

        private void ProjektBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            var ausgewaehlt = WaehleProjekt("Welches Projekt möchten Sie bearbeiten?");
            if (ausgewaehlt == null) return;

            var nameWindow = new ProjektNameEingabeWindow(ausgewaehlt.Name) { Owner = this };
            if (nameWindow.ShowDialog() != true) return;

            var importWindow = new ProjektImportWindow(ausgewaehlt);
            if (importWindow.ShowDialog() == true && importWindow.ErgebnisProjekt != null)
            {
                ausgewaehlt.Name = nameWindow.ProjektName;
                ausgewaehlt.Laengen = importWindow.ErgebnisProjekt.Laengen;
                ausgewaehlt.ProjektPfad = importWindow.ErgebnisProjekt.ProjektPfad;

                SpeichernUndBestaetigen();
                ZeigeUebersicht();
            }
        }

        private void ProjektLoeschen_Click(object sender, RoutedEventArgs e)
        {
            var ausgewaehlt = WaehleProjekt("Welches Projekt möchten Sie löschen?");
            if (ausgewaehlt == null) return;

            var result = MessageBox.Show(
                $"Möchten Sie das Projekt '{ausgewaehlt.Name}' wirklich löschen?",
                "Projekt löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _viewModel.Projekte.Remove(ausgewaehlt);
            SpeichernUndBestaetigen();
            ZeigeUebersicht();
        }

        private Projekt? WaehleProjekt(string ueberschrift)
        {
            if (AlleProjekte.Count == 0)
            {
                MessageBox.Show("Es sind noch keine Projekte vorhanden.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            var auswahlFenster = new ProjektAuswahlWindow(AlleProjekte, ueberschrift) { Owner = this };
            return auswahlFenster.ShowDialog() == true ? auswahlFenster.AusgewaehltesProjekt : null;
        }
    }
}
