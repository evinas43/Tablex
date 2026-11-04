using System;

namespace TableX.ViewModels
{
    // Una pestaña de la NavBar: qué texto e icono muestra y qué página abre.
    // Cuál está seleccionada lo controla ShellVM.SelectedItem.
    public class NavItemVM
    {
        public NavItemVM(string title, string icon, Func<object> createPage)
        {
            Title = title;
            Icon = icon;
            CreatePage = createPage;
        }

        public string Title { get; }

        // Uno de los iconos de NavIcons
        public string Icon { get; }

        public Func<object> CreatePage { get; }
    }
}
