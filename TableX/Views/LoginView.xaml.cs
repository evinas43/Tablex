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

        // ---------- PIN (teclado físico) ----------

        // Además del teclado en pantalla, se puede escribir el PIN con el teclado.
        // Si el foco está en el campo de usuario, las teclas van a ese campo (el usuario puede tener números).
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (DataContext is not LoginVM vm || Keyboard.FocusedElement is TextBox)
                return;

            if (e.Key >= Key.D0 && e.Key <= Key.D9 && Keyboard.Modifiers == ModifierKeys.None)
                vm.AddDigit(((int)(e.Key - Key.D0)).ToString());
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                vm.AddDigit(((int)(e.Key - Key.NumPad0)).ToString());
            else if (e.Key == Key.Back)
                vm.DeleteDigit();
            else if (e.Key == Key.Delete || e.Key == Key.Escape)
                vm.ClearPin();
            else
                return;

            e.Handled = true;
        }

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