using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjektManager.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T feld, T wert, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(feld, wert)) return false;
            feld = wert;
            OnPropertyChanged(name);
            return true;
        }
    }
}
