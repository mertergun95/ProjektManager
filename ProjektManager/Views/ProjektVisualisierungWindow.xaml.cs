using ProjektManager.Models;
using ProjektManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjektManager.Views
{
    public partial class ProjektVisualisierungPage : UserControl
    {
        public ProjektVisualisierungPage(MainWindow main, Projekt projekt)
        {
            InitializeComponent();
            DataContext = new ProjektVisualisierungViewModel(main, projekt);
        }

        private void LaengenBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: LaengeBlockViewModel block })
            {
                block.OeffnenCommand.Execute(null);
            }
        }
    }
}
