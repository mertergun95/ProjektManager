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

        public Projekt? AusgewaehltesProjekt { get; private set; }

        public ProjektUebersichtPage(MainWindow main, List<Projekt> projekte)
        {
            InitializeComponent();
            _main = main;
            _projekte = projekte;

            ErzeugeProjektKarten();
        }

        private void ErzeugeProjektKarten()
        {
            ProjektCardPanel.Children.Clear();
            LeerHinweisText.Visibility = _projekte.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach (var projekt in _projekte)
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

            bool istAusgewaehlt = projekt == AusgewaehltesProjekt;

            var karte = new Border
            {
                Style = (Style)Application.Current.Resources["CardStyle"],
                BorderBrush = istAusgewaehlt ? VisualBuilder.Accent : VisualBuilder.Border,
                BorderThickness = new Thickness(istAusgewaehlt ? 2 : 1),
                Margin = new Thickness(0, 0, 16, 16),
                Width = 260,
                Child = inhalt,
                Cursor = Cursors.Hand
            };

            karte.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    _main.AktuellesProjekt = projekt;
                    _main.ZeigeSeite(new ProjektVisualisierungPage(_main, projekt));
                    return;
                }

                AusgewaehltesProjekt = projekt;
                _main.AktuellesProjekt = projekt;
                ErzeugeProjektKarten();
                _main.UpdateProjektBearbeitenUndLoeschenButtons();
            };

            return karte;
        }
    }
}
