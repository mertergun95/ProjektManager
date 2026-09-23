using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.Views
{
    public partial class ProjektUebersichtPage : UserControl
    {
        private readonly MainWindow _main;
        private readonly List<Projekt> _projekte;

        public ProjektUebersichtPage(MainWindow main, List<Projekt> projekte)
        {
            InitializeComponent();
            _main = main;
            _projekte = projekte;

            ErzeugeGesamtuebersicht();
            ErzeugeProjektKarten(_projekte);
        }

        private void SucheBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string suchtext = SucheBox.Text.Trim();

            var gefiltert = string.IsNullOrEmpty(suchtext)
                ? _projekte
                : _projekte.Where(p => p.Name.Contains(suchtext, StringComparison.OrdinalIgnoreCase)).ToList();

            ErzeugeProjektKarten(gefiltert);
        }

        private void ErzeugeGesamtuebersicht()
        {
            GesamtDonutPanel.Children.Clear();

            if (_projekte.Count == 0)
            {
                GesamtuebersichtCard.Visibility = Visibility.Collapsed;
                return;
            }

            var alleLeistungen = _projekte.SelectMany(p => p.Laengen).SelectMany(l => l.Leistungen).ToList();
            double gesamtMeter = alleLeistungen.Sum(l => l.LaengeMeter ?? 0);
            double erledigtMeter = alleLeistungen.Where(l => l.IstFertiggestellt).Sum(l => l.LaengeMeter ?? 0);
            double abgerechnetMeter = alleLeistungen.Where(l => l.IstAbgerechnet).Sum(l => l.LaengeMeter ?? 0);

            double erledigtProzent = gesamtMeter > 0 ? erledigtMeter / gesamtMeter * 100 : 0;
            double abgerechnetProzent = gesamtMeter > 0 ? abgerechnetMeter / gesamtMeter * 100 : 0;

            GesamtDonutPanel.Children.Add(VisualBuilder.ErzeugeDonut(erledigtProzent, "Erledigt", VisualBuilder.Success));
            GesamtDonutPanel.Children.Add(VisualBuilder.ErzeugeDonut(abgerechnetProzent, "Abgerechnet", VisualBuilder.Accent));

            int laengenAnzahl = _projekte.Sum(p => p.Laengen.Count);
            GesamtStatistikText.Text =
                $"{_projekte.Count} Projekt(e) · {laengenAnzahl} Länge(n) · {gesamtMeter:N0} m gesamt\n" +
                $"{erledigtMeter:N0} m erledigt · {abgerechnetMeter:N0} m abgerechnet";
        }

        private void ErzeugeProjektKarten(List<Projekt> projekte)
        {
            ProjektCardPanel.Children.Clear();
            LeerHinweisText.Visibility = _projekte.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach (var projekt in projekte)
            {
                ProjektCardPanel.Children.Add(ErzeugeProjektCard(projekt));
            }
        }

        private Border ErzeugeProjektCard(Projekt projekt)
        {
            var alleLeistungen = projekt.Laengen.SelectMany(l => l.Leistungen).ToList();
            double gesamteMeter = alleLeistungen.Sum(x => x.LaengeMeter ?? 0);
            double erledigtMeter = alleLeistungen.Where(x => x.IstFertiggestellt).Sum(x => x.LaengeMeter ?? 0);
            double abgerechnetMeter = alleLeistungen.Where(x => x.IstAbgerechnet).Sum(x => x.LaengeMeter ?? 0);

            double erledigtProzent = gesamteMeter > 0 ? (erledigtMeter / gesamteMeter) * 100 : 0;
            double abgerechnetProzent = gesamteMeter > 0 ? (abgerechnetMeter / gesamteMeter) * 100 : 0;

            var titel = new TextBlock
            {
                Text = projekt.Name,
                Style = (Style)Application.Current.Resources["SectionTitleStyle"],
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var laengeInfo = new TextBlock
            {
                Text = $"{projekt.Laengen.Count} Länge(n)",
                Style = (Style)Application.Current.Resources["MutedTextStyle"],
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var donuts = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            donuts.Children.Add(VisualBuilder.ErzeugeDonut(erledigtProzent, null, VisualBuilder.Success, radius: 28, strokeDicke: 7));
            donuts.Children.Add(VisualBuilder.ErzeugeDonut(abgerechnetProzent, null, VisualBuilder.Accent, radius: 28, strokeDicke: 7));

            var inhalt = new StackPanel();
            inhalt.Children.Add(titel);
            inhalt.Children.Add(laengeInfo);
            inhalt.Children.Add(donuts);

            var karte = new Border
            {
                Style = (Style)Application.Current.Resources["CardStyle"],
                Margin = new Thickness(0, 0, 16, 16),
                Width = 260,
                Child = inhalt,
                Cursor = Cursors.Hand,
                ToolTip = "Doppelklick zum Öffnen"
            };

            karte.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount != 2) return;
                _main.AktuellesProjekt = projekt;
                _main.ZeigeSeite(new ProjektVisualisierungPage(_main, projekt));
            };

            return karte;
        }
    }
}
