using System;
using System.Collections.Generic;

public class LSTKabel
{
    // Temel bilgiler
    public string KabelNr { get; set; }
    public string Kabeltyp { get; set; }
    public double KmVon { get; set; }     // Başlangıç km
    public double KmBis { get; set; }     // Bitiş km
    public int LängeSoll { get; set; }    // Beklenen uzunluk (metre)
    public DateTime? VerlegeDatum { get; set; }
    public double VerlegtMeter { get; set; }
    public bool IstAbgerechnet { get; set; }

    // Ek bilgiler
    public string VonPunkt { get; set; }  // Başlangıç verteilerpunkt (örneğin: KS1613)
    public string BisPunkt { get; set; }  // Bitiş verteilerpunkt
    public string Kabelquerschnitt { get; set; }
    public string Trommelnummer { get; set; }
    public int? Bestelllaenge { get; set; }
    public string Bemerkung { get; set; }

    public string KmVonRaw { get; set; }
    public string KmBisRaw { get; set; }

    // Görselleştirme yardımcıları
    public double BerechneVisualLaenge(double scaleFaktor = 1000)  // 1 km = 1000 px gibi
    {
        return (KmBis - KmVon) * scaleFaktor;
    }

    public override string ToString()
    {
        return $"{KabelNr} ({LängeSoll} m)";
    }
}
