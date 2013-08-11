using System;
using System.Windows.Forms;
using Treefrog.Windows.Forms;
using Treefrog.Framework.Model;
using System.IO;
using Treefrog.Framework;
using System.Threading;
using ExceptionReporting;
using Treefrog.Core;

namespace Treefrog
{
#if WINDOWS
    static class Program
    {
        public static Project CurrentProject { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main ()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += HandleThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += HandleDomainException;

            Loader loader = new Loader();
            loader.Compose();

            Application.Run(new Main(loader));
        }

        private static void HandleException(Exception e)
        {
            if (CurrentProject != null)
            {
                if (!Directory.Exists("recovery"))
                    Directory.CreateDirectory("recovery");

                string filePath = "recovery/" + (CurrentProject.Name ?? "project.tlpx");
                using (FileStream fstr = File.OpenWrite(filePath))
                {
                    CurrentProject.Save(fstr, new FileProjectResolver(filePath));
                }
            }

            ExceptionReporter reporter = new ExceptionReporter();

            reporter.Config.ShowSysInfoTab = false;
            reporter.Config.ShowConfigTab = false;
            reporter.Config.EmailReportAddress = "jaquadro@gmail.com";
            reporter.Config.TitleText = "Treefrog Crash";
            reporter.Config.UserExplanationLabel = "Enter a brief description leading up to the crash.  Your project has been saved to /recovery.";

            reporter.Show(e);

            Application.Exit();
        }

        private static void HandleThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void HandleDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((e.ExceptionObject as Exception) ?? new Exception());
        }
    }
#endif
}

