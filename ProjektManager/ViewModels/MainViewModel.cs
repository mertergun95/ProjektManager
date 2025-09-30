using ProjektManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjektManager.Data;

namespace ProjektManager.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Projekt> Projekte { get; set; }

        public MainViewModel()
        {
            Projekte = new ObservableCollection<Projekt>(ProjektSpeicher.Laden());
            // Geçici örnek veri
            
        }
    }
}
