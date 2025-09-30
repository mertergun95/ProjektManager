using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektManager.Models
{
    public class Leistung
    {
        public int Id { get; set; }
        public double KmVon { get; set; }
        public double KmBis { get; set; }
        public string Bahnseite { get; set; }
        public string Leistungsbeschreibung { get; set; }
        public string Anmerkung { get; set; }
        public string Anmerkung2 { get; set; }
        public double? LaengeMeter { get; set; }

        // ✅ Yeni eklenenler:
        public bool IstFertiggestellt { get; set; } = false;     // Tamamlandı mı?
        public bool IstAbgerechnet { get; set; } = false;         // Faturalandırıldı mı?

        public string Notiz { get; set; } = "";

    }
}

