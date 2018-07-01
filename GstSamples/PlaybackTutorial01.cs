//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Playbin usage
    /// </summary>
    static  class PlaybackTutorial01
    {
        struct CustomData
        {
            public Element Playbin; // Our one and only element 
            public int NVideo; // Number of embedded video streams 
            public int NAudio; // Number of embedded audio streams 
            public int NText; // Number of embedded subtitle streams 

            public int CurrentVideo; // Currently playing video stream 
            public int CurrentAudio; // Currently playing audio stream 
            public int CurrentText; // Currently playing subtitle stream 

            public GLib.MainLoop MainLoop;
        }

        static CustomData _data;

        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Gst.Application.Init(ref args);

            // Create the elements
            _data.Playbin = ElementFactory.Make("playbin", "playbin");

            if (_data.Playbin == null)
            {
                "Not all elements could be created".PrintErr();
                return;
            }

            // Set the uri to play
            _data.Playbin["uri"] = "http://freedesktop.org/software/gstreamer-sdk/data/media/sintel_cropped_multilingual.webm";
            //Data.Playbin["uri"] = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
            //Data.Playbin["uri"] = "https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm";

            // Set flags to show Audio and Video but ignore Subtitles 
            GstPlayFlags flags = (GstPlayFlags)_data.Playbin["flags"];
            flags |= (GstPlayFlags.Video | GstPlayFlags.Audio);
            flags &= (~GstPlayFlags.Text);
            _data.Playbin["flags"] = (uint)flags;

            // Set connection speed. This will affect some internal decisions of playbin2 
            _data.Playbin["connection-speed"] = 56;

            var bus = _data.Playbin.Bus;
            bus.AddSignalWatch();
            bus.Message += HandleMessage;

            // Add a keyboard watch so we get notified of keystrokes
            GLib.Idle.Add(HandleKeyboard);

            // Start playing
            var ret = _data.Playbin.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                "Unable to set the pipeline to the playing state".PrintErr();
                _data.Playbin.Dispose();
                return;
            }

            _data.MainLoop = new GLib.MainLoop();
            _data.MainLoop.Run();
            _data.Playbin.SetState(State.Null);
        }

        /// <summary>
        /// Process keyboard input
        /// </summary>
        static bool HandleKeyboard()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(false);
                if (char.IsDigit(key.KeyChar))
                {
                    var digit = Int32.Parse(key.KeyChar.ToString());

                    if (digit < 0 || digit > _data.NAudio)
                    {
                        Console.WriteLine("Index out of bounds");
                    }
                    else
                    {
                        // If the input was a valid audio stream index, set the current audio stream
                        Console.WriteLine("Setting current audio stream to {0}", digit);
                        _data.Playbin["current-audio"] = digit;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Process messages from GStreamer
        /// </summary>
        static void HandleMessage(object o, MessageArgs args)
        {
            var bus = o as Bus;
            var msg = args.Message;

            switch (msg.Type)
            {
                case MessageType.Error:
                    msg.ParseError(out GLib.GException err, out string debug);
                    $"Error received from element {msg.Src.Name}: {err.Message}".PrintErr();
                    $"Debugging information: {debug ?? "none"}".PrintErr();
                    return;
                case MessageType.Eos:
                    Console.WriteLine("End of stream reached");
                    _data.MainLoop.Quit();
                    break;
                case MessageType.StateChanged:
                    msg.ParseStateChanged(out State oldState, out State newState, out State pending);
                    if (msg.Src == _data.Playbin)
                    {
                        // Once we are in the playing state, analyze the streams
                        if (newState == State.Playing)
                            AnalyzeStreams();
                    }
                    break;
            }
            // We want to keep receiving messages
            args.RetVal = true;
        }

        /// <summary>
        /// Extract some metadata from the streams and print it on the screen
        /// </summary>
        static void AnalyzeStreams()
        {
            // Read some properties
            _data.NVideo = (int)_data.Playbin["n-video"];
            _data.NAudio = (int)_data.Playbin["n-audio"];
            _data.NText = (int)_data.Playbin["n-text"];

            Console.WriteLine($"{_data.NVideo} video streams, {_data.NAudio} audio streams, {_data.NText} text streams");
            string str;
            for (int i = 0; i < _data.NVideo; i++)
            {
                // Retrieve the stream's video tags 
                using (TagList tags = (TagList)_data.Playbin.Emit("get-video-tags", i))
                {
                    if (tags == null)
                        continue;
                    Console.WriteLine($"video stream {i}:");
                    if (tags.GetString(Gst.Constants.TAG_VIDEO_CODEC, out str))
                        Console.WriteLine("  codec: {0}", str ?? "unknown");
                    tags.Dispose();
                }
            }

            for (int i = 0; i < _data.NAudio; i++)
            {
                // Retrieve the stream's audio tags
                using (TagList tags = (TagList)_data.Playbin.Emit("get-audio-tags", i))
                {
                    if (tags == null)
                        continue;
                    Console.WriteLine($"audio stream {i}:");
                    if (tags.GetString(Gst.Constants.TAG_AUDIO_CODEC, out str))
                        Console.WriteLine("  codec: {0}", str ?? "unknown");
                    if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                        Console.WriteLine($"  language: {str}");
                    if (tags.GetUint(Gst.Constants.TAG_BITRATE, out uint rate))
                        Console.WriteLine($"  bitrage: {rate}");
                }
            }

            for (int i = 0; i < _data.NText; i++)
            {
                //Retrieve the stream's subtitle tags
                using (TagList tags = (TagList)_data.Playbin.Emit("get-text-tags", i))
                {
                    if (tags == null)
                        continue;
                    Console.WriteLine($"Subtitle stream {i}:");
                    if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                        Console.WriteLine($"  language: {str}");
                }
            }

            _data.CurrentVideo = (int)_data.Playbin.GetProperty("current-video");
            _data.CurrentAudio = (int)_data.Playbin.GetProperty("current-audio");
            _data.CurrentText = (int)_data.Playbin.GetProperty("current-text");

            Console.WriteLine($"Currently playing video stream {_data.CurrentVideo}, audio stream {_data.CurrentAudio} and text stream {_data.CurrentText}");
            Console.WriteLine("Type any number and hit ENTER to select a different audio stream");
        }
    }
}
