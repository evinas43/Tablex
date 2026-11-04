using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using TableX.Models;
using TableX.Services;
using TableX.Utilities;

namespace TableX.ViewModels
{
    public class LoginVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;
        private readonly AuthService _auth;

        public LoginVM(NavigationVM navigation)
        {
            _navigation = navigation;
            _auth = new AuthService(HttpService.Client);

            LoginCommand = new RelayCommand(async _ => await Login());

            // Teclado numérico del PIN
            AddDigitCommand = new RelayCommand(p => AddDigit(p as string));
            DeleteDigitCommand = new RelayCommand(_ => DeleteDigit());
            ClearPinCommand = new RelayCommand(_ => ClearPin());

            // Botón gris "Iniciar Sesión" (debajo de "O inicia sesión")
            // TODO: cambia esto por lo que tenga que hacer ese botón
            AlternativeLoginCommand = new RelayCommand(_ =>
                ToastService.Instance.Show("Próximamente", "info"));
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand AlternativeLoginCommand { get; }
        public RelayCommand AddDigitCommand { get; }
        public RelayCommand DeleteDigitCommand { get; }
        public RelayCommand ClearPinCommand { get; }

        public string? Username { get; set; }

        // ---------- PIN ----------

        public const int PinLength = 4;

        private string _pin = string.Empty;
        public string Pin
        {
            get => _pin;
            private set
            {
                _pin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PinDots));
                OnPropertyChanged(nameof(PinCounter));
            }
        }

        // Un bool por cada círculo: true = dígito escrito
        public bool[] PinDots => Enumerable.Range(0, PinLength).Select(i => i < _pin.Length).ToArray();

        // Texto "2/4" de la esquina
        public string PinCounter => $"{_pin.Length}/{PinLength}";

        // Públicos para que la vista los use también con el teclado físico
        public void AddDigit(string? digit)
        {
            if (digit is { Length: 1 } && digit[0] >= '0' && digit[0] <= '9' && _pin.Length < PinLength)
                Pin += digit;
        }

        public void DeleteDigit()
        {
            if (_pin.Length > 0)
                Pin = _pin.Substring(0, _pin.Length - 1);
        }

        public void ClearPin() => Pin = string.Empty;

        private async Task Login()
        {
            // Evita lanzar dos logins a la vez (p. ej. pulsando Enter varias veces)
            if (IsLoading)
                return;

            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Pin))
            {
                ToastService.Instance.Show("Rellena todos los campos", "warning");
                return;
            }

            if (Pin.Length != PinLength)
            {
                ToastService.Instance.Show($"El PIN debe tener {PinLength} dígitos", "warning");
                return;
            }

            try
            {
                IsLoading = true;

                var login = await _auth.LoginAsync(Username.Trim(), Pin);

                if (login == null || string.IsNullOrEmpty(login.Token))
                {
                    ToastService.Instance.Show("Usuario o PIN incorrectos", "error");
                    ClearPin(); // para volver a escribirlo desde cero
                    return;
                }

                // El login ya trae el usuario con su rol, no hace falta llamar a /Me
                SessionUser.Username = login.User?.Username ?? Username.Trim();
                SessionUser.Role = login.User?.Role;
                SessionUser.Token = login.Token;

                HttpService.SetToken(login.Token);

                // Navegar al home
                // (si quieres separar Caja y Camarero, aquí puedes decidir según SessionUser.Role)
                _navigation.CurrentView = new ShellVM(_navigation);
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                // La API ha respondido, pero con un error (404 ruta mal, 400 datos mal, 500...)
                ToastService.Instance.Show($"Error del servidor ({(int)ex.StatusCode.Value})", "error");
            }
            catch (HttpRequestException ex) when (ex.InnerException is AuthenticationException)
            {
                // El certificado HTTPS de desarrollo no es de confianza
                ToastService.Instance.Show("Certificado HTTPS no válido: ejecuta 'dotnet dev-certs https --trust'", "error");
            }
            catch (HttpRequestException)
            {
                // La API no está levantada o la URL de HttpService está mal
                ToastService.Instance.Show("No se puede conectar con el servidor", "error");
            }
            catch (TaskCanceledException)
            {
                // Han pasado los 15 s del Timeout de HttpService
                ToastService.Instance.Show("El servidor no responde", "error");
            }
            catch (Exception ex)
            {
                ToastService.Instance.Show($"Error inesperado: {ex.Message}", "error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Spinner
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }
}
