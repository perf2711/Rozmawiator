using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rozmawiator.Controls;

namespace Rozmawiator.Extensions
{
    public static class WindowExtensions
    {
        public static void ShowError(this Window window, string header, string message, Brush headerBackground = null)
        {
            var panel = window.Content as Panel;
            if (panel == null)
            {
                return;
            }
            var errorControl = new ErrorControl
            {
                ErrorHeader = header,
                ErrorMessage = message,
            };

            Grid.SetColumnSpan(errorControl, 99);
            Grid.SetRowSpan(errorControl, 99);

            if (headerBackground != null)
            {
                errorControl.ErrorHeaderBackground = headerBackground;
            }

            panel.Children.Add(errorControl);
        }

        public static void ShowError(this Window window, string header, string message, Action<ErrorControl> callback, Brush headerBackground = null)
        {
            var panel = window.Content as Panel;
            if (panel == null)
            {
                return;
            }
            var errorControl = new ErrorControl
            {
                ErrorHeader = header,
                ErrorMessage = message,
            };

            Grid.SetColumnSpan(errorControl, 99);
            Grid.SetRowSpan(errorControl, 99);

            if (callback != null)
            {
                errorControl.CloseClick += callback;
            }
            if (headerBackground != null)
            {
                errorControl.ErrorHeaderBackground = headerBackground;
            }

            panel.Children.Add(errorControl);
        }
    }
}
