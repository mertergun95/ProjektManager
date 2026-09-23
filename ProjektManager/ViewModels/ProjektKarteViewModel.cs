using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjektManager.ViewModels
{
    /// <summary>Eine einzelne Projektkarte in der Übersicht: Name, Länge-Anzahl und Mini-Donuts.</summary>
    public class ProjektKarteViewModel : ViewModelBase
    {
        public Projekt Projekt { get; }
        public string Name => Projekt.Name;
        public string LaengenInfo => $"{Projekt.Laengen.Count} Länge(n)";
        public FrameworkElement DonutVisual { get; }
        public ICommand OeffnenCommand { get; }

        public ProjektKarteViewModel(Projekt projekt, Action<Projekt> oeffnen)
        {
            Projekt = projekt;

            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            double gesamteMeter = alleLeistungen.Sum(x => x.LaengeMeter ?? 0);
            double erledigtMeter = alleLeistungen.Where(x => x.IstFertiggestellt).Sum(x => x.LaengeMeter ?? 0);
            double abgerechnetMeter = alleLeistungen.Where(x => x.IstAbgerechnet).Sum(x => x.LaengeMeter ?? 0);

            double erledigtProzent = gesamteMeter > 0 ? erledigtMeter / gesamteMeter * 100 : 0;
            double abgerechnetProzent = gesamteMeter > 0 ? abgerechnetMeter / gesamteMeter * 100 : 0;

            var donuts = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            donuts.Children.Add(VisualBuilder.ErzeugeDonut(erledigtProzent, null, VisualBuilder.Success, radius: 28, strokeDicke: 7));
            donuts.Children.Add(VisualBuilder.ErzeugeDonut(abgerechnetProzent, null, VisualBuilder.Accent, radius: 28, strokeDicke: 7));
            DonutVisual = donuts;

            OeffnenCommand = new RelayCommand(() => oeffnen(projekt));
        }
    }
}
