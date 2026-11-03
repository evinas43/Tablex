using TableX.Utilities;

namespace TableX.ViewModels
{
    // Es el DataContext de MainWindow: cambiando CurrentView se cambia de pantalla
    public class NavigationVM : ViewModelBase
    {
        private object? _currentView;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public NavigationVM()
        {
            CurrentView = new LoginVM(this);
        }
    }
}
