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
    public partial class NotizEingabeWindow : Window
    {
        public string EingetrageneNotiz { get; private set; }

        public NotizEingabeWindow(string vorhandeneNotiz = "")
        {
            InitializeComponent();
            NotizTextBox.Text = vorhandeneNotiz;
            NotizTextBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            EingetrageneNotiz = NotizTextBox.Text.Trim();
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
