using ProjektManager.Models;
using System.Windows;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class ProjektAuswahlWindow : Window
    {
        public Projekt? AusgewaehltesProjekt { get; private set; }

        public ProjektAuswahlWindow(List<Projekt> projekte, string ueberschrift)
        {
            InitializeComponent();

            UeberschriftText.Text = ueberschrift;
            ProjektListe.ItemsSource = projekte;

            if (projekte.Count > 0)
                ProjektListe.SelectedIndex = 0;
        }

        private void Auswaehlen_Click(object sender, RoutedEventArgs e)
        {
            if (ProjektListe.SelectedItem is not Projekt projekt)
            {
                MessageBox.Show("Bitte wählen Sie ein Projekt aus.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AusgewaehltesProjekt = projekt;
            DialogResult = true;
            Close();
        }

        private void ProjektListe_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProjektListe.SelectedItem is not Projekt projekt) return;

            AusgewaehltesProjekt = projekt;
            DialogResult = true;
            Close();
        }

        private void Abbrechen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
