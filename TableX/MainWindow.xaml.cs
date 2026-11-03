using System.Windows;
using TableX.ViewModels;

namespace TableX
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new NavigationVM();
        }
    }
}
