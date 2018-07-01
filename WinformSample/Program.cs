using System;
using System.Windows.Forms;

namespace WinformSample
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VideoOverlay());
        }
    }
}
