using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using TableX.Models;
using TableX.Services;
using TableX.Utilities;

namespace TableX.ViewModels
{
    // Pantalla de inicio: saludo de bienvenida y mesas del bar
    public class HomeVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;
        private readonly MesaService _mesas;

        // Hora en que se abre la pantalla: el saludo, el icono y la fecha salen de aquí
        private readonly DateTime _now = DateTime.Now;

        public HomeVM(NavigationVM navigation)
        {
            _navigation = navigation;
            _mesas = new MesaService(HttpService.Client);

            AbrirMesaCommand = new RelayCommand(p => AbrirMesa(p as Mesa));

            // Se cargan al abrir la pantalla (el ViewModel se crea cada vez que se pulsa la pestaña)
            _ = LoadMesasAsync();
        }

        // ---------- Mesas ----------

        // ObservableCollection: al añadir mesas, la vista las dibuja sola
        public ObservableCollection<Mesa> Mesas { get; } = new ObservableCollection<Mesa>();

        // Cada mesa pasa su Mesa como CommandParameter
        public RelayCommand AbrirMesaCommand { get; }

        // Texto en el centro del área de mesas: cargando, sin mesas o error. null = no se muestra
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

        private async Task LoadMesasAsync()
        {
            try
            {
                StatusMessage = "Cargando mesas...";

                var mesas = await _mesas.GetAllAsync();

                Mesas.Clear();
                foreach (var mesa in mesas)
                    Mesas.Add(mesa);

                StatusMessage = Mesas.Count == 0 ? "No hay mesas" : null;
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                // La API ha respondido con error (401 sin token, 404 ruta mal, 500...)
                StatusMessage = $"No se han podido cargar las mesas: error del servidor ({(int)ex.StatusCode.Value})";
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
        }

        private void AbrirMesa(Mesa? mesa)
        {
            if (mesa == null)
                return;

            // TODO: abrir la mesa (pedido, comandas...)
        }

        // ---------- Saludo ----------

        public string UserName => SessionUser.Username ?? string.Empty;

        // Noche: de 20:00 a 5:59
        private bool IsNight => _now.Hour >= 20 || _now.Hour < 6;

        // "Buenos días, caja"
        public string Greeting
        {
            get
            {
                string saludo;

                if (IsNight)
                    saludo = "Buenas noches";
                else if (_now.Hour < 14)
                    saludo = "Buenos días";
                else
                    saludo = "Buenas tardes";

                return $"{saludo}, {UserName}";
            }
        } 

        // Sol de día, luna de noche (iconos de NavIcons)
        public string GreetingIcon => IsNight ? NavIcons.Moon : NavIcons.Sun;

        // "Martes, 10 de noviembre de 2026"
        public string TodayText
        {
            get
            {
                var text = _now.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
                return char.ToUpper(text[0]) + text.Substring(1);
            }
        }
    }
}
