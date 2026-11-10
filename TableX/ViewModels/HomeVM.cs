using System;
using System.Globalization;
using TableX.Models;
using TableX.Utilities;

namespace TableX.ViewModels
{
    // Pantalla de inicio: de momento solo el saludo de bienvenida
    public class HomeVM : ViewModelBase
    {
        private readonly NavigationVM _navigation;

        // Hora en que se abre la pantalla: el saludo, el icono y la fecha salen de aquí
        private readonly DateTime _now = DateTime.Now;

        public HomeVM(NavigationVM navigation)
        {
            _navigation = navigation;
        }

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
