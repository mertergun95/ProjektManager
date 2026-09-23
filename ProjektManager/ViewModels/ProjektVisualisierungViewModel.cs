using Microsoft.Win32;
using ProjektManager.Helpers;
using ProjektManager.Models;
using ProjektManager.Views;
using ProjektManager.Views.Shared;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProjektManager.ViewModels
{
    public class ProjektVisualisierungViewModel : ViewModelBase
    {
        private readonly MainWindow _main;

        public Projekt Projekt { get; }
        public string Titel { get; }
        public ObservableCollection<LaengeBlockViewModel> Laengen { get; } = new();
        public ObservableCollection<KmMarkerViewModel> KmMarker { get; } = new();
        public double GesamtBreite { get; }
        public FrameworkElement ZusammenfassungInhalt { get; }
        public ICommand ExcelExportCommand { get; }
        public ICommand BerichtDruckenCommand { get; }

        public ProjektVisualisierungViewModel(MainWindow main, Projekt projekt)
        {
            _main = main;
            Projekt = projekt;
            Titel = $"Projekt: {projekt.Name}";

            ExcelExportCommand = new RelayCommand(ExcelExportieren);
            BerichtDruckenCommand = new RelayCommand(BerichtDrucken);

            const double fixedWidth = 180;
            const double blockMargin = 2;
            double currentOffset = 20;

            for (int i = 0; i < projekt.Laengen.Count; i++)
            {
                var laenge = projekt.Laengen[i];

                Laengen.Add(new LaengeBlockViewModel(laenge, gewaehlteLaenge =>
                {
                    main.AktuellesProjekt = projekt;
                    main.ZeigeSeite(new LaengeDetailPage(main, gewaehlteLaenge));
                }));

                if (i == 0)
                {
                    double kmVon = laenge.Leistungen.FirstOrDefault()?.KmVon ?? 0;
                    KmMarker.Add(new KmMarkerViewModel($"{kmVon:F3} km", currentOffset));
                }

                double kmBis = laenge.Leistungen.LastOrDefault()?.KmBis ?? 0;
                currentOffset += fixedWidth + blockMargin * 2;
                KmMarker.Add(new KmMarkerViewModel($"{kmBis:F3} km", currentOffset));
            }

            GesamtBreite = currentOffset + 40;

            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            ZusammenfassungInhalt = VisualBuilder.ErzeugeZusammenfassung(alleLeistungen);
        }

        private void ExcelExportieren()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel-Datei (*.xlsx)|*.xlsx",
                FileName = $"{Projekt.Name}_Bericht.xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                BerichtExporter.ExportiereAlsExcel(Projekt, dialog.FileName);
                _main.ZeigeStatus("Excel-Bericht gespeichert");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Exportieren:\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BerichtDrucken()
        {
            try
            {
                BerichtDrucker.DruckeProjektBericht(Projekt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Drucken:\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
