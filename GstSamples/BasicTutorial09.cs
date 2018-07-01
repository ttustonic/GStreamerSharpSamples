//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;
using Gst.PbUtils;

namespace GstSamples
{
    /// <summary>
    /// Media information gathering
    /// </summary>
    static class BasicTutorial09
    {
        static Discoverer Discoverer;
        static GLib.MainLoop MainLoop;

        public static void Run(string[] args)
        {
            GLib.ExceptionManager.UnhandledException += ExceptionManager_UnhandledException;

            string uri = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
// Missing GError ?
            uri = "https://www.freedesktop.org/software/gstreamer-sdk/data/media/sintel_trailer-480p.webm";
            if (args.Length > 1)
                uri = args[0];

            Gst.Application.Init(ref args);
            Console.WriteLine($"Discovering {uri}");

            Discoverer = new Discoverer(5 * Gst.Constants.SECOND);
            if (Discoverer == null)
            {
                Console.WriteLine("Error creating discoverer ");
                return;
            }

            // Connect to the interesting signals
            Discoverer.Discovered += OnDiscoveredCb;
            Discoverer.Finished += OnFinishedCb;

            // Start the discoverer process (nothing to do yet)
            Discoverer.Start();

            //Add a request to process asynchronously the URI passed through the command line 
            if (!Discoverer.DiscoverUriAsync(uri))
            {
                $"Failed to start discovering {uri}".PrintErr();
                return;
            }

            // Create a GLib Main Loop and set it to run, so we can wait for the signals
            MainLoop = new GLib.MainLoop();
            MainLoop.Run();

            Discoverer.Stop();
            Console.ReadLine();
        }

        static void ExceptionManager_UnhandledException(GLib.UnhandledExceptionArgs args)
        {
            Console.WriteLine("Unhandled exception");
        }

        /// <summary>
        /// This function is called every time the discoverer has information regarding one of the URIs we provided.
        /// </summary>
        static void OnDiscoveredCb(object o, DiscoveredArgs args)
        {
            var discoverer = o as Discoverer;
            var info = args.Info;
            var uri = info.Uri;
            var result = info.Result;

            switch (result)
            {
                case DiscovererResult.UriInvalid:
                    $"Invalid uri {uri}".PrintErr();
                    break;
                case DiscovererResult.Error:
                    var err = new GLib.GException(args.Error);
                    $"Discoverer error {err.Message}".PrintErr();
                    break;
                case DiscovererResult.Timeout:
                    Console.WriteLine("Timeout");
                    break;
                case DiscovererResult.Busy:
                    Console.WriteLine("Busy");
                    break;
                case DiscovererResult.MissingPlugins:
                    var s = info.Misc;
                    if (s != null)
                        Console.WriteLine($"Missing plugins {s}");
                    break;
                case DiscovererResult.Ok:
                    Console.WriteLine($"Discovered {uri}");
                    break;
            }
            if (result != DiscovererResult.Ok)
            {
                "This URI cannot be played".PrintErr();
                return;
            }

            // If we got no error, show the retrieved information
            var duration = new TimeSpan((long)info.Duration);
            Console.WriteLine($"Duration {duration}");

            var tags = info.Tags;
            if (tags != null)
            {
                Console.WriteLine("Tags: ");
                tags.Foreach(PrintTagForeach);
            }

            Console.WriteLine("Seekable: {0}", info.Seekable ? "yes" : "no");
            DiscovererStreamInfo sinfo = info.StreamInfo;
            if (sinfo == null)
                return;
            Console.WriteLine("Stream information: ");
            PrintTopology(sinfo, 1);
            Console.WriteLine();
        }

        /// <summary>
        /// Print information regarding a stream and its substreams, if any
        /// </summary>
        static void PrintTopology(DiscovererStreamInfo info, int depth)
        {
            if (info == null)
                return;
            PrintStreamInfo(info, depth);
            DiscovererStreamInfo next = info.Next;
            if (next != null)
            {
                PrintTopology(next, depth + 1);
                next.Dispose();
            }
            else if (info is DiscovererContainerInfo)
            {
                var streams = ((DiscovererContainerInfo)info).Streams;
                foreach (var stream in streams)
                    PrintTopology(stream, depth + 1);
            }
        }

        /// <summary>
        /// Print information regarding a stream
        /// </summary>
        static void PrintStreamInfo(DiscovererStreamInfo info, int depth)
        {
            var caps = info.Caps;
            string desc = String.Empty;
            if (caps != null)
            {
                if (caps.IsFixed)
                    desc = Gst.PbUtils.Global.PbUtilsGetCodecDescription(caps);
                else
                    desc = caps.ToString();
            }
            Console.WriteLine("{0}{1}: {2}", new string(' ', 2 * depth), info.StreamTypeNick, (desc != null ? desc : ""));

            TagList tags = info.Tags;
            if (tags != null)
            {
                Console.WriteLine("{0}Tags:", new string(' ', 2 * (depth + 1)));
                tags.Foreach((TagForeachFunc)delegate (TagList list, string tag)
                {
                    PrintTagForeach(list, tag);
                });
            }
        }

        static void PrintTagForeach(TagList list, string tag)
        {
            GLib.Value val = GLib.Value.Empty;
            TagList.CopyValue(ref val, list, tag);
            string str;

            if (val.Val is string)
                str = (string)(val.Val);
            else
                str = Gst.Value.Serialize(val);
            Console.WriteLine("{0} : {1}", Tag.GetNick(tag), str);
        }

        /// <summary>
        /// This function is called when the discoverer has finished examining all the URIs we provided.
        /// </summary>
        static void OnFinishedCb(object sender, EventArgs e)
        {
            Console.WriteLine("Finished discovering");
            MainLoop.Quit();
        }
    }
}
