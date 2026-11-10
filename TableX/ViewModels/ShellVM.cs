using System;
using System.Collections.Generic;
using TableX.Models;
using TableX.Services;
using TableX.Utilities;

namespace TableX.ViewModels
{
    // Zona privada de la app: la NavBar arriba y la página activa debajo.
    public class ShellVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;

        public ShellVM(NavigationVM navigation)
        {
            _navigation = navigation;

            // Para añadir una pestaña, añade una línea aquí
            NavItems = new List<NavItemVM>
            {
                new NavItemVM("Inicio", NavIcons.Home, () => new HomeVM(_navigation)),
                new NavItemVM("Usuarios", NavIcons.Users, () => new UserListVM(_navigation)),
            };

            LogoutCommand = new RelayCommand(_ => Logout());

            SelectedItem = NavItems[0];
        }

        // ---------- Pestañas ----------

        public IReadOnlyList<NavItemVM> NavItems { get; }

        // La NavBar lo actualiza al pulsar una pestaña, y al cambiar se abre su página
        private NavItemVM? _selectedItem;
        public NavItemVM? SelectedItem
        {
            get => _selectedItem;
            set
            {
                // Se ignora null: siempre hay una pestaña activa, aunque el ListBox se descargue
                if (value == null || value == _selectedItem)
                    return;

                _selectedItem = value;
                OnPropertyChanged();

                CurrentPage = value.CreatePage();
            }
        }

        private object? _currentPage;
        public object? CurrentPage
        {
            get => _currentPage;
            private set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        // ---------- Usuario de la sesión ----------

        public string UserName => SessionUser.Username ?? string.Empty;

        // "admin" -> "AD"
        public string UserInitials =>
            UserName.Length <= 2 ? UserName.ToUpper() : UserName.Substring(0, 2).ToUpper();

        // "caja" -> "Caja"; la API puede devolver "admin", que se muestra como "Administrador"
        public string UserRole
        {
            get
            {
                var role = SessionUser.Role;

                if (string.IsNullOrEmpty(role))
                    return string.Empty;

                if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
                    return "Administrador";

                return char.ToUpper(role[0]) + role.Substring(1);
            }
        }

        // ---------- Cerrar sesión ----------

        public RelayCommand LogoutCommand { get; }

        private void Logout()
        {
            SessionUser.Clear();
            HttpService.ClearToken();
            _navigation.CurrentView = new LoginVM(_navigation);
        }
    }
}
