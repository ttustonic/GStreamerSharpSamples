namespace GstSamples
{
    class Program
    {
        public static void Main(string[] args)
        {
            //Console.WriteLine(" b<xx> for basic tutorial xx, p<xx> for playback tutorial xx");
            //var line = Console.ReadLine();
            //var name = "TestGst." + line.Replace("b", "BasicTutorial").Replace("p", "PlaybackTutorial");
            //var type = Type.GetType(name);
            //var run = type.GetMethod("Run");
            //run.Invoke(null, new object[] { args });

            WebCamTutorial02.Run(args);
        }
    }
}