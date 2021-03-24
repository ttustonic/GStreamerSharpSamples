//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using GLib;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Playback speed
    /// </summary>
    static class BasicTutorial13
    {
        public class CustomData
        {
            public Element Pipeline;
            public Element VideoSink;
            public GLib.MainLoop Loop;
            public bool Playing;
            public double Rate;
        }

        static CustomData _data = new CustomData();

        public static void Run(string[] args)
        {
            Gst.Application.Init(ref args);

            // Print usage map
            Console.WriteLine(
@"USAGE: Choose one of the following option, then press enter:
 'P' to toggle between pause and play
 'S' to increase playback speed, 's' to decrease playback speed
 'D' to toggle playback direction
 'N' to move to next frame (in current direction, better in PAUSE)
 'Q' to quit
");
//            _data.Pipeline = Gst.Parse.Launch("playbin uri=https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm");
//            _data.Pipeline = Gst.Parse.Launch("playbin uri=http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4");
            _data.Pipeline = Gst.Parse.Launch("playbin uri=file:///U:/Video/test.mp4");

            // Add a keyboard watch so we get notified of keystrokes
            GLib.IOChannel ioStdin;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ioStdin= new GLib.IOChannel(0); // stdin = 0, stdout = 1, stderr = 2
            }
            else
            {
                ioStdin = null; // ?? how ??
            }
            ioStdin.AddWatch(0, GLib.IOCondition.In, HandleKeyboard);

            //Start playing
            var ret = _data.Pipeline.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                "Unable to set the pipeline to the playing state".PrintErr();
                _data.Pipeline.Dispose();
                return;
            }

            _data.Playing = true;
            _data.Rate = 1.0;

            // Create a GLib Main Loop and set it to run
            _data.Loop = new MainLoop();
            _data.Loop.Run();

            ioStdin.Dispose();
            _data.Pipeline.SetState(State.Null);
            if (_data.VideoSink != null)
                _data.VideoSink.Dispose();
            _data.Pipeline.Dispose();
        }

        /// <summary>
        /// Process keyboard input
        /// </summary>
        static bool HandleKeyboard(IOChannel source, IOCondition condition)
        {
            if (source.ReadLine(out string str) != IOStatus.Normal)
                return true;
            switch (Char.ToLower(str[0]))
            {
                case 'p':
                    _data.Playing = !_data.Playing;
                    var newState = _data.Playing ? State.Playing : State.Paused;
                    _data.Pipeline.SetState(newState);
                    Console.WriteLine("Setting state to {0}", newState);
                    break;
                case 's':
                    if (str[0] == 'S')
                        _data.Rate *= 2.0;
                    else
                        _data.Rate /= 2.0;
                    SendSeekEvent();
                    break;
                case 'd':
                    _data.Rate *= -1.0;
                    SendSeekEvent();
                    break;
                case 'n':
                    if (_data.VideoSink == null)
                    {
                        //If we have not done so, obtain the sink through which we will send the step events
                        _data.VideoSink = (Element)_data.Pipeline["video-sink"];
                    }
                    var stepEvent = Gst.Event.NewStep(Format.Buffers, 10, Math.Abs(_data.Rate), true, false);
                    _data.VideoSink.SendEvent(stepEvent);
                    Console.WriteLine("Stepping one frame");
                    break;
                case 'q':
                    _data.Loop.Quit();
                    break;
            }
            return true;
        }

        /// <summary>
        /// Send seek event to change rate
        /// </summary>
        static void SendSeekEvent()
        {
            var format = Format.Time;
            Gst.Event seekEvent;

            // obtain the current position, needed for the seek event
            if (!_data.Pipeline.QueryPosition(format, out long position))
            {
                "Unable to retrieve current position".PrintErr();
                return;
            }

            // Create the seek event
            if (_data.Rate > 0)
                seekEvent = Gst.Event.NewSeek(_data.Rate, Format.Time, SeekFlags.Flush | SeekFlags.Accurate, Gst.SeekType.Set, position, Gst.SeekType.None, 0);
            else
                seekEvent = Gst.Event.NewSeek(_data.Rate, Format.Time, SeekFlags.Flush | SeekFlags.Accurate, Gst.SeekType.Set, 0, Gst.SeekType.Set, position);

            if (_data.Rate < 0)
            {
                seekEvent.ParseSeek(out double rate0, out Gst.Format format0, out SeekFlags flags0, out Gst.SeekType startType0, out long start0, out Gst.SeekType stopType0, out long stop0);
                Console.WriteLine($"{rate0} :: {format0} :: {flags0} :: {startType0} :: {start0} :: {stopType0} :: {stop0} ");
            }

            if (_data.VideoSink == null)
            {
                //If we have not done so, obtain the sink through which we will send the step events
                _data.VideoSink = (Element)_data.Pipeline["video-sink"];
            }
            // Send the event
            _data.VideoSink.SendEvent(seekEvent);
            Console.WriteLine($"Current rate {_data.Rate}");
        }
    }
}
