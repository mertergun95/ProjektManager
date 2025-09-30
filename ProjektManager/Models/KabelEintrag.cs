using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektManager.Models
{
    public class KabelEintrag
    {
        public string Kabelnummer { get; set; }
        public string Kabeltyp { get; set; }
        public double VonKm { get; set; }
        public double BisKm { get; set; }
        public double LaengeSoll { get; set; }
        public double LaengeIst { get; set; }
    }
}
