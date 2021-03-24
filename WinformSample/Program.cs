using System;
using System.Windows.Forms;

namespace WinformSample
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
//            Application.Run(new BasicTutorial5());  // works
            Application.Run(new VideoOverlay());  // works

        }
    }
}
