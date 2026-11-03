using TableX.Utilities;

namespace TableX.ViewModels
{
    // De momento vacío: solo sirve para comprobar que el login navega bien
    public class HomeVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;

        public HomeVM(NavigationVM navigation)
        {
            _navigation = navigation;
        }
    }
}
