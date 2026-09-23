using Microsoft.Win32;
using ProjektManager.Helpers;
using ProjektManager.Models;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class ProjektImportWindow : Window
    {
        private DataTable? _fullExcelData;
        private readonly List<Laenge> _importierteLaengen = new();
        private readonly Projekt? _bearbeitetesProjekt;

        public string? ExcelPfad { get; set; }
        public Projekt? ErgebnisProjekt { get; private set; }

        public ProjektImportWindow()
        {
            InitializeComponent();
        }

        public ProjektImportWindow(Projekt projekt) : this()
        {
            _bearbeitetesProjekt = projekt;
            ExcelPfad = projekt.ProjektPfad;
            DateiPfadText.Text = ExcelPfad;

            _importierteLaengen = projekt.Laengen.Select(l => new Laenge
            {
                Bezeichnung = l.Bezeichnung,
                KabelVerlegt = l.KabelVerlegt,
                Leistungen = l.Leistungen.Select(le => new Leistung
                {
                    KmVon = le.KmVon,
                    KmBis = le.KmBis,
                    Bahnseite = le.Bahnseite,
                    Leistungsbeschreibung = le.Leistungsbeschreibung,
                    Anmerkung = le.Anmerkung,
                    Anmerkung2 = le.Anmerkung2,
                    LaengeMeter = le.LaengeMeter,
                    IstFertiggestellt = le.IstFertiggestellt,
                    IstAbgerechnet = le.IstAbgerechnet,
                    Notiz = le.Notiz
                }).ToList()
            }).ToList();

            foreach (var l in _importierteLaengen)
                LaengenListe.Items.Add(l.Bezeichnung);
        }

        private void DateiWaehlen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Excel-Dateien (*.xlsx)|*.xlsx" };
            if (openFileDialog.ShowDialog() != true) return;

            ExcelPfad = openFileDialog.FileName;

            if (string.IsNullOrWhiteSpace(ExcelPfad) || !File.Exists(ExcelPfad))
            {
                MessageBox.Show("Ungültiger Dateipfad.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                DateiPfadText.Text = ExcelPfad;
                _fullExcelData = ExcelReader.LeseExcel(ExcelPfad);
                ExcelGrid.ItemsSource = _fullExcelData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Datei:\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaengeHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (_fullExcelData == null || ExcelGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show("Bitte wählen Sie mindestens eine Zeile aus.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int naechsteNummer = _importierteLaengen.Count + 1;
            string laengeName = LaengeNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(laengeName))
                laengeName = $"Länge {naechsteNummer}";

            var neueLaenge = new Laenge { Bezeichnung = laengeName };

            foreach (DataRowView selectedRow in ExcelGrid.SelectedItems)
            {
                var zeile = selectedRow.Row;
                string zusatz = zeile.Table.Columns.Count > 9 ? (zeile[9]?.ToString() ?? "") : "";

                neueLaenge.Leistungen.Add(new Leistung
                {
                    KmVon = ExcelReader.ParseDouble(zeile[1]),
                    KmBis = ExcelReader.ParseDouble(zeile[2]),
                    Bahnseite = zeile[3]?.ToString() ?? "",
                    Leistungsbeschreibung = zeile[4]?.ToString() ?? "",
                    Anmerkung = zeile[5]?.ToString() ?? "",
                    LaengeMeter = ExcelReader.ParseDouble(zeile[7]),
                    Anmerkung2 = $"{zeile[8]} {zusatz}".Trim()
                });
            }

            _importierteLaengen.Add(neueLaenge);
            LaengenListe.Items.Add(neueLaenge.Bezeichnung);
            LaengeNameBox.Text = $"Länge {naechsteNummer + 1}";

            foreach (var item in ExcelGrid.SelectedItems.Cast<DataRowView>().ToList())
                _fullExcelData.Rows.Remove(item.Row);

            ExcelGrid.ItemsSource = _fullExcelData.DefaultView;
        }

        private void LaengeLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (LaengenListe.SelectedIndex < 0)
            {
                MessageBox.Show("Bitte wählen Sie eine Länge zum Löschen aus.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _importierteLaengen.RemoveAt(LaengenListe.SelectedIndex);
            LaengenListe.Items.RemoveAt(LaengenListe.SelectedIndex);
        }

        private void LaengenListe_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LaengenListe.SelectedIndex < 0) return;

            LaengeUmbenennenBox.Text = _importierteLaengen[LaengenListe.SelectedIndex].Bezeichnung;
            LaengeUmbenennenBox.Focus();
            LaengeUmbenennenBox.SelectAll();
        }

        private void LaengeUmbenennen_Click(object sender, RoutedEventArgs e)
        {
            if (LaengenListe.SelectedIndex < 0)
            {
                MessageBox.Show("Bitte wählen Sie eine Länge zum Umbenennen.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string neuerName = LaengeUmbenennenBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(neuerName))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Namen ein.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int index = LaengenListe.SelectedIndex;
            _importierteLaengen[index].Bezeichnung = neuerName;
            LaengenListe.Items[index] = neuerName;
            LaengeUmbenennenBox.Clear();
        }

        private void LaengeNachOben_Click(object sender, RoutedEventArgs e)
        {
            int index = LaengenListe.SelectedIndex;
            if (index <= 0) return;
            VertauscheLaengen(index, index - 1);
        }

        private void LaengeNachUnten_Click(object sender, RoutedEventArgs e)
        {
            int index = LaengenListe.SelectedIndex;
            if (index < 0 || index >= _importierteLaengen.Count - 1) return;
            VertauscheLaengen(index, index + 1);
        }

        private void VertauscheLaengen(int von, int nach)
        {
            var item = _importierteLaengen[von];
            _importierteLaengen.RemoveAt(von);
            _importierteLaengen.Insert(nach, item);

            LaengenListe.Items.RemoveAt(von);
            LaengenListe.Items.Insert(nach, item.Bezeichnung);
            LaengenListe.SelectedIndex = nach;
        }

        private void Fertigstellen_Click(object sender, RoutedEventArgs e)
        {
            if (_importierteLaengen.Count == 0)
            {
                MessageBox.Show("Es wurden keine Längen importiert.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_bearbeitetesProjekt != null)
            {
                _bearbeitetesProjekt.ProjektPfad = ExcelPfad ?? _bearbeitetesProjekt.ProjektPfad;
                _bearbeitetesProjekt.Laengen = _importierteLaengen;
                ErgebnisProjekt = _bearbeitetesProjekt;
            }
            else
            {
                ErgebnisProjekt = new Projekt
                {
                    Name = "Neues Projekt",
                    ProjektPfad = ExcelPfad ?? string.Empty,
                    Laengen = _importierteLaengen
                };
            }

            DialogResult = true;
            Close();
        }
    }
}
