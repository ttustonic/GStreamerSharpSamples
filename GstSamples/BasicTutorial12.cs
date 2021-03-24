//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Streaming
    /// </summary>
    static class BasicTutorial12
    {
        struct CustomData
        {
            public bool IsLive;
            public Element Pipeline;
            public GLib.MainLoop MainLoop;
        }

        static CustomData _data;

        public static void Run(string[] args)
        {
            _data = new CustomData();

            Gst.Application.Init(ref args);
            _data.Pipeline = Gst.Parse.Launch("playbin uri=https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm");
            var bus = _data.Pipeline.Bus;

            //start playing
            var ret = _data.Pipeline.SetState(Gst.State.Playing);
            if(ret == StateChangeReturn.Failure)
            {
                "Unable to set pipeline to the playing state".PrintErr();
                return;
            }
            else if (ret == StateChangeReturn.NoPreroll)
            {
                _data.IsLive = true;
            }

            _data.MainLoop = new GLib.MainLoop();

            bus.AddSignalWatch();
            bus.Message += CbMessage;
            _data.MainLoop.Run();

            _data.Pipeline.SetState(State.Null);
            bus.Dispose();
            _data.Pipeline.Dispose();
        }

        static void CbMessage(object o, MessageArgs args)
        {
            var msg = args.Message;
            switch (msg.Type)
            {
                case MessageType.Error:
                    msg.ParseError(out GLib.GException err, out string debug);
                    Console.WriteLine($"Error {err.Message}");
                    _data.Pipeline.SetState(State.Ready);
                    _data.MainLoop.Quit();
                    break;
                case MessageType.Eos:
                    _data.Pipeline.SetState(State.Ready);
                    _data.MainLoop.Quit();
                    break;
                case MessageType.Buffering:
                    if (_data.IsLive)
                        break;
                    var percent = msg.ParseBuffering();
                    Console.WriteLine($"Buffering {percent}%");
                    // Wait until buffering is complete before start/resume playing
                    if (percent < 100)
                        _data.Pipeline.SetState(State.Paused);
                    else
                        _data.Pipeline.SetState(State.Playing);
                    break;
                case MessageType.ClockLost:
                    // GEt a new clock
                    _data.Pipeline.SetState(State.Paused);
                    _data.Pipeline.SetState(State.Playing);
                    break;
                default:
                    // Unhandled message
                    break;
            }
        }
    }
}
