//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Time management
    /// </summary>
    static class BasicTutorial4
    {
        static Element playbin;
        static bool playing;
        static bool terminate;
        static bool seekEnabled;
        static bool seekDone;
        static long duration = -1L;

        public static void Run(string[] args)
        {
            Application.Init(ref args);

            playbin = ElementFactory.Make("playbin", "playbin");
            if (playbin == null)
            {
                Console.WriteLine("Not all elements could be created");
                return;
            }

            playbin["uri"] = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
//            playbin["uri"] = @"file:///U:/Video/test.avi";

            var ret = playbin.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                Console.WriteLine("Unable to set the pipeline to the playing state");
                playbin.SetState(State.Null);
                return;
            }
            Bus bus = playbin.Bus;
            do
            {
                var msg = bus.TimedPopFiltered(100 * Constants.MSECOND, MessageType.StateChanged | MessageType.Error | MessageType.Eos | MessageType.DurationChanged);

                if (msg != null)
                {
                    HandleMessage(msg);
                }
                else
                {
                    if (playing)
                    {
                        var fmt = Format.Time;
                        var current = -1L;

                        if (!playbin.QueryPosition(fmt, out current))
                        {
                            Console.WriteLine("Could not query current position");
                        }
                        if (duration <= 0)
                        {
                            if (!playbin.QueryDuration(fmt, out duration))
                            {
                                Console.WriteLine("Could not query duration");
                            }
                        }
                        // Print current position and total duration
                        Console.WriteLine("Position {0} / {1} - SeekEnabled={2} - SeekDone={3}", 
                            new TimeSpan(current), new TimeSpan(duration), seekEnabled, seekDone);
                        if (seekEnabled && !seekDone && current > 10 * Constants.SECOND)
                        {
                            Console.WriteLine($"Performing seek current = {current}");
                            //                            playbin.Seek(1.0, Format.Time, SeekFlags.Flush, SeekType.None, 30 * Constants.SECOND, SeekType.End, -1);
//                            playbin.SetState(State.Paused) ;
                            playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, 30L * Constants.SECOND);
//                            playbin.SetState(State.Playing);
                            seekDone = true;
                        }
                    }
                }
            } while (!terminate);
            playbin.SetState(State.Null);
            Console.WriteLine("End");
        }

        static void HandleMessage(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Error:
                    msg.ParseError(out GLib.GException exc, out string debug);
                    Console.WriteLine(string.Format("Error received from element {0}: {1}", msg.Src.Name, exc.Message));
                    Console.WriteLine("Debugging information: {0}", debug);
                    terminate = true;
                    break;
                case MessageType.Eos:
                    Console.WriteLine("End of stream reached");
                    terminate = true;
                    break;
                case MessageType.DurationChanged:
                    duration = -1;
                    break;
                case MessageType.StateChanged:
                    if (msg.Src != playbin)
                        break;
                    msg.ParseStateChanged(out State oldState, out State newState, out State pending);
                    Console.WriteLine($"Pipeline state changed from {Element.StateGetName(oldState)} to {Element.StateGetName(newState)}");
                    playing = newState == State.Playing;
                    if (playing)
                    {
                        var query = Query.NewSeeking(Format.Time);
                        if (playbin.Query(query))
                        {
                            query.ParseSeeking(out Format fmt, out seekEnabled, out long start, out long end);
                            if (seekEnabled)
                                Console.WriteLine($"Seeking is ENABLED from {new TimeSpan(start)} to {new TimeSpan(end)}");
                            else
                                Console.WriteLine("Seeking is DISABLED for this stream");
                        }
                        else
                            Console.WriteLine("Seeking query failed");
                    }
                    break;
                default:
                    Console.WriteLine("Unexpected message received");
                    break;

            }
        }
    }
}
