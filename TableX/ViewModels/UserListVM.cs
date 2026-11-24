using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using TableX.Models;
using TableX.Services;
using TableX.Utilities;

namespace TableX.ViewModels
{
    // Lista de usuarios: tabla con usuario, rol y acciones
    public class UserListVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;
        private readonly UserService _users;

        public UserListVM(NavigationVM navigation)
        {
            _navigation = navigation;
            _users = new UserService(HttpService.Client);

            EditUserCommand = new RelayCommand(p => EditUser(p as UserInfo));

            // Se cargan al abrir la pantalla (el ViewModel se crea cada vez que se pulsa la pestaña)
            _ = LoadUsersAsync();
        }

        // ObservableCollection: si se añade o quita un usuario, la tabla se actualiza sola
        public ObservableCollection<UserInfo> Users { get; } = new ObservableCollection<UserInfo>();

        // ---------- Carga desde la API ----------

        private async Task LoadUsersAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Cargando usuarios...";

                var users = await _users.GetAllAsync();

                Users.Clear();
                foreach (var user in users)
                    Users.Add(user);

                StatusMessage = Users.Count == 0 ? "No hay usuarios" : null;
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                // La API ha respondido con error (401 sin token, 403 sin permiso, 404 ruta mal, 500...)
                StatusMessage = $"Error del servidor ({(int)ex.StatusCode.Value})";
            }
            catch (HttpRequestException)
            {
                StatusMessage = "No se puede conectar con el servidor";
            }
            catch (TaskCanceledException)
            {
                // Han pasado los 15 s del Timeout de HttpService
                StatusMessage = "El servidor no responde";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // Texto bajo la tabla: cargando, lista vacía o error. null = no se muestra nada
        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // ---------- Acciones ----------

        // Cada fila pasa su UserInfo como CommandParameter
        public RelayCommand EditUserCommand { get; }

        // Panel de edición abierto. null = se ve la tabla; con valor = se ve el panel
        private UserEditVM? _editingUser;
        public UserEditVM? EditingUser
        {
            get => _editingUser;
            private set
            {
                _editingUser = value;
                OnPropertyChanged();
            }
        }

        private void EditUser(UserInfo? user)
        {
            if (user == null)
                return;

            EditingUser = new UserEditVM(user, _users, CloseEditor, OnUserSaved, OnUserDeleted);
        }

        private void CloseEditor() => EditingUser = null;

        // El panel ya lo ha guardado en la API: se cambia la fila por el usuario nuevo y se vuelve a la lista.
        // Se sustituye el objeto entero porque UserInfo no avisa a la tabla si cambian sus propiedades.
        private void OnUserSaved(UserInfo oldUser, UserInfo updated)
        {
            var index = Users.IndexOf(oldUser);
            if (index >= 0)
                Users[index] = updated;

            CloseEditor();
        }

        // El panel ya lo ha borrado en la API: se quita de la tabla y se vuelve a la lista
        private void OnUserDeleted(UserInfo user)
        {
            Users.Remove(user);
            StatusMessage = Users.Count == 0 ? "No hay usuarios" : null;
            CloseEditor();
        }
    }
}
