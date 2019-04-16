using System.IO;

namespace TestPlugins
{
    class Program
    {
        public static string _rootDir =
            Path.GetFullPath(Path.Combine(@"..\..\..\..\..\soundsamples\"));

        static void Main(string[] args)
        {
            Gst.Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            TestElement.Run();
        }
    }
}
