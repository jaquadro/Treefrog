using System;
using System.Windows.Forms;
using Treefrog.Windows.Forms;
using Treefrog.Framework.Model;
using System.IO;
using Treefrog.Framework;
using System.Threading;

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

            Application.Run(new Main());
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

            MessageBox.Show("TreeFrog crashed with the following:\r\n\r\n" + e.Message + "\r\n\r\n" + e.StackTrace);

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

