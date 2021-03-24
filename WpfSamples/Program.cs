using System;
using System.Windows;

namespace WpfSamples
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new Application();
            app.Run(new VideoOverlay());
        }
    }
}
