using ProjektManager.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ProjektManager.Views
{
    public partial class SpaltenZuordnungWindow : Window
    {
        private record SpaltenOption(int Index, string Anzeige);

        public ExcelSpaltenZuordnung Zuordnung { get; private set; }

        public SpaltenZuordnungWindow(string[] kopfzeile, ExcelSpaltenZuordnung aktuelleZuordnung)
        {
            InitializeComponent();
            Zuordnung = aktuelleZuordnung.Kopie();

            var optionen = new List<SpaltenOption> { new(-1, "(keine)") };
            for (int i = 0; i < kopfzeile.Length; i++)
            {
                var titel = string.IsNullOrWhiteSpace(kopfzeile[i]) ? $"Spalte {i + 1}" : $"Spalte {i + 1}: {kopfzeile[i]}";
                optionen.Add(new SpaltenOption(i, titel));
            }

            foreach (var box in new[] { KmVonBox, KmBisBox, BahnseiteBox, BeschreibungBox, AnmerkungBox, LaengeMeterBox, Anmerkung2ABox, Anmerkung2BBox })
                box.ItemsSource = optionen;

            KmVonBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.KmVon) ?? optionen[0];
            KmBisBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.KmBis) ?? optionen[0];
            BahnseiteBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.Bahnseite) ?? optionen[0];
            BeschreibungBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.Beschreibung) ?? optionen[0];
            AnmerkungBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.Anmerkung) ?? optionen[0];
            LaengeMeterBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.LaengeMeter) ?? optionen[0];
            Anmerkung2ABox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.Anmerkung2A) ?? optionen[0];
            Anmerkung2BBox.SelectedItem = optionen.FirstOrDefault(o => o.Index == Zuordnung.Anmerkung2B) ?? optionen[0];
        }

        private static int AusgewaehlterIndex(ComboBox box) => (box.SelectedItem as SpaltenOption)?.Index ?? -1;

        private void Uebernehmen_Click(object sender, RoutedEventArgs e)
        {
            Zuordnung = new ExcelSpaltenZuordnung
            {
                KmVon = AusgewaehlterIndex(KmVonBox),
                KmBis = AusgewaehlterIndex(KmBisBox),
                Bahnseite = AusgewaehlterIndex(BahnseiteBox),
                Beschreibung = AusgewaehlterIndex(BeschreibungBox),
                Anmerkung = AusgewaehlterIndex(AnmerkungBox),
                LaengeMeter = AusgewaehlterIndex(LaengeMeterBox),
                Anmerkung2A = AusgewaehlterIndex(Anmerkung2ABox),
                Anmerkung2B = AusgewaehlterIndex(Anmerkung2BBox)
            };

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
