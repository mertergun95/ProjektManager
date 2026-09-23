using ProjektManager.Models;
using ProjektManager.Views.Shared;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.ViewModels
{
    /// <summary>
    /// Ein einzelner Leistungsblock in der Länge-Detailansicht. Wird bei jeder Änderung (Filter,
    /// Auswahl, Bearbeitung) von <see cref="LaengeDetailViewModel"/> neu erzeugt statt aktualisiert
    /// – das entspricht dem bisherigen "alles neu zeichnen"-Verhalten, jetzt aber auf Ebene von
    /// leichten ViewModel-Objekten statt manuell gebauter WPF-Elemente.
    /// </summary>
    public class LeistungBlockViewModel
    {
        private readonly LaengeDetailViewModel _parent;

        public Leistung Leistung { get; }
        public double Breite { get; }
        public double Metraj { get; }

        public string Beschreibung => Leistung.Leistungsbeschreibung;
        public string MeterText => $"{(int)Metraj} m";
        public bool ZeigtMeterText => Metraj > 0;
        public Brush Hintergrund => Leistung.IstFertiggestellt ? VisualBuilder.Success : VisualBuilder.Accent;
        public bool IstAusgewaehlt => _parent.IstAusgewaehlt(Leistung);
        public Brush RandFarbe => IstAusgewaehlt ? VisualBuilder.Danger : Brushes.Transparent;
        public Thickness RandDicke => new(IstAusgewaehlt ? 2 : 0);
        public bool ZeigtAbgerechnetBadge => Leistung.IstAbgerechnet;
        public string AbgerechnetBadgeText => Leistung.AufmassNummer.HasValue ? $"€ {Leistung.AufmassNummer}" : "€";
        public bool ZeigtNotizBadge => !string.IsNullOrWhiteSpace(Leistung.Notiz);
        public string ToolTipText { get; }

        public string AbrechnenHeader
        {
            get
            {
                int anzahl = _parent.AuswahlAnzahlFuer(Leistung);
                return anzahl > 1 ? $"Als abgerechnet markieren ({anzahl} Positionen) …" : "Als abgerechnet markieren …";
            }
        }

        public ICommand BearbeitenCommand { get; }
        public ICommand NotizCommand { get; }
        public ICommand ErledigtUmschaltenCommand { get; }
        public ICommand AbrechnenCommand { get; }
        public ICommand AbrechnungZuruecksetzenCommand { get; }

        public LeistungBlockViewModel(Leistung leistung, double breite, LaengeDetailViewModel parent)
        {
            Leistung = leistung;
            Breite = breite;
            _parent = parent;
            Metraj = (leistung.KmBis - leistung.KmVon) * 1000;
            ToolTipText = BaueToolTipText();

            BearbeitenCommand = new RelayCommand(() => parent.Bearbeiten(Leistung));
            NotizCommand = new RelayCommand(() => parent.NotizBearbeiten(Leistung));
            ErledigtUmschaltenCommand = new RelayCommand(() => parent.ErledigtUmschalten(Leistung));
            AbrechnenCommand = new RelayCommand(() => parent.Abrechnen(Leistung));
            AbrechnungZuruecksetzenCommand = new RelayCommand(() => parent.AbrechnungZuruecksetzen(Leistung));
        }

        /// <summary>Wird vom Code-behind bei MouseLeftButtonDown aufgerufen (Strg-Status lässt sich nicht sinnvoll per ICommand binden).</summary>
        public void Auswaehlen(bool strgGedrueckt) => _parent.LeistungAuswaehlen(Leistung, strgGedrueckt);

        private string BaueToolTipText()
        {
            var zeilen = new List<string> { $"Bahnseite: {Leistung.Bahnseite}", $"Länge: {(int)Metraj} m" };

            if (!string.IsNullOrWhiteSpace(Leistung.Anmerkung)) zeilen.Add($"Anmerkung: {Leistung.Anmerkung}");
            if (!string.IsNullOrWhiteSpace(Leistung.Anmerkung2)) zeilen.Add($"Anmerkung: {Leistung.Anmerkung2}");

            if (Leistung.IstAbgerechnet)
            {
                zeilen.Add(Leistung.AufmassNummer.HasValue
                    ? $"Abgerechnet in Aufmaß Nr. {Leistung.AufmassNummer}"
                    : "Abgerechnet (Aufmaß-Nr. unbekannt)");
            }

            if (!string.IsNullOrWhiteSpace(Leistung.Notiz)) zeilen.Add($"Notiz: {Leistung.Notiz}");

            return string.Join("\n", zeilen);
        }
    }
}
