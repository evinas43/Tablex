using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableX.Services
{
    public class ToastService
    {
        public static ToastService Instance { get; } = new ToastService();

        public event Action<string, string>? OnShowToast;

        public void Show(string message, string type = "info")
        {
            OnShowToast?.Invoke(message, type);
        }
    }
}
