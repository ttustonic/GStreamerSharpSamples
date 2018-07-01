//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Linq;
using GLib;
using Gst;
using Gst.App;

namespace GstSamples
{
    /// <summary>
    /// Short-cutting the pipeline
    /// </summary>
    static class BasicTutorial08
    {
        class CustomData
        {
            public Pipeline Pipeline;
            public Gst.App.AppSrc AppSource;
            public Element Tee;
            public Element AudioQueue;
            public Element AudioConvert1;
            public Element AudioResample;
            public Element AudioSink;
            public Element VideoQueue;
            public Element AudioConvert2;
            public Element Visual;
            public Element VideoConvert;
            public Element VideoSink;
            public Element AppQueue;
            public Gst.App.AppSink AppSink;
            public long NumSamples;
            public float a, b, c, d;
            public uint SourceId = 0;
            public GLib.MainLoop MainLoop;
        }

        const int CHUNK_SIZE = 1024;   /* Amount of bytes we are sending in each buffer */
        const int SAMPLE_RATE = 44100;  /* Samples per second we are sending */
        static CustomData Data = new CustomData();

        public static void Run(string[] args)
        {
            var info = new Gst.Audio.AudioInfo();
            Data.b = 1;
            Data.d = 1;
            Gst.Application.Init(ref args);

            Data.AppSource = new Gst.App.AppSrc("audio_source");
            Data.Tee = ElementFactory.Make("tee", "tee");
            Data.AudioQueue = ElementFactory.Make("queue", "audio_queue");
            Data.AudioConvert1 = ElementFactory.Make("audioconvert", "audio_convert1");
            Data.AudioResample = ElementFactory.Make("audioresample", "audio_resample");
            Data.AudioSink = ElementFactory.Make("autoaudiosink", "audio_sink");
            Data.VideoQueue = ElementFactory.Make("queue", "video_queue");
            Data.AudioConvert2 = ElementFactory.Make("audioconvert", "audio_convert2");
            Data.Visual = ElementFactory.Make("wavescope", "visual");
            Data.VideoConvert = ElementFactory.Make("videoconvert", "csp");
            Data.VideoSink = ElementFactory.Make("autovideosink", "video_sink");
            Data.AppQueue = ElementFactory.Make("queue", "app_queue");
            Data.AppSink = new Gst.App.AppSink("app_sink");
            Data.Pipeline = new Pipeline("test-pipeline");

            if (new[] {Data.Pipeline, Data.AppSource, Data.Tee, Data.AudioQueue,
                        Data.AudioConvert1, Data.AudioResample, Data.AudioSink, Data.VideoQueue,
                        Data.AudioConvert2, Data.Visual, Data.VideoConvert, Data.VideoSink, Data.AppQueue, Data.AppSink }.Any(el => el == null))
            {
                "Not all elements could be created".PrintErr();
                return;
            }

            // Configure wavescope
            Data.Visual["shader"] = 0;
            Data.Visual["style"] = 0;

            // Configure appsrc
            info.SetFormat(Gst.Audio.AudioFormat.S16, SAMPLE_RATE, 1);
            var audioCaps = info.ToCaps();
            Data.AppSource["caps"] = audioCaps;
            Data.AppSource["format"] = Format.Time;
            Data.AppSource.Connect("need-data", StartFeed);
//            Data.AppSource.NeedData += StartFeed;
            Data.AppSource.EnoughData += StopFeed;

            // Configure appsink          
            Data.AppSink.EmitSignals = true;
            Data.AppSink.Caps = audioCaps;
            Data.AppSink.NewSample += NewSample;

            // Link all elements that can be automatically linked because they have "Always" pads
            Data.Pipeline.Add(Data.AppSource, Data.Tee, Data.AudioQueue, Data.AudioConvert1, Data.AudioResample,
                Data.AudioSink, Data.VideoQueue, Data.AudioConvert2, Data.Visual, Data.VideoConvert, Data.VideoSink, Data.AppQueue, Data.AppSink);
            if (!Element.Link(Data.AppSource, Data.Tee) ||
                !Element.Link(Data.AudioQueue, Data.AudioConvert1, Data.AudioResample, Data.AudioSink) ||
                !Element.Link(Data.VideoQueue, Data.AudioConvert2, Data.Visual, Data.VideoConvert, Data.VideoSink) ||
                !Element.Link(Data.AppQueue, Data.AppSink))
            {
                "Elements could not be linked.".PrintErr();
                return;
            }

            // Manually link the Tee, which has "Request" pads
            var teeAudioPad = Data.Tee.GetRequestPad("src_%u");
            Console.WriteLine($"Obtained request pad {teeAudioPad.Name} for audio branch.");
            var queueAudioPad = Data.AudioQueue.GetStaticPad("sink");
            var teeVideoPad = Data.Tee.GetRequestPad("src_%u");
            Console.WriteLine($"Obtained request pad {teeVideoPad.Name} for video branch.");
            var queueVideoPad = Data.VideoQueue.GetStaticPad("sink");
            var teeAppPad = Data.Tee.GetRequestPad("src_%u");
            Console.WriteLine($"Obtained request pad {teeAppPad.Name} for app branch.");
            var queueAppPad = Data.AppQueue.GetStaticPad("sink");

            if ((teeAudioPad.Link(queueAudioPad) != PadLinkReturn.Ok) ||
                (teeVideoPad.Link(queueVideoPad) != PadLinkReturn.Ok) ||
                (teeAppPad.Link(queueAppPad) != PadLinkReturn.Ok))
            {
                "tee could not be linked".PrintErr();
                return;
            }

            // Instruct the bus to emit signals for each received message, and connect to the interesting signals

            var bus = Data.Pipeline.Bus;
            bus.AddSignalWatch();
            bus.Connect("message::error", HandleError);

            // Start playing the pipeline
            Data.Pipeline.SetState(State.Playing);

            Data.MainLoop = new GLib.MainLoop();
            Data.MainLoop.Run();

            // Release the request pads from the Tee, and unref them
            Data.Tee.ReleaseRequestPad(teeAudioPad);
            Data.Tee.ReleaseRequestPad(teeVideoPad);
            Data.Tee.ReleaseRequestPad(teeAppPad);

            // Free resources
            Data.Pipeline.SetState(State.Null);

            Gst.Global.Deinit();
        }

