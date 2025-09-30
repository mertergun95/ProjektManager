using ProjektManager.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ProjektManager.Views
{
    public partial class LeistungBearbeitenWindow : Window
    {
        private Leistung _leistung;

        public LeistungBearbeitenWindow(Leistung leistung)
        {
            InitializeComponent();
            _leistung = leistung;
            Fuellen();
        }

        private void Fuellen()
        {
            BeschreibungBox.Text = _leistung.Leistungsbeschreibung;
            BahnseiteBox.Text = _leistung.Bahnseite;
            KmVonBox.Text = _leistung.KmVon.ToString("F3", CultureInfo.InvariantCulture);
            KmBisBox.Text = _leistung.KmBis.ToString("F3", CultureInfo.InvariantCulture);
            AnmerkungBox.Text = _leistung.Anmerkung;
            Anmerkung2Box.Text = _leistung.Anmerkung2;
        }

        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            bool hatSichGeaendert = false;

            if (sender == BeschreibungBox && _leistung.Leistungsbeschreibung != BeschreibungBox.Text)
            {
                _leistung.Leistungsbeschreibung = BeschreibungBox.Text;
                hatSichGeaendert = true;
            }
            else if (sender == BahnseiteBox && _leistung.Bahnseite != BahnseiteBox.Text)
            {
                _leistung.Bahnseite = BahnseiteBox.Text;
                hatSichGeaendert = true;
            }
            else if (sender == KmVonBox && double.TryParse(KmVonBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var kmVon))
            {
                if (Math.Abs(_leistung.KmVon - kmVon) > 0.001)
                {
                    _leistung.KmVon = kmVon;
                    hatSichGeaendert = true;
                }
            }
            else if (sender == KmBisBox && double.TryParse(KmBisBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var kmBis))
            {
                if (Math.Abs(_leistung.KmBis - kmBis) > 0.001)
                {
                    _leistung.KmBis = kmBis;
                    hatSichGeaendert = true;
                }
            }
            else if (sender == AnmerkungBox && _leistung.Anmerkung != AnmerkungBox.Text)
            {
                _leistung.Anmerkung = AnmerkungBox.Text;
                hatSichGeaendert = true;
            }
            else if (sender == Anmerkung2Box && _leistung.Anmerkung2 != Anmerkung2Box.Text)
            {
                _leistung.Anmerkung2 = Anmerkung2Box.Text;
                hatSichGeaendert = true;
            }

            if (hatSichGeaendert)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}