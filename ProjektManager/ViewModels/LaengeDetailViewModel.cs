using ProjektManager.Models;
using ProjektManager.Views;
using ProjektManager.Views.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.ViewModels
{
    public class LaengeDetailViewModel : ViewModelBase
    {
        private readonly MainWindow _main;
        private readonly Laenge _laenge;
        private readonly Func<Window?> _ownerAbrufen;
        private readonly List<Leistung> _ausgewaehlt = new();

        private bool _nurErledigt;
        private bool _nurAbgerechnet;
        private double _gesamtBreite;
        private FrameworkElement _zusammenfassungInhalt = new StackPanel();

        public string Titel { get; }
        public ObservableCollection<LeistungBlockViewModel> Bloecke { get; } = new();
        public ObservableCollection<KmMarkerViewModel> KmMarker { get; } = new();

        public double GesamtBreite
        {
            get => _gesamtBreite;
            private set => SetField(ref _gesamtBreite, value);
        }

        public FrameworkElement ZusammenfassungInhalt
        {
            get => _zusammenfassungInhalt;
            private set => SetField(ref _zusammenfassungInhalt, value);
        }

        public bool NurErledigt
        {
            get => _nurErledigt;
            set { if (SetField(ref _nurErledigt, value)) AktualisiereAlles(); }
        }

        public bool NurAbgerechnet
        {
            get => _nurAbgerechnet;
            set { if (SetField(ref _nurAbgerechnet, value)) AktualisiereAlles(); }
        }

        public bool KabelVerlegt
        {
            get => _laenge.KabelVerlegt;
            set
            {
                if (_laenge.KabelVerlegt == value) return;
                _laenge.KabelVerlegt = value;
                _main.SpeichernUndBestaetigen();
            }
        }

        public ICommand VorherigeCommand { get; }
        public ICommand NaechsteCommand { get; }

        public LaengeDetailViewModel(MainWindow main, Laenge laenge, Func<Window?> ownerAbrufen)
        {
            _main = main;
            _laenge = laenge;
            _ownerAbrufen = ownerAbrufen;

            double kmVon = laenge.Leistungen.FirstOrDefault()?.KmVon ?? 0;
            double kmBis = laenge.Leistungen.LastOrDefault()?.KmBis ?? 0;
            Titel = $"Länge: {laenge.Bezeichnung} (km {kmVon:F3} – {kmBis:F3})";

            VorherigeCommand = new RelayCommand(() => WechselZuLaenge(-1));
            NaechsteCommand = new RelayCommand(() => WechselZuLaenge(1));

            AktualisiereAlles();
        }

        public bool IstAusgewaehlt(Leistung leistung) => _ausgewaehlt.Contains(leistung);

        public int AuswahlAnzahlFuer(Leistung leistung) => _ausgewaehlt.Contains(leistung) ? _ausgewaehlt.Count : 1;

        private List<Leistung> AuswahlFuer(Leistung leistung) =>
            _ausgewaehlt.Contains(leistung) ? _ausgewaehlt : new List<Leistung> { leistung };

        public void LeistungAuswaehlen(Leistung leistung, bool strgGedrueckt)
        {
            if (strgGedrueckt)
            {
                if (!_ausgewaehlt.Remove(leistung))
                    _ausgewaehlt.Add(leistung);
            }
            else
            {
                _ausgewaehlt.Clear();
                _ausgewaehlt.Add(leistung);
            }

            AktualisiereAlles();
        }

        private void WechselZuLaenge(int richtung)
        {
            var projekt = _main.AktuellesProjekt;
            if (projekt == null) return;

            int index = projekt.Laengen.IndexOf(_laenge);
            int neuerIndex = index + richtung;
            if (index < 0 || neuerIndex < 0 || neuerIndex >= projekt.Laengen.Count) return;

            _main.ZeigeSeite(new LaengeDetailPage(_main, projekt.Laengen[neuerIndex]));
        }

        public void Bearbeiten(Leistung leistung)
        {
            var (beschreibungen, bahnseiten) = SammleVorschlaege();
            var dialog = new LeistungBearbeitenWindow(leistung, beschreibungen, bahnseiten) { Owner = _ownerAbrufen() };
            if (dialog.ShowDialog() != true) return;

            double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
            double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);

            if (leistung.KmVon < laengeMin || leistung.KmBis > laengeMax)
            {
                MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ÜberprüfeUndAktualisiereLeistung(leistung);
        }

        public void NotizBearbeiten(Leistung leistung)
        {
            var dialog = new NotizEingabeWindow(leistung.Notiz) { Owner = _ownerAbrufen() };
            if (dialog.ShowDialog() != true) return;

            leistung.Notiz = dialog.EingetrageneNotiz;
            _main.SpeichernUndBestaetigen();
            AktualisiereAlles();
        }

        public void ErledigtUmschalten(Leistung leistung)
        {
            foreach (var l in AuswahlFuer(leistung)) l.IstFertiggestellt = !l.IstFertiggestellt;
            _main.SpeichernUndBestaetigen();
            AktualisiereAlles();
        }

        public void Abrechnen(Leistung leistung)
        {
            var dialog = new AufmassNummerWindow(leistung.AufmassNummer) { Owner = _ownerAbrufen() };
            if (dialog.ShowDialog() != true) return;

            foreach (var l in AuswahlFuer(leistung))
            {
                l.IstAbgerechnet = true;
                l.AufmassNummer = dialog.AufmassNummer;
            }
            _main.SpeichernUndBestaetigen();
            AktualisiereAlles();
        }

        public void AbrechnungZuruecksetzen(Leistung leistung)
        {
            foreach (var l in AuswahlFuer(leistung))
            {
                l.IstAbgerechnet = false;
                l.AufmassNummer = null;
            }
            _main.SpeichernUndBestaetigen();
            AktualisiereAlles();
        }

        private void ÜberprüfeUndAktualisiereLeistung(Leistung geaenderteLeistung)
        {
            var andereLeistungen = _laenge.Leistungen.Where(l => l != geaenderteLeistung).ToList();

            double laengeMin = _laenge.Leistungen.Min(l => l.KmVon);
            double laengeMax = _laenge.Leistungen.Max(l => l.KmBis);

            if (geaenderteLeistung.KmVon < laengeMin || geaenderteLeistung.KmBis > laengeMax)
            {
                MessageBox.Show($"Die Werte dürfen den Bereich der Länge nicht überschreiten ({laengeMin:F3} – {laengeMax:F3} km).", "Ungültiger Bereich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            geaenderteLeistung.LaengeMeter = (geaenderteLeistung.KmBis - geaenderteLeistung.KmVon) * 1000;

            foreach (var block in andereLeistungen.ToList())
            {
                bool ueberschneidung = block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmVon;
                if (!ueberschneidung) continue;

                if (block.KmVon >= geaenderteLeistung.KmVon && block.KmBis <= geaenderteLeistung.KmBis)
                {
                    _laenge.Leistungen.Remove(block);
                }
                else if (block.KmVon < geaenderteLeistung.KmVon && block.KmBis > geaenderteLeistung.KmVon)
                {
                    block.KmBis = geaenderteLeistung.KmVon;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
                else if (block.KmVon < geaenderteLeistung.KmBis && block.KmBis > geaenderteLeistung.KmBis)
                {
                    block.KmVon = geaenderteLeistung.KmBis;
                    block.LaengeMeter = (block.KmBis - block.KmVon) * 1000;
                }
            }

            _laenge.Leistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();

            int index = _laenge.Leistungen.IndexOf(geaenderteLeistung);
            if (index > 0)
            {
                var vorheriger = _laenge.Leistungen[index - 1];
                if (Math.Abs(geaenderteLeistung.KmVon - vorheriger.KmBis) > 0.001)
                {
                    _laenge.Leistungen.Add(new Leistung
                    {
                        KmVon = vorheriger.KmBis,
                        KmBis = geaenderteLeistung.KmVon,
                        Leistungsbeschreibung = "Leer",
                        LaengeMeter = (geaenderteLeistung.KmVon - vorheriger.KmBis) * 1000
                    });
                }
            }

            if (index < _laenge.Leistungen.Count - 1)
            {
                var nachfolgend = _laenge.Leistungen[index + 1];
                if (Math.Abs(nachfolgend.KmVon - geaenderteLeistung.KmBis) > 0.001)
                {
                    _laenge.Leistungen.Add(new Leistung
                    {
                        KmVon = geaenderteLeistung.KmBis,
                        KmBis = nachfolgend.KmVon,
                        Leistungsbeschreibung = "Leer",
                        LaengeMeter = (nachfolgend.KmVon - geaenderteLeistung.KmBis) * 1000
                    });
                }
            }

            _laenge.Leistungen = _laenge.Leistungen.OrderBy(l => l.KmVon).ToList();
            _main.SpeichernUndBestaetigen();
            AktualisiereAlles();
        }

        private void AktualisiereAlles()
        {
            const double fontSize = 12;
            const double padding = 20;
            const double margin = 3;

            IEnumerable<Leistung> leistungen = _laenge.Leistungen;
            if (NurErledigt) leistungen = leistungen.Where(l => l.IstFertiggestellt);
            if (NurAbgerechnet) leistungen = leistungen.Where(l => l.IstAbgerechnet);

            var liste = leistungen.ToList();
            foreach (var l in liste) l.LaengeMeter = (l.KmBis - l.KmVon) * 1000;

            var minWidths = liste
                .Select(l => new FormattedText(
                    l.Leistungsbeschreibung,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1).Width + padding)
                .ToList();

            double toplamLaenge = liste.Sum(l => l.LaengeMeter ?? 0);
            bool hepsiSifir = liste.All(l => (l.LaengeMeter ?? 0) <= 0);
            double kalanAlan = Math.Max(0, 1000 - minWidths.Sum());
            double currentOffset = 0;

            Bloecke.Clear();
            KmMarker.Clear();

            for (int i = 0; i < liste.Count; i++)
            {
                var l = liste[i];
                double metraj = (l.KmBis - l.KmVon) * 1000;

                double ekAlan = (toplamLaenge > 0 && !hepsiSifir) ? (metraj / toplamLaenge) * kalanAlan : 0;
                double toplamGenislik = minWidths[i] + ekAlan;
                double disGenislik = toplamGenislik + margin * 2;

                Bloecke.Add(new LeistungBlockViewModel(l, toplamGenislik, this));

                if (i == 0)
                    KmMarker.Add(new KmMarkerViewModel($"{l.KmVon:F3} km", currentOffset + margin + 15));

                KmMarker.Add(new KmMarkerViewModel($"{l.KmBis:F3} km", currentOffset + disGenislik + margin + 15));

                currentOffset += disGenislik;
            }

            GesamtBreite = currentOffset + 40;
            ZusammenfassungInhalt = BaueZusammenfassung();
        }

        private FrameworkElement BaueZusammenfassung()
        {
            var wurzel = new StackPanel();

            var kabelBox = new CheckBox
            {
                Content = "Kabel verlegt",
                IsChecked = _laenge.KabelVerlegt,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            };
            kabelBox.Checked += (s, e) => KabelVerlegt = true;
            kabelBox.Unchecked += (s, e) => KabelVerlegt = false;
            wurzel.Children.Add(kabelBox);

            wurzel.Children.Add(VisualBuilder.ErzeugeZusammenfassung(_laenge.Leistungen));
            return wurzel;
        }

        /// <summary>
        /// Sammelt projektübergreifend bereits verwendete Leistungsbeschreibungen und Bahnseiten
        /// (häufigste zuerst) als Vorschläge für die Autovervollständigung im Bearbeiten-Dialog.
        /// </summary>
        private (List<string> Beschreibungen, List<string> Bahnseiten) SammleVorschlaege()
        {
            var alleLeistungen = _main.AlleProjekte.SelectMany(p => p.Laengen).SelectMany(l => l.Leistungen).ToList();

            List<string> HaeufigsteZuerst(Func<Leistung, string> auswahl) => alleLeistungen
                .Select(auswahl)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToList();

            return (HaeufigsteZuerst(l => l.Leistungsbeschreibung), HaeufigsteZuerst(l => l.Bahnseite));
        }
    }
}
