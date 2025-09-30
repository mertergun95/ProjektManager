using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektManager.Models
{
    public class LSTProjekt
    {
        public string Name { get; set; }
        public string DateiPfad { get; set; }
        public List<LSTKabel> KabelListe { get; set; }
    }

}
