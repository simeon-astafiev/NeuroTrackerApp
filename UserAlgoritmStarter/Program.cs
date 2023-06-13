using System;
using System.Windows.Forms;
using TogiSoft.Program.Core.Controls;

namespace UserAlgoritmStarter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var control = new UserAlgoritmControl();
            ControlBox.Show(control, "Интегратор внешних программ", MessageBoxButtons.OKCancel, defaultButton: 0, canResize: true);
            Application.Run();
        }
    }
}
