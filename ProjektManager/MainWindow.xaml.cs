using ProjektManager.Data;
using ProjektManager.Models;
using ProjektManager.ViewModels;
using ProjektManager.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjektManager
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Stack<UserControl> _pageHistory = new();

        public MainViewModel ViewModel => _viewModel;
        public Projekt? AktuellesProjekt { get; set; }
        public List<Projekt> AlleProjekte => _viewModel.Projekte.ToList();

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            ZeigeUebersicht();
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pageHistory.Count > 0)
                MainContent.Content = _pageHistory.Pop();

            BackButton.Visibility = _pageHistory.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NeuesProjekt_Click(object sender, RoutedEventArgs e)
        {
            var nameWindow = new ProjektNameEingabeWindow();
            if (nameWindow.ShowDialog() != true) return;

            var importWindow = new ProjektImportWindow();
            if (importWindow.ShowDialog() == true && importWindow.ErgebnisProjekt != null)
            {
                var neuesProjekt = importWindow.ErgebnisProjekt;
                neuesProjekt.Name = nameWindow.ProjektName;
                _viewModel.Projekte.Add(neuesProjekt);

                ProjektSpeicher.Speichern(_viewModel.Projekte.ToList());
                ZeigeUebersicht();
            }
        }

        private void ProjektBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is not ProjektUebersichtPage uebersichtPage || uebersichtPage.AusgewaehltesProjekt is not Projekt ausgewaehlt)
                return;

            var nameWindow = new ProjektNameEingabeWindow(ausgewaehlt.Name);
            if (nameWindow.ShowDialog() != true) return;

            var importWindow = new ProjektImportWindow(ausgewaehlt);
            if (importWindow.ShowDialog() == true && importWindow.ErgebnisProjekt != null)
            {
                ausgewaehlt.Name = nameWindow.ProjektName;
                ausgewaehlt.Laengen = importWindow.ErgebnisProjekt.Laengen;
                ausgewaehlt.ProjektPfad = importWindow.ErgebnisProjekt.ProjektPfad;

                ProjektSpeicher.Speichern(AlleProjekte);
                ZeigeUebersicht();
            }
        }

        private void ProjektLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is not ProjektUebersichtPage uebersichtPage || uebersichtPage.AusgewaehltesProjekt is not Projekt ausgewaehlt)
                return;

            var result = MessageBox.Show(
                $"Möchten Sie das Projekt '{ausgewaehlt.Name}' wirklich löschen?",
                "Projekt löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _viewModel.Projekte.Remove(ausgewaehlt);
            ProjektSpeicher.Speichern(AlleProjekte);
            ZeigeUebersicht();
        }

        public void UpdateProjektBearbeitenUndLoeschenButtons()
        {
            if (MainContent.Content is not ProjektUebersichtPage uebersichtPage) return;

            bool hatAuswahl = uebersichtPage.AusgewaehltesProjekt != null;
            ProjektBearbeitenButton.IsEnabled = hatAuswahl;
            ProjektLoeschenButton.IsEnabled = hatAuswahl;
        }
    }
}
