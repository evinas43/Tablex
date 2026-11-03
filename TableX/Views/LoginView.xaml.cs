using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TableX.Services;
using TableX.ViewModels;

namespace TableX.Views
{
    /// <summary>
    /// Lógica de interacción para LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private int _toastVersion;

        public LoginView()
        {
            InitializeComponent();

            // Me suscribo al cargar y me desuscribo al salir de la vista
            Loaded += (s, e) => ToastService.Instance.OnShowToast += ShowToast;
            Unloaded += (s, e) => ToastService.Instance.OnShowToast -= ShowToast;
        }

        // ---------- PIN ----------

        // PasswordBox no admite Binding en Password, así que se pasa a mano al ViewModel
        private void PinBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PinPlaceholder.Visibility = PinBox.Password.Length == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (DataContext is LoginVM vm)
                vm.Pin = PinBox.Password;
        }

        // Solo deja escribir dígitos
        private void PinBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        // El espacio no pasa por PreviewTextInput, se bloquea aquí
        private void PinBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // Evita pegar texto que no sea numérico
        private void PinBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetData(typeof(string)) is not string text || !IsNumeric(text))
                e.CancelCommand();
        }

        private static bool IsNumeric(string text) =>
            text.Length > 0 && text.All(c => c >= '0' && c <= '9');

        // ---------- TOAST ----------

        public async void ShowToast(string message, string type = "info")
        {
            int version = ++_toastVersion;

            ToastText.Text = message;

            // Icono, colores y "sacudida" según el tipo
            double shake = 0;

            switch (type)
            {
                case "error":
                    ToastBadge.Background = Hex("#DC2626");
                    Toast.BorderBrush = Hex("#FECACA");
                    ToastIcon.Text = "✕";
                    shake = 5;
                    break;

                case "warning":
                    ToastBadge.Background = Hex("#FF5A26");
                    Toast.BorderBrush = Hex("#FED7C7");
                    ToastIcon.Text = "!";
                    shake = 2;
                    break;

                case "success":
                    ToastBadge.Background = Hex("#16A34A");
                    Toast.BorderBrush = Hex("#BBF7D0");
                    ToastIcon.Text = "✓";
                    break;

                default: // info
                    ToastBadge.Background = Hex("#1C1C1C");
                    Toast.BorderBrush = Hex("#E4E4E7");
                    ToastIcon.Text = "i";
                    break;
            }

            // Transform nuevo en cada toast: Y la anima el Storyboard, X la sacudida
            var transform = new TranslateTransform(0, -20);
            Toast.RenderTransform = transform;

            if (shake > 0)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation
                {
                    From = -shake,
                    To = shake,
                    Duration = TimeSpan.FromMilliseconds(50),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(3),
                    FillBehavior = FillBehavior.Stop // al acabar vuelve a X = 0
                });
            }

            ((Storyboard)Resources["ShowToast"]).Begin(this);

            await Task.Delay(2500);

            // Si mientras tanto ha salido otro toast, no lo escondemos
            if (version == _toastVersion)
                ((Storyboard)Resources["HideToast"]).Begin(this);
        }

        private static SolidColorBrush Hex(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }
}