using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using TableX.Models;
using TableX.Services;
using TableX.Utilities;

namespace TableX.ViewModels
{
    // Una opción del desplegable de rol: el valor que entiende la API y el texto que se ve
    public class RoleOption
    {
        public RoleOption(string value, string label)
        {
            Value = value;
            Label = label;
        }

        public string Value { get; }
        public string Label { get; }
    }

    // Panel para editar un usuario: cambiar nombre y rol, o eliminarlo.
    // Lo abre UserListVM al pulsar el lápiz y se muestra en lugar de la tabla.
    public class UserEditVM : ViewModelBase
    {
        private readonly UserInfo _user;
        private readonly UserService _users;
        private readonly Action _onClose;
        private readonly Action<UserInfo, UserInfo> _onSaved;
        private readonly Action<UserInfo> _onDeleted;

        public UserEditVM(UserInfo user, UserService users, Action onClose,
                          Action<UserInfo, UserInfo> onSaved, Action<UserInfo> onDeleted)
        {
            _user = user;
            _users = users;
            _onClose = onClose;
            _onSaved = onSaved;
            _onDeleted = onDeleted;

            // Copias para editar: el UserInfo de la tabla no cambia hasta guardar
            _username = user.Username ?? string.Empty;
            _role = user.Role;

            BackCommand = new RelayCommand(_ => _onClose());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => HasChanges && !IsBusy);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsSelf && !IsBusy);
        }

        // ---------- Datos que se muestran ----------

        // Nombre original (cabecera del panel)
        public string OriginalName => _user.Username ?? string.Empty;

        public int Id => _user.Id;

        // Roles que se pueden elegir
        public IReadOnlyList<RoleOption> Roles { get; } = new List<RoleOption>
        {
            new RoleOption("caja", "Caja"),
            new RoleOption("camarero", "Camarero"),
        };

        // ¿Es el usuario con la sesión iniciada? No se puede eliminar a sí mismo
        public bool IsSelf =>
            string.Equals(_user.Username, SessionUser.Username, StringComparison.OrdinalIgnoreCase);

        // ---------- Campos editables ----------

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        // Se enlaza con SelectedValue del desplegable ("caja" o "camarero")
        private string? _role;
        public string? Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        private bool HasChanges =>
            !string.IsNullOrWhiteSpace(Username) &&
            (Username.Trim() != OriginalName || Role != _user.Role);

        // ---------- Estado ----------

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        // Texto de resultado o error bajo los campos. null = no se muestra
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

        public RelayCommand BackCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }

        private async Task SaveAsync()
        {
            var username = Username.Trim();

            // Mismas reglas que la API (UpdateUserRequest): nombre de 3 a 50 caracteres y un rol
            if (username.Length < 3 || username.Length > 50)
            {
                StatusMessage = "El nombre debe tener entre 3 y 50 caracteres";
                return;
            }

            if (string.IsNullOrEmpty(Role))
            {
                StatusMessage = "Elige un rol";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = null;

                var updated = await _users.UpdateAsync(_user.Id, username, Role);

                // Si se ha editado a sí mismo, la sesión pasa a tener el nombre y rol nuevos
                if (IsSelf)
                {
                    SessionUser.Username = updated.Username;
                    SessionUser.Role = updated.Role;
                }

                // Cambia la fila de la tabla y vuelve a la lista
                _onSaved(_user, updated);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                StatusMessage = "Ya existe un usuario con ese nombre";
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                // 400 datos no válidos, 401 sin token, 403 sin permiso, 404 ya no existe, 500...
                StatusMessage = $"No se ha podido guardar: error del servidor ({(int)ex.StatusCode.Value})";
            }
            catch (HttpRequestException)
            {
                StatusMessage = "No se puede conectar con el servidor";
            }
            catch (TaskCanceledException)
            {
                StatusMessage = "El servidor no responde";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteAsync()
        {
            var confirm = MessageBox.Show(
                $"¿Seguro que quieres eliminar al usuario \"{OriginalName}\"?\nEsta acción no se puede deshacer.",
                "Eliminar usuario",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = null;

                await _users.DeleteAsync(_user.Id);

                // Lo quita de la tabla y vuelve a la lista
                _onDeleted(_user);
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                // 401 sin token, 403 sin permiso, 404 ya no existe, 500...
                StatusMessage = $"No se ha podido eliminar: error del servidor ({(int)ex.StatusCode.Value})";
            }
            catch (HttpRequestException)
            {
                StatusMessage = "No se puede conectar con el servidor";
            }
            catch (TaskCanceledException)
            {
                StatusMessage = "El servidor no responde";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
