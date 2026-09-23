using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjektManager.Views.Shared
{
    /// <summary>
    /// Hängt einer TextBox eine einfache Vorschlagsliste an: während der Eingabe werden
    /// passende, bereits verwendete Werte in einem Popup unter dem Feld angezeigt, per Klick
    /// oder Pfeiltaste+Enter übernehmbar. Wird u.a. für Leistungsbeschreibung und Bahnseite
    /// im Bearbeiten-Dialog verwendet, damit bereits erfasste Formulierungen wiederverwendet
    /// statt jedes Mal neu getippt werden.
    /// </summary>
    public static class AutoComplete
    {
        public static void Aktivieren(TextBox box, IEnumerable<string> vorschlaege)
        {
            var werte = vorschlaege.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            if (werte.Count == 0) return;

            var listBox = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                ItemContainerStyle = new Style(typeof(ListBoxItem))
                {
                    Setters = { new Setter(Control.PaddingProperty, new Thickness(10, 6, 10, 6)) }
                }
            };

            var rahmen = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceBrush"],
                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 12, ShadowDepth = 2, Opacity = 0.15 },
                Child = listBox
            };

            var popup = new Popup
            {
                PlacementTarget = box,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                Child = rahmen
            };

            void AktualisiereVorschlaege()
            {
                string text = box.Text.Trim();
                if (text.Length == 0)
                {
                    popup.IsOpen = false;
                    return;
                }

                var treffer = werte
                    .Where(w => w.Contains(text, StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(w, text, StringComparison.OrdinalIgnoreCase))
                    .Take(8)
                    .ToList();

                if (treffer.Count == 0)
                {
                    popup.IsOpen = false;
                    return;
                }

                listBox.ItemsSource = treffer;
                popup.Width = Math.Max(box.ActualWidth, 220);
                popup.IsOpen = true;
            }

            void Uebernehmen(string wert)
            {
                box.Text = wert;
                box.CaretIndex = box.Text.Length;
                popup.IsOpen = false;
                box.Focus();
            }

            box.TextChanged += (s, e) => AktualisiereVorschlaege();

            box.PreviewKeyDown += (s, e) =>
            {
                if (!popup.IsOpen) return;

                if (e.Key == Key.Escape)
                {
                    popup.IsOpen = false;
                }
                else if (e.Key == Key.Down && listBox.Items.Count > 0)
                {
                    listBox.Focus();
                    listBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            };

            listBox.PreviewMouseLeftButtonUp += (s, e) =>
            {
                if (listBox.SelectedItem is string wert) Uebernehmen(wert);
            };

            listBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && listBox.SelectedItem is string wert) Uebernehmen(wert);
            };
        }
    }
}
