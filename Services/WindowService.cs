using System;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Monitor.Services
{
    public class WindowService
    {
        private static readonly Logger logger = LogManager.GetLogger("WindowService");

        public class Window
        {
            public Window(IntPtr windowHandle, string windowTitle)
            {
                this.WindowHandle = windowHandle;
                this.WindowTitle = windowTitle;
            }

            public string WindowTitle { get; set; }

            public IntPtr WindowHandle { get; }
        }

        public Window[] ListWindows()
        {
            logger.Info("ListWindows()");
            var processes = Process.GetProcesses();
            var result = processes.Select(p => new Window(p.MainWindowHandle, p.MainWindowTitle))
                .Where(w => !string.IsNullOrWhiteSpace(w.WindowTitle)).ToArray();
            logger.Info(String.Join("\n", result.Select(p => p.WindowTitle)));
            return result;
        }
    }
}