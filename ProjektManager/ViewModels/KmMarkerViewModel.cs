using ProjektManager.Views.Shared;
using System.Windows;

namespace ProjektManager.ViewModels
{
    /// <summary>Eine Km-Beschriftung an einer bestimmten X-Position auf der Km-Leiste (Canvas).</summary>
    public class KmMarkerViewModel
    {
        public FrameworkElement Visual { get; }
        public double X { get; }

        public KmMarkerViewModel(string text, double x)
        {
            Visual = VisualBuilder.ErzeugeKmMarker(text);
            X = x;
        }
    }
}
