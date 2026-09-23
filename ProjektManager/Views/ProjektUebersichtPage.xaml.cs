using ProjektManager.Models;
using ProjektManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class ProjektUebersichtPage : UserControl
    {
        public ProjektUebersichtPage(MainWindow main, List<Projekt> projekte)
        {
            InitializeComponent();

            DataContext = new ProjektUebersichtViewModel(projekte, projekt =>
            {
                main.AktuellesProjekt = projekt;
                main.ZeigeSeite(new ProjektVisualisierungPage(main, projekt));
            });
        }

        private void Karte_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: ProjektKarteViewModel karte })
            {
                karte.OeffnenCommand.Execute(null);
            }
        }
    }
}
