using ProjektManager.Models;
using ProjektManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class LaengeDetailPage : UserControl
    {
        public LaengeDetailPage(MainWindow main, Laenge laenge)
        {
            InitializeComponent();
            DataContext = new LaengeDetailViewModel(main, laenge, () => Window.GetWindow(this));
        }

        /// <summary>
        /// Strg-Klick zur Mehrfachauswahl lässt sich nicht sinnvoll rein per ICommand-Binding
        /// abbilden (Tastaturmodifikatoren gehören nicht zum Command-Parameter), daher bleibt
        /// dieser eine Klick-Handler im Code-behind und delegiert sofort an das ViewModel.
        /// </summary>
        private void LeistungsBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: LeistungBlockViewModel block })
            {
                block.Auswaehlen(Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
            }
        }
    }
}
