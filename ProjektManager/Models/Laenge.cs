using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektManager.Models
{
    public class Laenge
    {
        public int Id { get; set; }
        public string Bezeichnung { get; set; }  // Länge 1, Länge 2, vs.
        public double KmVon { get; set; }
        public double KmBis { get; set; }
        public List<Leistung> Leistungen { get; set; } = new List<Leistung>();

        public List<(double KmVon, double KmBis)> Fertiggestellt { get; set; } = new();
        public List<(double KmVon, double KmBis)> Abgerechnet { get; set; } = new();

        public bool KabelVerlegt { get; set; }

    }

}
