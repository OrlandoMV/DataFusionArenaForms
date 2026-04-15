using System;
using System.Windows.Forms;

namespace DataFusionArena
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Register global exception handlers to surface runtime errors
            Application.ThreadException += (s, e) => HandleException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception exObj)
                    HandleException(exObj);
                else
                    HandleException(new Exception("Unhandled exception: " + e.ExceptionObject?.ToString()));
            };

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleException(Exception ex)
        {
            try
            {
                // Log to file next to exe
                try
                {
                    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFusionArena_error.log");
                    System.IO.File.AppendAllText(path, DateTime.Now.ToString("s") + " - " + ex.ToString() + Environment.NewLine);
                }
                catch { }

                MessageBox.Show("Se produjo una excepción:\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                try { Console.WriteLine(ex.ToString()); } catch { }
            }
        }
    }
}
