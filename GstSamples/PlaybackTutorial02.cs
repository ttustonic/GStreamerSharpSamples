//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Subtitle management
    /// </summary>
    static class PlaybackTutorial02
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
            _data.Playbin["uri"] = "https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.ogv";
            // Set the subtitle URI to play and some font description
// if setting this, AnalyzeStreams doesn't show subtitle streams when called from state change message.
// but Greek subtitles work
            _data.Playbin["suburi"] = "https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer_gr.srt";
            _data.Playbin["subtitle-font-desc"] = "Sans, 18";

            // Set flags to show Audio and Video and Subtitles 
            GstPlayFlags flags = (GstPlayFlags)_data.Playbin["flags"];
            flags |= GstPlayFlags.Audio | GstPlayFlags.Video | GstPlayFlags.Text ;
            _data.Playbin["flags"] = (uint)flags;

            // Add a bus watch, so we get notified when a message arrives
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

            // Create a GLib Main Loop and set it to run
            _data.MainLoop = new GLib.MainLoop();
            _data.MainLoop.Run();

            // Free resources
            bus.Dispose();
            _data.Playbin.SetState(State.Null);
            _data.Playbin.Dispose();
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
                    var index = Int32.Parse(key.KeyChar.ToString());

                    if (index < 0 || index > _data.NText)
                    {
                        Console.WriteLine("Index out of bounds");
                    }
                    else
                    {
                        // If the input was a valid subtitle stream index, set the current subtitle stream
                        Console.WriteLine("Setting current subtitle stream to {0}", index);
                        _data.Playbin["current-text"] = index;
                    }
                }
                else
                {
                    AnalyzeStreams();
                }
            }
            return true;
        }

        /// <summary>
        /// Process messages from GStreamer
        /// </summary>
        static void HandleMessage(object o, MessageArgs args)
        {
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
                TagList tags = (TagList)_data.Playbin.Emit("get-text-tags", i);
                {
                    if (tags == null)
                        continue;
                    Console.WriteLine("Subtitle stream: {0}", i);
                    if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                        Console.WriteLine($"  language: {str}");
                }
            }

            _data.CurrentVideo = (int)_data.Playbin.GetProperty("current-video");
            _data.CurrentAudio = (int)_data.Playbin.GetProperty("current-audio");
            _data.CurrentText = (int)_data.Playbin.GetProperty("current-text");

            Console.WriteLine($"Currently playing video stream {_data.CurrentVideo}, audio stream {_data.CurrentAudio} and text stream {_data.CurrentText}");
            Console.WriteLine("Type any number and hit ENTER to select a different subtitle stream");
        }
    }
}