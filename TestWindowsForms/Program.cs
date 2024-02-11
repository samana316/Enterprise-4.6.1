using System;
using System.Windows.Forms;

namespace TestWindowsForms
{
    internal static class Program
    {
        [STAThread]
        internal static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void CurrentDomain_UnhandledException(
            object sender, 
            UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }
    }
}
