using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjektManager.Views
{
    /// <summary>
    /// Interaction logic for ProjektNameEingabeWindow.xaml
    /// </summary>
    public partial class ProjektNameEingabeWindow : Window
    {
        public string ProjektName { get; private set; }

        public ProjektNameEingabeWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjektNameTextBox.Text))
            {
                MessageBox.Show("Projektname darf nicht leer sein.");
                return;
            }

            ProjektName = ProjektNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        public ProjektNameEingabeWindow(string alterName) : this()
        {
            ProjektNameTextBox.Text = alterName;
        }


    }

}