        static void StartFeed(object o, SignalArgs args)
        {
            if (Data.SourceId != 0)
                return;
            Console.WriteLine("Start feeding");
            Data.SourceId = GLib.Idle.Add(PushData);
        }

        /// <summary>
        /// This callback triggers when appsrc has enough data and we can stop sending.
        /// We remove the idle handler from the mainloop
        /// </summary>
        static void StopFeed(object sender, System.EventArgs e)
        {
            if (Data.SourceId == 0)
                return;
            Console.WriteLine("Stop feeding");
            GLib.Source.Remove(Data.SourceId);
            Data.SourceId = 0;
        }
    
        /// <summary>
        /// This method is called by the idle GSource in the mainloop, to feed CHUNK_SIZE bytes into appsrc.
        /// The ide handler is added to the mainloop when appsrc requests us to start sending data(need-data signal)
        /// and is removed when appsrc has enough data(enough-data signal).
        /// </summary>
        static bool PushData()
        {
            var numSamples = CHUNK_SIZE / 2;
//            var numSamples = CHUNK_SIZE ;

            // Create a new empty buffer
            Gst.Buffer buffer = new Gst.Buffer(null, CHUNK_SIZE, AllocationParams.Zero);

            //Set its timestamp and duration 
            buffer.Pts = Util.Uint64Scale((ulong)Data.NumSamples, (ulong)Gst.Constants.SECOND, (ulong)SAMPLE_RATE);
            buffer.Dts = Util.Uint64Scale((ulong)Data.NumSamples, (ulong)Gst.Constants.SECOND, (ulong)SAMPLE_RATE);
            buffer.Duration = Util.Uint64Scale((ulong)Data.NumSamples, (ulong)Gst.Constants.SECOND, (ulong)SAMPLE_RATE);

            // Generate some psychodelic waveforms
            buffer.Map(out MapInfo map, MapFlags.Write);
// map.Data is a marshal.copy of IntPtr, that's why I can't just set the array values.
//  See PlaybackTutorial03 for the unsafe version
            var raw = map.Data;
            Data.c += Data.d;
            Data.d -= Data.c / 1000;
            float freq = 1100 + 1000 * Data.d;
            for (int i=0; i<numSamples; i++)
            {
                Data.a += Data.b;
                Data.b -= Data.a / freq;
                var sh = (short)(500 * Data.a);
                var bytes = BitConverter.GetBytes(sh);
                raw[2*i] = bytes[0];
                raw[2*i + 1] = bytes[1];
            }
            map.Data = raw;
            buffer.Unmap(map);
            Data.NumSamples += numSamples;

            // push the buffer into the appsrc
            var ret = Data.AppSource.PushBuffer(buffer);
            buffer.Dispose();

            return (ret == FlowReturn.Ok);
        }

        /// <summary>
        /// The appsink has received a buffer
        /// </summary>
        static void NewSample(object o, NewSampleArgs args)
        {
            AppSink sink = o as AppSink;

            // Retrieve the buffer
            using (var sample = sink.PullSample())
            {
                if (sample == null)
                    return;
                // The only thing we do in this example is print a * to indicate a received buffer
                Console.Write("* ");
                sample.Dispose();
            }
        }

        // This function is called when an error message is posted on the bus
        static void HandleError(object sender, GLib.SignalArgs args)
        {
            var msg = (Message)args.Args[0];

            // Print error details on the screen
            msg.ParseError(out GException err, out string debug);
            Console.WriteLine($"Error received from element {msg.Src.Name}: {err.Message}", msg.Src.Name, err.Message);
            Console.WriteLine("Debugging information: {0}", debug ?? "none");
            Data.MainLoop.Quit();
        }
    }
}
