using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektManager.Models
{
    public class Projekt
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProjektPfad { get; set; } // Excel dosyasının yolu
        public List<Laenge> Laengen { get; set; } = new List<Laenge>();
    }

}
