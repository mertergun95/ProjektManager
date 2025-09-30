using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using ProjektManager.Helpers;
using System.Collections.Generic;
using ProjektManager.Models;
using System.Data;
using System.IO;


namespace ProjektManager.Views
{
    public partial class ProjektImportWindow : Window
    {
        private DataTable _fullExcelData;
        private List<Laenge> _importierteLaengen = new List<Laenge>();
        public string ExcelPfad { get; set; }
        public Projekt ErgebnisProjekt { get; private set; }


        public ProjektImportWindow()
        {
            InitializeComponent();
        }

        private void DateiWaehlen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel-Dateien (*.xlsx)|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExcelPfad = openFileDialog.FileName;

                if (string.IsNullOrWhiteSpace(ExcelPfad) || !File.Exists(ExcelPfad))
                {
                    MessageBox.Show("Ungültiger Dateipfad.");
                    return;
                }

                try
                {
                    DateiPfadText.Text = ExcelPfad;
                    var dt = ExcelReader.LeseExcel(ExcelPfad);
                    _fullExcelData = dt;
                    ExcelGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Datei:\n" + ex.Message);
                }
            }
        }


        private void LaengeHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (ExcelGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show("Bitte wählen Sie mindestens eine Zeile aus.");
                return;
            }

            // Otomatik Länge adı
            int naechsteNummer = _importierteLaengen.Count + 1;
            string laengeName = $"Länge {naechsteNummer}";
            LaengeNameBox.Text = laengeName;

            var neueLaenge = new Laenge
            {
                Bezeichnung = laengeName,
                Leistungen = new List<Leistung>()
            };

            foreach (DataRowView selectedRow in ExcelGrid.SelectedItems)
            {
                var leistung = new Leistung
                {
                    KmVon = TryParseDouble(selectedRow.Row[1]),
                    KmBis = TryParseDouble(selectedRow.Row[2]),
                    Bahnseite = selectedRow.Row[3].ToString(),
                    Leistungsbeschreibung = selectedRow.Row[4].ToString(),
                    Anmerkung = selectedRow.Row[5].ToString(),
                    LaengeMeter = TryParseDouble(selectedRow.Row[7]),
                    Anmerkung2 = $"{selectedRow.Row[8]} {(selectedRow.Row.Table.Columns.Count > 9 ? selectedRow.Row[9]?.ToString() : "")}"

                };
                neueLaenge.Leistungen.Add(leistung);
            }

            _importierteLaengen.Add(neueLaenge);
            LaengenListe.Items.Add(neueLaenge.Bezeichnung);

            // Seçilen satırları grid'den kaldır
            foreach (var item in ExcelGrid.SelectedItems.Cast<DataRowView>().ToList())
                _fullExcelData.Rows.Remove(item.Row);

            ExcelGrid.ItemsSource = _fullExcelData.DefaultView;
        }

        private double TryParseDouble(object value)
        {
            double result;
            if (value != null && double.TryParse(value.ToString(), out result))
                return result;
            return 0;
        }
        private void Fertigstellen_Click(object sender, RoutedEventArgs e)
        {
            if (_importierteLaengen.Count == 0)
            {
                MessageBox.Show("Es wurden keine Längen importiert.");
                return;
            }

            if (_bearbeitetesProjekt != null)
            {
                _bearbeitetesProjekt.ProjektPfad = ExcelPfad;
                _bearbeitetesProjekt.Laengen = _importierteLaengen;
                ErgebnisProjekt = _bearbeitetesProjekt;
            }
            else
            {
                ErgebnisProjekt = new Projekt
                {
                    Name = "Neues Projekt",
                    ProjektPfad = ExcelPfad,
                    Laengen = _importierteLaengen
                };
            }

            MessageBox.Show($"{_importierteLaengen.Count} Längen wurden erfolgreich übernommen.");
            this.DialogResult = true;
            this.Close();
        }

        private Projekt _bearbeitetesProjekt;

        public ProjektImportWindow(Projekt projekt) : this()
        {
            _bearbeitetesProjekt = projekt;
            ExcelPfad = projekt.ProjektPfad;
            DateiPfadText.Text = ExcelPfad;

            // Eski Längen'i listeye ekle
            _importierteLaengen = projekt.Laengen.Select(l => new Laenge
            {
                Bezeichnung = l.Bezeichnung,
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

            // Liste kutusuna yaz
            foreach (var l in _importierteLaengen)
                LaengenListe.Items.Add(l.Bezeichnung);
        }

        private void LaengeLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (LaengenListe.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie eine Länge zum Löschen aus.");
                return;
            }

            var ausgewaehlterName = LaengenListe.SelectedItem.ToString();
            var zuLoeschen = _importierteLaengen.FirstOrDefault(l => l.Bezeichnung == ausgewaehlterName);
            if (zuLoeschen != null)
            {
                _importierteLaengen.Remove(zuLoeschen);
                LaengenListe.Items.Remove(LaengenListe.SelectedItem);
            }
        }

        private void LaengenListe_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LaengenListe.SelectedItem == null) return;

            var laengeName = LaengenListe.SelectedItem.ToString();
            var zuBearbeiten = _importierteLaengen.FirstOrDefault(l => l.Bezeichnung == laengeName);

            if (zuBearbeiten != null)
            {
                var dialog = new LaengeDetailWindow(zuBearbeiten); // senin zaten vardı
                if (dialog.ShowDialog() == true)
                {
                    // Veriler zaten referans ile güncellenmiş olur
                }
            }
        }
        private void LaengeUmbenennen_Click(object sender, RoutedEventArgs e)
        {
            if (LaengenListe.SelectedIndex < 0)
            {
                MessageBox.Show("Bitte wählen Sie eine Länge zum Umbenennen.");
                return;
            }

            string neuerName = LaengeUmbenennenBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(neuerName))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Namen ein.");
                return;
            }

            var laenge = _importierteLaengen[LaengenListe.SelectedIndex];
            laenge.Bezeichnung = neuerName;
            LaengenListe.Items[LaengenListe.SelectedIndex] = neuerName;
            LaengeUmbenennenBox.Clear();
        }

        private void LaengeNachOben_Click(object sender, RoutedEventArgs e)
        {
            int index = LaengenListe.SelectedIndex;
            if (index <= 0) return;

            var item = _importierteLaengen[index];
            _importierteLaengen.RemoveAt(index);
            _importierteLaengen.Insert(index - 1, item);

            LaengenListe.Items.RemoveAt(index);
            LaengenListe.Items.Insert(index - 1, item.Bezeichnung);
            LaengenListe.SelectedIndex = index - 1;
        }

        private void LaengeNachUnten_Click(object sender, RoutedEventArgs e)
        {
            int index = LaengenListe.SelectedIndex;
            if (index < 0 || index >= _importierteLaengen.Count - 1) return;

            var item = _importierteLaengen[index];
            _importierteLaengen.RemoveAt(index);
            _importierteLaengen.Insert(index + 1, item);

            LaengenListe.Items.RemoveAt(index);
            LaengenListe.Items.Insert(index + 1, item.Bezeichnung);
            LaengenListe.SelectedIndex = index + 1;
        }

    }
}
