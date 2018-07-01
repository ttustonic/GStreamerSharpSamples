//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Short-cutting the pipeline
    /// </summary>
    static class PlaybackTutorial03
    {
        const int CHUNK_SIZE = 1024;   // Amount of bytes we are sending in each buffer
        const int SAMPLE_RATE = 44100; // Samples per second we are sending

        struct CustomData
        {
            public Element Pipeline;
            public Gst.App.AppSrc AppSource;
            public long NumSamples; //Number of samples generated so far (for timestamp generation)
            public float a, b, c, d; // For waveform generation
            public uint SourceId; //To control the GSource
            public GLib.MainLoop MainLoop; //GLib's Main Loop
        }

        static CustomData _data;

        public static void Run(string[] args)
        {
            _data = new CustomData
            {
                b = 1, // for waveform generation
                d = 1,
            };

            // Initialize GStreamer
            Application.Init(ref args);

            // create the playbin element
            using (_data.Pipeline = Parse.Launch("playbin uri=appsrc://"))
            {
                _data.Pipeline.Connect("source-setup", HandleSourceSetup);

                // Instruct the bus to emit signals for each received message, and connect to the interesting signals
                using (var bus = _data.Pipeline.Bus)
                {
                    bus.AddSignalWatch();
                    bus.Connect("message::error", HandleError);

                    _data.Pipeline.SetState(State.Playing);

                    // Create a GLib Main Loop and set it to run
                    _data.MainLoop = new GLib.MainLoop();
                    _data.MainLoop.Run();

                    // Free resources
                    _data.Pipeline.SetState(State.Null);
                }
            }
        }

        /// <summary>
        /// This function is called when playbin has created the appsrc element, so we have
        /// a chance to configure it.
        /// </summary>
        static void HandleSourceSetup(object o, GLib.SignalArgs args)
        {
            var pipeline = o as Pipeline;
            Console.WriteLine("Source has been created. Configuring.");

            //AppSource = (Element)args.Args[0];
            _data.AppSource = new Gst.App.AppSrc(((Element)args.Args[0]).Handle); ;
            // Configure appsrc
            var info = new Gst.Audio.AudioInfo();
            Gst.Audio.AudioChannelPosition[] position = { };
            info.SetFormat(Gst.Audio.AudioFormat.S16, SAMPLE_RATE, 1, position);
            var audioCaps = info.ToCaps();
            _data.AppSource["caps"] = audioCaps;
            _data.AppSource["format"] = Format.Time;
            _data.AppSource.NeedData += HandleStartFeed;
            _data.AppSource.EnoughData += HandleStopFeed;
//            AppSource.Connect("need-data", HandleStartFeed);
//            AppSource.Connect("enough-data", HandleStopFeed);
            audioCaps.Dispose();
        }

        /// <summary>
        /// This callback triggers when appsrc has enough data and we can stop sending.
        /// We remove the idle handler from the mainloop
        /// </summary>
        static void HandleStopFeed(object o, EventArgs args)
        {
            if (_data.SourceId == 0)
                return;
            Console.WriteLine("Stop feeding");
            GLib.Source.Remove(_data.SourceId);
            _data.SourceId = 0;
        }

        static void HandleStartFeed(object o, Gst.App.NeedDataArgs args)
        {
            if (_data.SourceId != 0)
                return;
            Console.WriteLine("Start feeding");
            _data.SourceId = GLib.Idle.Add(PushData);
        }

        /// <summary>
        /// This method is called by the idle GSource in the mainloop, to feed CHUNK_SIZE bytes into appsrc.
        /// The idle handler is added to the mainloop when appsrc requests us to start sending data(need-data signal)
        /// and is removed when appsrc has enough data(enough-data signal).
        /// </summary>
        static bool PushData()
        {
            var numSamples = CHUNK_SIZE / 2; //Because each sample is 16 bits

            // Create a new empty buffer
            using (var buffer = new Gst.Buffer(null, CHUNK_SIZE, AllocationParams.Zero))
            {
                //Set its timestamp and duration 
                buffer.Pts = Util.Uint64Scale((ulong)_data.NumSamples, (ulong)Constants.SECOND, (ulong)SAMPLE_RATE);
                buffer.Dts = Util.Uint64Scale((ulong)_data.NumSamples, (ulong)Constants.SECOND, (ulong)SAMPLE_RATE);
                buffer.Duration = Util.Uint64Scale((ulong)_data.NumSamples, (ulong)Constants.SECOND, (ulong)SAMPLE_RATE);

                // Generate some psychodelic waveforms
                buffer.Map(out MapInfo map, MapFlags.Write);

//  See BasicTutorial08 for the managed version
                unsafe
                {
                    var raw = (short*)map.DataPtr;
                    _data.c += _data.d;
                    _data.d -= _data.c / 1000f;
                    var freq = 1100f + 1000f * _data.d;
                    for (int i = 0; i < numSamples; i++)
                    {
                        _data.a += _data.b;
                        _data.b -= _data.a / freq;
                        raw[i] = (short)(500 * _data.a);
                    }
                }
                buffer.Unmap(map);
                _data.NumSamples += numSamples;

                // push the buffer into the appsrc
                var ret = _data.AppSource.PushBuffer(buffer);

                return (ret == FlowReturn.Ok);
                // Free the buffer now that we are done with it
            }
        }

        /// <summary>
        /// This function is called when an error message is posted on the bus
        /// </summary>
        static void HandleError(object o, GLib.SignalArgs args)
        {
            var msg = (Message)args.Args[0];

            // Print error details on the screen
            msg.ParseError(out GLib.GException err, out string debug);
            Console.WriteLine($"Error received from element {msg.Src.Name}: {err.Message}");
            Console.WriteLine("Debugging information: {0}", debug ?? "none");

            _data.MainLoop.Quit();
        }
    }
}
