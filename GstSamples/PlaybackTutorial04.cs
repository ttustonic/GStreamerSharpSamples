//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using GLib;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Progressive streaming
    /// </summary>
    static class PlaybackTutorial04
    {
        const int GRAPH_LENGTH = 78;
        const uint GST_PLAY_FLAG_DOWNLOAD = (1 << 7);
        static bool _isLive = false;
        static Element _pipeline;
        static GLib.MainLoop _loop;
        static int _bufferingLevel;

        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Gst.Application.Init(ref args);

            _bufferingLevel = 100;

            // Build the pipeline
            /* error when setting download flag
                                    using (_pipeline = Parse.Launch("playbin uri=https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm"))
                        Error: The stream is in the wrong format.
                        ../gst-libs/gst/audio/gstaudiobasesink.c(1117): gst_audio_base_sink_wait_event (): /GstPlayBin:playbin0/GstPlaySink:playsink/GstBin:abin/GstWasapiSink:wasapisink0:
                        Sink not negotiated before eos event.
            */

            using (_pipeline = Parse.Launch("playbin uri=https://download.blender.org/durian/trailer/sintel_trailer-1080p.ogv")) // Works
//            using (_pipeline = Parse.Launch("playbin uri=https://dash.akamaized.net/akamai/bbb_30fps/bbb_30fps_1920x1080_8000k.mpd")) // works
//          using (_pipeline = Parse.Launch("playbin uri=file:///U:/Video/test.mp4")) // works
            {
                var bus = _pipeline.Bus;

                var flags = _pipeline.Flags;
                flags |= GST_PLAY_FLAG_DOWNLOAD;
                _pipeline.Flags = flags;

                // Uncomment this line to limit the amount of downloaded data
                _pipeline["ring-buffer-max-size"] = 4000000L;

                // Start playing
                var ret = _pipeline.SetState(State.Playing);
                if (ret == StateChangeReturn.Failure)
                {
                    "Unable to set the pipeline to the playing state.".PrintErr();
                    _pipeline.Dispose();
                    return;
                }
                else if (ret == StateChangeReturn.NoPreroll)
                    _isLive = true;

                _loop = new GLib.MainLoop();

                bus.AddSignalWatch();
                bus.Message += CbMessage;
                _pipeline.Connect("deep-notify::temp-location", GotLocation);

                // Register a function that GLib will call every second
                GLib.Timeout.AddSeconds(1, RefreshUI);

                _loop.Run();

                _pipeline.SetState(State.Null);
                bus.Dispose();
            }
        }

        static bool RefreshUI()
        {
            var query = new Query(Format.Percent);
            var result = _pipeline.Query(query);
            if (result)
            {
                var graph = new char[GRAPH_LENGTH];
                for (int i = 0; i < GRAPH_LENGTH; i++)
                    graph[i] = ' ';

                var nRanges = query.NBufferingRanges;
                for (uint range = 0; range < nRanges; range++)
                {
                    query.ParseNthBufferingRange(range, out long start, out long stop);
                    start = start * GRAPH_LENGTH / (stop - start);
                    stop = stop * GRAPH_LENGTH / (stop - start);
                    for (int i = (int)start; i < stop; i++)
                        graph[i] = '-';
                }
                if (_pipeline.QueryPosition(Format.Time, out long position)
                    && position >= 0
                    && _pipeline.QueryDuration(Format.Time, out long duration)
                    && duration > 0)
                {
                    var i = (int)(GRAPH_LENGTH * (double)position / (double)(duration + 1));
                    graph[i] = _bufferingLevel < 100 ? 'X' : '>';
                }
                Console.Write(new String(graph));
                if (_bufferingLevel < 100)
                    Console.Write($"Buffering {_bufferingLevel}");
                else
                    Console.Write($"                ");
                Console.Write("\r");
            }
            return true;
        }

        static void GotLocation(object o, SignalArgs args)
        {
            var propObject = args.Args[0] as Gst.Object;
            var location = (string)propObject.GetProperty("temp-location");
            Console.WriteLine($"Temporary file {location}");
            // Uncomment this line to keep the temporary file after the program exits
            // propObject["temp-remove"] = false;
        }

        static void CbMessage(object o, MessageArgs args)
        {
            var msg = args.Message;

            switch (args.Message.Type)
            {
                case MessageType.Error:
                    msg.ParseError(out GLib.GException err, out string debug);
                    $"Error: {err.Message}\n{debug}".PrintErr();
                    _pipeline.SetState(State.Ready);
                    _loop.Quit();
                    break;
                case MessageType.Eos:
                    // end-of-stream
                    _pipeline.SetState(State.Ready);
                    _loop.Quit();
                    break;
                case MessageType.Buffering:
                    // If the stream is live, we do not care about buffering.
                    if (_isLive)
                        break;
                    _bufferingLevel = msg.ParseBuffering();
                    if (_bufferingLevel < 100)
                        _pipeline.SetState(State.Paused);
                    else
                        _pipeline.SetState(State.Playing);
                    break;
                case MessageType.ClockLost:
                    // Get a new clock
                    _pipeline.SetState(State.Paused);
                    _pipeline.SetState(State.Playing);
                    break;
                default:
                    // Unhandled message
                    break;
            }
        }
    }
}
