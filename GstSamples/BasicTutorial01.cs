//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using Gst;

namespace GstSamples
{
    static class BasicTutorial01
    {
        public static void Run(string[] args)
        {
            // Initialize Gstreamer
            Application.Init(ref args);

            // Build the pipeline
// uri from CS samples
//            var pipeline = Parse.Launch("playbin uri=http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4");
// original uri
            var pipeline = Parse.Launch("playbin uri=http://freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm");
            // Start playing
            pipeline.SetState(State.Playing);

            // Wait until error or EOS
            var bus = pipeline.Bus;
            var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);

            // Free resources
            pipeline.SetState(State.Null);
        }
    }
}
