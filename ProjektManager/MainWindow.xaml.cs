using ProjektManager.Models;
using ProjektManager.ViewModels;
using ProjektManager.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjektManager.Data;
using Microsoft.Win32;
using ProjektManager.Helpers;
using Newtonsoft.Json;
using System.IO;

namespace ProjektManager
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private Stack<UserControl> _pageHistory = new();
        public MainViewModel ViewModel => _viewModel;


        public MainWindow()
        {
            InitializeComponent();
            // OneDrive klasörleri otomatik oluşturulsun

            ProjektPfadHelper.StelleVerzeichnisseSicher("LST_Projekte");
            ProjektPfadHelper.StelleVerzeichnisseSicher("LWL_Projekte");


            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            ZeigeSeite(new ProjektUebersichtPage(this, _viewModel.Projekte.ToList()));


        }

        public Projekt AktuellesProjekt { get; set; }
        public List<Projekt> AlleProjekte => _viewModel.Projekte.ToList();
        public void ZeigeSeite(UserControl seite)
        {
            if (MainContent.Content != null)
                _pageHistory.Push((UserControl)MainContent.Content);

            MainContent.Content = seite;
            BackButton.Visibility = _pageHistory.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OeffneLSTVisualisierung_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel-Dateien (*.xlsx)|*.xlsx";

            if (dialog.ShowDialog() == true)
            {
                var lstKabelListe = ExcelReader.LeseLSTKabel(dialog.FileName);
                var lstPage = new LSTVisualisierungPage(lstKabelListe);
                MainContent.Content = lstPage;
            }
        }

        private List<Projekt> LadeAlleLWLProjekte()
        {
            var liste = new List<Projekt>();

            List<string> namen = new();
            bool indexAusLegacy = false;

            if (File.Exists(ProjektPfadHelper.LWL_IndexDatei))
            {
                namen = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ProjektPfadHelper.LWL_IndexDatei)) ?? new();
            }

            if (namen.Count == 0)
            {
                if (File.Exists(ProjektPfadHelper.LegacyLWLIndexDatei))
                {
                    namen = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ProjektPfadHelper.LegacyLWLIndexDatei)) ?? new();
                    indexAusLegacy = namen.Count > 0;
                }
            }

            var eindeutigeNamen = namen.Distinct().ToList();
            bool dateienAusLegacy = false;

            foreach (var name in eindeutigeNamen)
            {
                string pfad = Path.Combine(ProjektPfadHelper.LWL_Projekte_Ordner, name + ".json");
                Projekt? projekt = null;

                if (File.Exists(pfad))
                {
                    projekt = JsonConvert.DeserializeObject<Projekt>(File.ReadAllText(pfad));
                }
                else
                {
                    string legacyPfad = ProjektPfadHelper.LegacyProjektDatei("LWL_Projekte", name);
                    if (File.Exists(legacyPfad))
                    {
                        var json = File.ReadAllText(legacyPfad);
                        projekt = JsonConvert.DeserializeObject<Projekt>(json);
                        if (projekt != null)
                        {
                            Directory.CreateDirectory(ProjektPfadHelper.LWLProjektOrdner);
                            File.WriteAllText(pfad, json);
                            ProjektPfadHelper.TryDeleteLegacyFile(legacyPfad);
                            dateienAusLegacy = true;
                        }
                    }
                }

                if (projekt != null)
                {
                    liste.Add(projekt);
                }
            }

            if ((indexAusLegacy || dateienAusLegacy) && eindeutigeNamen.Count > 0)
            {
                Directory.CreateDirectory(ProjektPfadHelper.LWLProjektOrdner);
                File.WriteAllText(ProjektPfadHelper.LWLIndexDatei,
                    JsonConvert.SerializeObject(eindeutigeNamen, Formatting.Indented));
                ProjektPfadHelper.TryDeleteLegacyFile(ProjektPfadHelper.LegacyLWLIndexDatei);
            }

            return liste;
        }

        private void LSTImportieren_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel-Dateien (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                var lstKabelListe = ExcelReader.LeseLSTKabel(dialog.FileName);
                if (lstKabelListe.Any())
                {
                    // 1. Visualize
                    var lstPage = new LSTVisualisierungPage(lstKabelListe);
                    ZeigeSeite(lstPage);

                    // 2. Save
                    string projektName = Path.GetFileNameWithoutExtension(dialog.FileName);

                    // 💡 OneDrive yoluna göre LST dizini
                    string ordner = ProjektPfadHelper.LSTProjektOrdner;
                    Directory.CreateDirectory(ordner); // her ihtimale karşı

                    string jsonPfad = Path.Combine(ordner, projektName + ".json");
                    string indexPfad = Path.Combine(ordner, "lst_projekte_index.json");

                    File.WriteAllText(jsonPfad, JsonConvert.SerializeObject(lstKabelListe, Formatting.Indented));
                    ProjektPfadHelper.TryDeleteLegacyFile(ProjektPfadHelper.LegacyProjektDatei("LST_Projekte", projektName));

                    // Listeye kaydet
                    List<string> projektListe = new();
                    if (File.Exists(indexPfad))
                        projektListe = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(indexPfad)) ?? new();

                    if (!projektListe.Contains(projektName))
                    {
                        projektListe.Add(projektName);
                        projektListe = projektListe.Distinct().ToList();
                        File.WriteAllText(indexPfad, JsonConvert.SerializeObject(projektListe, Formatting.Indented));
                    }

                    ProjektPfadHelper.TryDeleteLegacyFile(ProjektPfadHelper.LegacyLSTIndexDatei);
                }
                else
                {
                    MessageBox.Show("Keine gültigen Kabel gefunden.");
                }
            }
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
            if (importWindow.ShowDialog() == true)
            {
                var neuesProjekt = importWindow.ErgebnisProjekt;
                if (neuesProjekt != null)
                {
                    neuesProjekt.Name = nameWindow.ProjektName;
                    _viewModel.Projekte.Add(neuesProjekt);

                    ProjektSpeicher.Speichern(_viewModel.Projekte.ToList());
                }
            }
        }

        private void ProjektBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is ProjektUebersichtPage uebersichtPage && uebersichtPage.AusgewaehltesProjekt is Projekt ausgewaehlt)
            {
                var nameWindow = new ProjektNameEingabeWindow(ausgewaehlt.Name);
                if (nameWindow.ShowDialog() != true) return;

                // Excel import ekranı
                var importWindow = new ProjektImportWindow(ausgewaehlt);
                if (importWindow.ShowDialog() == true)
                {
                    // İsim güncellemesi
                    ausgewaehlt.Name = nameWindow.ProjektName;

                    // Yeni verilerle Länge listesini güncelle
                    if (importWindow.ErgebnisProjekt != null && importWindow.ErgebnisProjekt.Laengen != null)
                    {
                        ausgewaehlt.Laengen = importWindow.ErgebnisProjekt.Laengen;
                        ausgewaehlt.ProjektPfad = importWindow.ErgebnisProjekt.ProjektPfad;
                    }

                    ProjektSpeicher.Speichern(AlleProjekte);
                    ZeigeSeite(new ProjektUebersichtPage(this, AlleProjekte));
                }
            }
        }



        private void ProjektLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is ProjektUebersichtPage uebersichtPage && uebersichtPage.AusgewaehltesProjekt is Projekt ausgewaehlt)
            {
                var result = MessageBox.Show($"Möchten Sie das Projekt '{ausgewaehlt.Name}' wirklich löschen?", "Projekt löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.Projekte.Remove(ausgewaehlt);
                    ProjektSpeicher.Speichern(AlleProjekte);
                    ZeigeSeite(new ProjektUebersichtPage(this, AlleProjekte));
                }
            }
        }


        public void UpdateProjektBearbeitenUndLoeschenButtons()
        {
            // Ana sayfada mı? Kontrol et
            if (MainContent.Content is ProjektUebersichtPage uebersichtPage)
            {
                var projekt = uebersichtPage.AusgewaehltesProjekt;

                // Null kontrolü: butonlar sadece bir şey seçildiyse aktifleşsin
                var bearbeitenButton = LogicalTreeHelper.FindLogicalNode(this, "ProjektBearbeitenButton") as Button;
                var loeschenButton = LogicalTreeHelper.FindLogicalNode(this, "ProjektLoeschenButton") as Button;

                if (bearbeitenButton != null)
                    bearbeitenButton.IsEnabled = projekt != null;

                if (loeschenButton != null)
                    loeschenButton.IsEnabled = projekt != null;
            }
        }

    }
}
