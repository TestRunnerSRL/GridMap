using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GridMap
{
    public static class Logger
    {
        static List<Window> windows;

        static Logger()
        {
            windows = new List<Window>();
        }

        public static void AddWindowHandler(Window window)
        {
            windows.Add(window);
        }

        public static void RemoveWindowHandler(Window window)
        {
            windows.Remove(window);
        }

        public static void Log(string title, string description = null)
        {
            if (description == null)
            {
                Console.WriteLine($"{title}");
            }
            else
            {
                Console.WriteLine($"{title}: {description}");
            }

            foreach (var window in windows)
            {
                Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => {
                    var notification = new NotificationWindow(window, title, description);
                    notification.Show();
                }));
            }
        }
    }
}
