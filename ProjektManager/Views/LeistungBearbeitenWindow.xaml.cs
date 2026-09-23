using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Globalization;
using System.Windows;

namespace ProjektManager.Views
{
    public partial class LeistungBearbeitenWindow : Window
    {
        private readonly Leistung _leistung;

        public LeistungBearbeitenWindow(Leistung leistung, IEnumerable<string>? beschreibungsVorschlaege = null, IEnumerable<string>? bahnseiteVorschlaege = null)
        {
            InitializeComponent();
            _leistung = leistung;

            BeschreibungBox.Text = leistung.Leistungsbeschreibung;
            BahnseiteBox.Text = leistung.Bahnseite;
            KmVonBox.Text = leistung.KmVon.ToString("F3", CultureInfo.InvariantCulture);
            KmBisBox.Text = leistung.KmBis.ToString("F3", CultureInfo.InvariantCulture);
            AnmerkungBox.Text = leistung.Anmerkung;
            Anmerkung2Box.Text = leistung.Anmerkung2;

            AutoComplete.Aktivieren(BeschreibungBox, beschreibungsVorschlaege ?? Enumerable.Empty<string>());
            AutoComplete.Aktivieren(BahnseiteBox, bahnseiteVorschlaege ?? Enumerable.Empty<string>());
        }

        private void Speichern_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(KmVonBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var kmVon) ||
                !double.TryParse(KmBisBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var kmBis))
            {
                ZeigeFehler("„Km von“ und „Km bis“ müssen gültige Zahlen sein (z.B. 12.345).");
                return;
            }

            if (kmBis <= kmVon)
            {
                ZeigeFehler("„Km bis“ muss größer als „Km von“ sein.");
                return;
            }

            _leistung.Leistungsbeschreibung = BeschreibungBox.Text.Trim();
            _leistung.Bahnseite = BahnseiteBox.Text.Trim();
            _leistung.KmVon = kmVon;
            _leistung.KmBis = kmBis;
            _leistung.Anmerkung = AnmerkungBox.Text.Trim();
            _leistung.Anmerkung2 = Anmerkung2Box.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Abbrechen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ZeigeFehler(string text)
        {
            FehlerText.Text = text;
            FehlerText.Visibility = Visibility.Visible;
        }
    }
}
