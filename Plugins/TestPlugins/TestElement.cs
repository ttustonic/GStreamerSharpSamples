using System;
using System.IO;
using Gst;

namespace TestPlugins
{
    class TestElement
    {
        public static void Run()
        {
            Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", Environment.CurrentDirectory);
            var t = new TestElement();
//            t.TestAudioSrc();
//            t.TestMixer();
            t.TestSimpleIdentity();
        }

        void TestAudioSrc()
        {
            Plugin.RegisterStatic(1, 14, "csaudiotestsrc", "csaudiotestsrc plugin",
                CsAudioTestSrc.RegisterPlugin, "0.1", "LGPL", "csaudiotestsrc", "csaudiotestsrc", "http://test");
            var pipe = Parse.Launch(@"csaudiotestsrc ! audioconvert ! audioresample ! autoaudiosink ");
            var bus = pipe.Bus;
            bool terminate = false;
            pipe.SetState(State.Playing);

            do
            {
                var msg = bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);
                switch (msg.Type)
                {
                    case MessageType.Error:
                        msg.ParseError(out var err, out var dbg);
                        Console.WriteLine(err.Message);
                        Console.WriteLine(dbg);
                        terminate = true;
                        break;
                    case MessageType.Eos:
                        Console.WriteLine("Eos");
                        terminate = true;
                        break;
                }
            }
            while (!terminate);
            pipe.SetState(State.Null);
            Console.ReadLine();
        }

        void TestMixer()
        {
            Plugin.RegisterStatic(1, 14, "csvideomixer", "csvideomixer plugin",
                CsVideoMixer.RegisterPlugin, "0.1", "LGPL", "simpleidentity", "simpleidentity", "http://test");

            var pipe = Parse.Launch(@"csvideomixer name=mixer ! videoconvert ! autovideosink videotestsrc pattern=ball ! mixer. videotestsrc pattern=spokes ! mixer. ");
            var bus = pipe.Bus;
            bool terminate = false;
            pipe.SetState(State.Playing);

            do
            {
                var msg = bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);
                switch (msg.Type)
                {
                    case MessageType.Error:
                        msg.ParseError(out var err, out var dbg);
                        Console.WriteLine(err.Message);
                        Console.WriteLine(dbg);
                        terminate = true;
                        break;
                    case MessageType.Eos:
                        Console.WriteLine("Eos");
                        terminate = true;
                        break;
                }
            }
            while (!terminate);
            pipe.SetState(State.Null);
            Console.ReadLine();
        }

        void TestSimpleIdentity()
        {
            bool isReg1 = Gst.Plugin.RegisterStatic(1, 14, "simpleidentity", "simpleidentity plugin", 
                SimpleIdentity.RegisterPlugin, "0.1", "LGPL", "simpleidentity", "simpleidentity", "http://test");

            var path = Path.Combine(Program._rootDir, "Kickflip.mp3");
            var uri = (new System.Uri(path)).AbsoluteUri;

            var pipe = Gst.Parse.Launch($"uridecodebin uri={uri} ! simpleidentity ! fakesink");
            var bus = pipe.Bus;
            bool terminate = false;
            pipe.SetState(State.Playing);

            do
            {
                var msg = bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);
                switch (msg.Type)
                {
                    case MessageType.Error:
                        msg.ParseError(out var err, out var dbg);
                        Console.WriteLine(err.Message);
                        Console.WriteLine(dbg);
                        terminate = true;
                        break;
                    case MessageType.Eos:
                        Console.WriteLine("Eos");
                        terminate = true;
                        break;
                }
            }
            while (!terminate);
            pipe.SetState(State.Null);
            Console.ReadLine();
        }

    }
}
