﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvilsoftCommons.Exceptions;
using GrimDamage.GUI.Browser;
using GrimDamage.GUI.Forms;
using GrimDamage.Settings;
using GrimDamage.Tracking.Model;
using GrimDamage.Utility;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;

namespace GrimDamage {
    static class Program {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static string GetUuid() {

            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\EvilSoft\GrimDamage")) {
                string uuid = (string)registryKey?.GetValue("uuid");
                if (!string.IsNullOrEmpty(uuid)) {
                    return uuid;
                }

                UuidGenerator g = Guid.NewGuid();
                uuid = g.ToString().Replace("-", "");

                registryKey?.SetValue("uuid", uuid);
                return uuid;
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Thread.CurrentThread.Name == null) {
                Thread.CurrentThread.Name = "Main/UI";
                ExceptionReporter.EnableLogUnhandledOnThread();
            }

            ExceptionReporter.UrlCrashreport = "http://ribbs.dreamcrash.org/gddamage/crashreport.php";
            ExceptionReporter.UrlStats = "http://ribbs.dreamcrash.org/gddamage/stats.php";
            ExceptionReporter.LogExceptions = true;
            ExceptionReporter.Uuid = GetUuid();
#if !DEBUG
#endif
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            Logger.InfoFormat("Running version {0}.{1}.{2}.{3} from {4:dd/MM/yyyy}", version.Major, version.Minor, version.Build, version.Revision, buildDate);

            Logger.Info("Anonymous usage statistics and crash reports will be collected.");
            Logger.Info("Statistics and crash reports can be found at http://ribbs.dreamcrash.org/gddamage/logs.html");

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hook.dll"))) {
                MessageBox.Show("Error - It appears that hook.dll is missing\nMost likely this installation has been corrupted.", "Error");
                return;
            }

            
            string url = Properties.Settings.Default.DarkModeEnabled 
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "content", "darkmode.html")
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "content", "index.html");

            if (!File.Exists(url)) {
                MessageBox.Show("Error - It appears the stat view is missing", "Error");
            }

            bool showDevtools = args != null && args.Any(m => m.Contains("-devtools"));
            using (var browser = new CefBrowserHandler()) {
                WebViewJsPojo jsPojo = new WebViewJsPojo();
                browser.InitializeChromium(url, jsPojo);
                Application.Run(new Form1(browser, GetSettings(), showDevtools));
            }
        }

        private static AppSettings GetSettings() {
            return new AppSettings();
        }
    }
}
