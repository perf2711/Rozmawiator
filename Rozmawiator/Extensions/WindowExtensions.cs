using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Rozmawiator.Controls;

namespace Rozmawiator.Extensions
{
    public static class WindowExtensions
    {
        public static void ShowError(this Window window, string header, string message, Brush headerBackground = null)
        {
            window.Dispatcher.Invoke(() =>
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
            });
        }

        public static void ShowError(this Window window, string header, string message, Action<ErrorControl> callback, Brush headerBackground = null)
        {
            window.Dispatcher.Invoke(() =>
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
            });
        }

        public static void ShowLoading(this Window window, string text, bool hideOtherLoadings = true)
        {
            if (hideOtherLoadings)
            {
                window.HideLoading();
            }

            window.Dispatcher.Invoke(() =>
            {
                var panel = window.Content as Panel;
                if (panel == null)
                {
                    return;
                }

                var loadingControl = new LoadingControl
                {
                    Text = text
                };

                Grid.SetColumnSpan(loadingControl, 99);
                Grid.SetRowSpan(loadingControl, 99);
                
                panel.Children.Add(loadingControl);
            });
        }

        public static void HideLoading(this Window window)
        {
            window.Dispatcher.Invoke(() =>
            {
                var panel = window.Content as Panel;
                if (panel == null)
                {
                    return;
                }

                var loadingControls = panel.Children.OfType<LoadingControl>().ToArray();

                foreach (var loadingControl in loadingControls)
                {
                    panel.Children.Remove(loadingControl);
                }
            });
        }
    }
}
