//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Custom playbin sinks
    /// </summary>
    static class PlaybackTutorial07
    {
        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            /* Build the pipeline */
            var pipeline = Parse.Launch("playbin3 uri=https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm");

            // Create the elements inside the sink bin
            var equalizer = ElementFactory.Make("equalizer-3bands", "equalizer");
            var convert = ElementFactory.Make("audioconvert", "convert");
            var sink = ElementFactory.Make("autoaudiosink", "audio_sink");
            if (equalizer == null || convert == null || sink == null)
            {
                "Not all elements could be created".PrintErr();
                return;
            }

            /* Create the sink bin, add the elements and link them */
            var bin = new Bin("audio_sink_bin");
            bin.Add(equalizer, convert, sink);
            if (!Element.Link(equalizer, convert, sink))
            {
                "Elements could not be linked".PrintErr();
                return;
            }
            var pad = equalizer.GetStaticPad("sink");
            var ghostPad = new GhostPad("sink", pad);
            ghostPad.SetActive(true);
            bin.AddPad(ghostPad);

            /* Configure the equalizer */
            equalizer["band1"] = -24.0;
            equalizer["band2"] = -24.0;

            /* Set playbin's audio sink to be our sink bin */
            pipeline["audio-sink"] = bin;

            /* Start playing */
            pipeline.SetState(State.Playing);

            /* Wait until error or EOS */
            var bus = pipeline.Bus;
            var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);

            if (msg != null)
            {
                System.Console.WriteLine(msg.Type);
                msg.Dispose();
            }
            bus.Dispose();

            pipeline.SetState(State.Null);
            pipeline.Dispose();
        }
    }
}
