using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.ViewModels
{
    /// <summary>Ein Länge-Block auf der Projekt-Visualisierungsseite.</summary>
    public class LaengeBlockViewModel
    {
        public Laenge Laenge { get; }
        public string Bezeichnung => Laenge.Bezeichnung;
        public Brush Hintergrund => Laenge.KabelVerlegt ? VisualBuilder.Success : VisualBuilder.Accent;
        public ICommand OeffnenCommand { get; }

        public LaengeBlockViewModel(Laenge laenge, Action<Laenge> oeffnen)
        {
            Laenge = laenge;
            OeffnenCommand = new RelayCommand(() => oeffnen(laenge));
        }
    }
}
