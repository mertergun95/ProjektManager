using System.Windows;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class AufmassNummerWindow : Window
    {
        public int? AufmassNummer { get; private set; }

        public AufmassNummerWindow(int? vorhandeneNummer = null)
        {
            InitializeComponent();

            if (vorhandeneNummer.HasValue)
                NummerBox.Text = vorhandeneNummer.Value.ToString();

            NummerBox.Focus();
            NummerBox.SelectAll();
        }

        private void NummerBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void Uebernehmen_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NummerBox.Text, out int nummer) || nummer <= 0)
            {
                MessageBox.Show("Bitte geben Sie eine gültige Aufmaß-Nummer ein.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AufmassNummer = nummer;
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
