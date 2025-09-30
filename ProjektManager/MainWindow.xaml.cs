using ProjektManager.Models;
using ProjektManager.ViewModels;
using ProjektManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjektManager.Data;
using Microsoft.Win32;
using ProjektManager.Helpers;
using Newtonsoft.Json;
using System.IO;
using ProjektManager.Helpers;

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

            if (!File.Exists(ProjektPfadHelper.LWL_IndexDatei))
                return liste;

            var namen = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ProjektPfadHelper.LWL_IndexDatei)) ?? new();
            foreach (var name in namen)
            {
                string pfad = System.IO.Path.Combine(ProjektPfadHelper.LWL_Projekte_Ordner, name + ".json");
                if (File.Exists(pfad))
                {
                    var projekt = JsonConvert.DeserializeObject<Projekt>(File.ReadAllText(pfad));
                    if (projekt != null)
                        liste.Add(projekt);
                }
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

                    // Listeye kaydet
                    List<string> projektListe = new();
                    if (File.Exists(indexPfad))
                        projektListe = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(indexPfad)) ?? new();

                    if (!projektListe.Contains(projektName))
                    {
                        projektListe.Add(projektName);
                        File.WriteAllText(indexPfad, JsonConvert.SerializeObject(projektListe, Formatting.Indented));
                    }
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
                    SpeichereLWLProjekt(neuesProjekt, null);

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
                    string alterName = ausgewaehlt.Name;

                    // İsim güncellemesi
                    ausgewaehlt.Name = nameWindow.ProjektName;

                    // Yeni verilerle Länge listesini güncelle
                    if (importWindow.ErgebnisProjekt != null && importWindow.ErgebnisProjekt.Laengen != null)
                    {
                        ausgewaehlt.Laengen = importWindow.ErgebnisProjekt.Laengen;
                        ausgewaehlt.ProjektPfad = importWindow.ErgebnisProjekt.ProjektPfad;
                    }

                    SpeichereLWLProjekt(ausgewaehlt, alterName);

                    ProjektSpeicher.Speichern(AlleProjekte);
                    ZeigeSeite(new ProjektUebersichtPage(this, AlleProjekte));
                }
            }
        }

        private void SpeichereLWLProjekt(Projekt projekt, string? alterName)
        {
            if (projekt == null || string.IsNullOrWhiteSpace(projekt.Name))
                return;

            Directory.CreateDirectory(ProjektPfadHelper.LWLProjektOrdner);

            string indexPfad = ProjektPfadHelper.LWLIndexDatei;
            List<string> indexEintraege = new();

            if (File.Exists(indexPfad))
            {
                var geleseneEintraege = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(indexPfad));
                if (geleseneEintraege != null)
                    indexEintraege = geleseneEintraege;
            }

            if (!string.IsNullOrWhiteSpace(alterName) && !string.Equals(alterName, projekt.Name, StringComparison.Ordinal))
            {
                string alterPfad = Path.Combine(ProjektPfadHelper.LWLProjektOrdner, alterName + ".json");
                if (File.Exists(alterPfad))
                    File.Delete(alterPfad);

                indexEintraege.RemoveAll(name => string.Equals(name, alterName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(projekt.ProjektPfad))
                {
                    string excelBasierterName = Path.GetFileNameWithoutExtension(projekt.ProjektPfad);
                    if (!string.IsNullOrWhiteSpace(excelBasierterName) &&
                        !string.Equals(excelBasierterName, projekt.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        string excelJsonPfad = Path.Combine(ProjektPfadHelper.LWLProjektOrdner, excelBasierterName + ".json");
                        if (File.Exists(excelJsonPfad))
                            File.Delete(excelJsonPfad);

                        indexEintraege.RemoveAll(name => string.Equals(name, excelBasierterName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            string neuerPfad = Path.Combine(ProjektPfadHelper.LWLProjektOrdner, projekt.Name + ".json");
            File.WriteAllText(neuerPfad, JsonConvert.SerializeObject(projekt, Formatting.Indented));

            indexEintraege.RemoveAll(name => string.Equals(name, projekt.Name, StringComparison.OrdinalIgnoreCase));
            indexEintraege.Add(projekt.Name);

            File.WriteAllText(indexPfad, JsonConvert.SerializeObject(indexEintraege, Formatting.Indented));
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
