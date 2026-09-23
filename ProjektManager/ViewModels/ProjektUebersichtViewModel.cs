using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ProjektManager.ViewModels
{
    public class ProjektUebersichtViewModel : ViewModelBase
    {
        private readonly List<Projekt> _alleProjekte;
        private readonly Action<Projekt> _projektOeffnen;
        private string _suchtext = string.Empty;

        public ObservableCollection<ProjektKarteViewModel> Karten { get; } = new();

        public bool ZeigeLeerHinweis => _alleProjekte.Count == 0;
        public bool HatGesamtuebersicht => _alleProjekte.Count > 0;
        public FrameworkElement? GesamtDonuts { get; }
        public string GesamtStatistikText { get; } = string.Empty;

        public string Suchtext
        {
            get => _suchtext;
            set
            {
                if (SetField(ref _suchtext, value))
                    AktualisiereKarten();
            }
        }

        public ProjektUebersichtViewModel(List<Projekt> alleProjekte, Action<Projekt> projektOeffnen)
        {
            _alleProjekte = alleProjekte;
            _projektOeffnen = projektOeffnen;

            if (HatGesamtuebersicht)
            {
                var alleLeistungen = _alleProjekte.SelectMany(p => p.Laengen).SelectMany(l => l.Leistungen).ToList();
                double gesamtMeter = alleLeistungen.Sum(l => l.LaengeMeter ?? 0);
                double erledigtMeter = alleLeistungen.Where(l => l.IstFertiggestellt).Sum(l => l.LaengeMeter ?? 0);
                double abgerechnetMeter = alleLeistungen.Where(l => l.IstAbgerechnet).Sum(l => l.LaengeMeter ?? 0);

                double erledigtProzent = gesamtMeter > 0 ? erledigtMeter / gesamtMeter * 100 : 0;
                double abgerechnetProzent = gesamtMeter > 0 ? abgerechnetMeter / gesamtMeter * 100 : 0;

                var donuts = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                donuts.Children.Add(VisualBuilder.ErzeugeDonut(erledigtProzent, "Erledigt", VisualBuilder.Success));
                donuts.Children.Add(VisualBuilder.ErzeugeDonut(abgerechnetProzent, "Abgerechnet", VisualBuilder.Accent));
                GesamtDonuts = donuts;

                int laengenAnzahl = _alleProjekte.Sum(p => p.Laengen.Count);
                GesamtStatistikText =
                    $"{_alleProjekte.Count} Projekt(e) · {laengenAnzahl} Länge(n) · {gesamtMeter:N0} m gesamt\n" +
                    $"{erledigtMeter:N0} m erledigt · {abgerechnetMeter:N0} m abgerechnet";
            }

            AktualisiereKarten();
        }

        private void AktualisiereKarten()
        {
            var gefiltert = string.IsNullOrWhiteSpace(_suchtext)
                ? _alleProjekte
                : _alleProjekte.Where(p => p.Name.Contains(_suchtext, StringComparison.OrdinalIgnoreCase));

            Karten.Clear();
            foreach (var projekt in gefiltert)
                Karten.Add(new ProjektKarteViewModel(projekt, _projektOeffnen));
        }
    }
}
