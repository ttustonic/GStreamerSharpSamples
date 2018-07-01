//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Linq;
using Gst;
using Gst.Video;

namespace GstSamples
{
    /// <summary>
    /// Color Balance
    /// </summary>
    static class PlaybackTutorial05
    {
        static Element _pipeline;
        static GLib.MainLoop _loop;

        public static void Run(string[] args)
        {
            Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            Console.WriteLine(
@"USAGE: Choose one of the following options, then press enter:
 'C' to increase contrast, 'c' to decrease contrast
 'B' to increase brightness, 'b' to decrease brightness
 'H' to increase hue, 'h' to decrease hue
 'S' to increase saturation, 's' to decrease saturation
 'Q' to quit");

            using (_pipeline = Parse.Launch("playbin uri=https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm"))
            {
                GLib.Idle.Add(HandleKeyboard);

                // Start playing
                var ret = _pipeline.SetState(State.Playing);
                if (ret == StateChangeReturn.Failure)
                {
                    "Unable to set the pipeline to the playing state.".PrintErr();
                    return;
                }

                PrintCurrentValues();

                // Create a GLib Main Loop and set it to run
                _loop = new GLib.MainLoop();
                _loop.Run();

                // Free resources
                _pipeline.SetState(State.Null);
            }
        }

        static void PrintCurrentValues()
        {
            var cb = new Gst.Video.ColorBalanceAdapter(_pipeline.Handle);
            var channels = cb.ListChannels(); 

            for (int i = 0; i < channels.Length; i++)
            {
                var channel = channels[i];
                var value = cb.GetValue(channel);
                Console.WriteLine("{0}: {1}", channel.Label, 100 * (value - channel.MinValue) / (channel.MaxValue - channel.MinValue));
            }
        }

        static bool HandleKeyboard()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(false);
                var cb = new ColorBalanceAdapter(_pipeline.Handle);

                switch (key.Key)
                {
                    case ConsoleKey.C:
                        UpdateColorChannel("CONTRAST", key.Modifiers == ConsoleModifiers.Shift, cb);
                        break;
                    case ConsoleKey.B:
                        UpdateColorChannel("BRIGHTNESS", key.Modifiers == ConsoleModifiers.Shift, cb);
                        break;
                    case ConsoleKey.H:
                        UpdateColorChannel("HUE", key.Modifiers == ConsoleModifiers.Shift, cb);
                        break;
                    case ConsoleKey.S:
                        UpdateColorChannel("SATURATION", key.Modifiers == ConsoleModifiers.Shift, cb);
                        break;
                    case ConsoleKey.Q:
                        _loop.Quit();
                        break;
                }
                PrintCurrentValues();
            }
            return true;
        }

        static void UpdateColorChannel(string channelName, bool increase, IColorBalance cb)
        {
            // Retrieve the list of channels and locate the requested one
            var channels = cb.ListChannels();
            ColorBalanceChannel channel = channels.FirstOrDefault(cbc => cbc.Label.Contains(channelName));
            if (channel == null)
                return;

            // Change the channel's value
            var step = 0.1 * (channel.MaxValue - channel.MinValue);
            var value = cb.GetValue(channel);
            if (increase)
            {
                value = (int)(value + step);
                if (value > channel.MaxValue)
                    value = channel.MaxValue;
            }
            else
            {
                value = (int)(value - step);
                if (value < channel.MinValue)
                    value = channel.MinValue;
            }
            cb.SetValue(channel, value);
        }
    }
}
