//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Dynamic pipelines
    /// </summary>
    static class BasicTutorial03
    {
        static Pipeline pipeline; 
 		static Element source; 
 		static Element audioConvert; 
 		static Element audioSink;

        static Element videoConvert;
        static Element videoSink;

        public static void Run(string[] args)
        {
            Application.Init(ref args);

            source = ElementFactory.Make("uridecodebin", "source");
            audioConvert = ElementFactory.Make("audioconvert", "audioConvert");
            audioSink = ElementFactory.Make("autoaudiosink", "audioSink");

            videoConvert = ElementFactory.Make("videoconvert", "videoConvert");
            videoSink = ElementFactory.Make("autovideosink", "videoSink");

            pipeline = new Pipeline("test-pipeline");

            if (pipeline == null || source == null || audioSink == null || audioConvert == null)
            {
                Console.WriteLine("Not all elements could be created");
                return;
            }
            pipeline.Add(source, audioConvert, videoConvert, audioSink, videoSink);
            if (!audioConvert.Link(audioSink))
            {
                Console.WriteLine("audioSink could not be linked");
                return;
            }
            if (!videoConvert.Link(videoSink))
            {
                Console.WriteLine("videoSink could not be linked");
                return;
            }

            //            Error received from element source: Secure connection setup failed.
            //Debugging information gstsouphttpsrc.c(1377): gst_soup_http_src_parse_status(): / GstPipeline:test - pipeline / GstURIDecodeBin:source / GstSoupHTTPSrc:source:
            //            TLS / SSL support not available; install glib-networking(6), URL: http://freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm, Redirect to: https://freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm
            //http://www.gimpusers.com/forums/gimp-developer/19860-glib-networking-required-on-windows

            source["uri"] = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
            source["uri"] = "https://freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm";

            source.PadAdded += OnPadAdded;            
            var ret = pipeline.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                Console.WriteLine("Unable to set the pipeline to the playing state.");
                return;
            }
            var bus = pipeline.Bus;

            bool terminate = false;
            do
            {
                var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.StateChanged | MessageType.Error | MessageType.Eos);
                if (msg != null)
                {
                    switch (msg.Type)
                    {
                        case MessageType.Error:
                            msg.ParseError(out GLib.GException err, out string debug);
                            Console.WriteLine($"Error received from element {msg.Src.Name}: {err.Message}");
                            Console.WriteLine("Debugging information {0}", debug ?? "(none)");
                            terminate = true;
                            break;
                        case MessageType.Eos:
                            Console.WriteLine("End of stream reached");
                            break;
                        case MessageType.StateChanged:
                            if (msg.Src == pipeline)
                            {
                                msg.ParseStateChanged(out State oldState, out State newState, out State pending);
                                Console.WriteLine($"Pipeline state changed from {Element.StateGetName(oldState)} to {Element.StateGetName(newState)}");
                            }
                            break;
                        default:
                            Console.WriteLine("Unexpected message received");
                            break;
                    }
                }

            } while (!terminate);
            pipeline.SetState(State.Null);
            source.PadAdded -= OnPadAdded;
        }

        static void OnPadAdded(object sender, PadAddedArgs args)
        {
            var src = (Element)sender;
            var newPad = args.NewPad;

            var newPadCaps = newPad.CurrentCaps;
            var newPadStruct = newPadCaps.GetStructure(0);
            var newPadType = newPadStruct.Name;

            if (newPadType.StartsWith("audio/x-raw"))
            {
                Pad sinkPad = audioConvert.GetStaticPad("sink");
                Console.WriteLine(String.Format("Received new pad '{0}' from '{1}':", newPad.Name, src.Name));

                if (sinkPad.IsLinked)
                {
                    Console.WriteLine("We are already linked, ignoring");
                    return;
                }

                var ret = newPad.Link(sinkPad);
                if (ret == PadLinkReturn.Ok)
                    Console.WriteLine($"Link succeeded type {newPadType}");
                else
                    Console.WriteLine($"Type is {newPadType} but link failed");
            }
            else if (newPadType.StartsWith("video/x-raw"))
            {
                Pad sinkPad = videoConvert.GetStaticPad("sink");
                Console.WriteLine(String.Format("Received new pad '{0}' from '{1}':", newPad.Name, src.Name));
                if (sinkPad.IsLinked)
                {
                    Console.WriteLine("We are already linked, ignoring");
                    return;
                }
                var ret = newPad.Link(sinkPad);
                if (ret == PadLinkReturn.Ok)
                    Console.WriteLine($"Link succeeded type {newPadType}");
                else
                    Console.WriteLine($"Type is {newPadType} but link failed");
            }
            else
            {
                Console.WriteLine($"It has type '{newPadType}' which is not raw audio or video. Ignoring.");
                return;
            }
        }
    }
}
