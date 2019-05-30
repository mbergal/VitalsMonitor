using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Monitor;
using Monitor.Windows.MainWindow;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Player
{
    static class Program
    {
        private static NLog.Logger _logger = LogManager.GetLogger("Program");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigureLogging();
            _logger.Info("Starting");

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            var logFile =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Vitals Monitor", "log.txt");

            var fileTarget = new FileTarget("target2")
            {
                FileName = logFile,
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForAllLevels(fileTarget);
            LogManager.Configuration = config;
        }
    }
}