using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Dynamic pipelines example, uridecodebin with sinks added and removed
    /// <para>
    /// https://github.com/sdroege/gst-snippets/blob/master/dynamic-tee-vsink.c
    /// </para>
    /// </summary>
    static class DynamicTeeVsink
    {
        struct SinkStruct
        {
            public Pad teePad;
            public Element queue;
            public Element conv;
            public Element sink;
            public int removing;
        }

        static GLib.MainLoop _loop;
        static Pipeline _pipeline;
        /// <summary>filesrc</summary>
        static Element _src;
        /// <summary>decodebin</summary>
        static Element _dbin;
        /// <summary>videoconvert</summary>
        static Element _conv;
        static Element _tee;
        static bool _linked;

        static LinkedList<SinkStruct> _sinks = new LinkedList<SinkStruct>();

        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Gst.Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            string fileName = @"U:/Video/test.avi";

            _pipeline = new Pipeline();
            _src = ElementFactory.Make("filesrc");
            _dbin = ElementFactory.Make("decodebin");
            _conv = ElementFactory.Make("videoconvert");
            _tee = ElementFactory.Make("tee");

            if (new[] { _pipeline, _src, _dbin, _conv, _tee }.Any(e => e == null))
            {
                "Failed to create elements".PrintErr();
                return;
            }
            _src["location"] = fileName;

            _pipeline.Add(_src, _dbin, _conv, _tee);
            if (!Element.Link(_src, _dbin) || !Element.Link(_conv, _tee))
            {
                "Failed to link elements".PrintErr();
                return;
            }

            _dbin.PadAdded += PadAddedCb;
            _loop = new GLib.MainLoop();
            var bus = _pipeline.Bus;
            bus.AddSignalWatch();
            bus.Message += MessageCb;

            if (_pipeline.SetState(State.Playing) == StateChangeReturn.Failure)
            {
                "Failed to go into PLAYING state".PrintErr();
                return;
            }
            _loop.Run();
            _pipeline.SetState(State.Null);
            _pipeline.Dispose();
        }

        static void PadAddedCb(object o, PadAddedArgs args)
        {
            if (_linked)
                return;
            var src = (Element)o;
            var pad = args.NewPad;
            using (var caps = pad.CurrentCaps)
            {
                var s = caps.GetStructure(0);
                var name = s.Name;
                if (!name.StartsWith("video/x-raw"))
                    return;

                using (var sinkPad = _conv.GetStaticPad("sink"))
                {
                    if (pad.Link(sinkPad) != PadLinkReturn.Ok)
                    {
                        "Failed to link dbin with conb".PrintErr();
                        _loop.Quit();
                        return;
                    }
                }

                PadTemplate templ = _tee.GetPadTemplate("src_%u");
                Pad teePad = _tee.RequestPad(templ);
                var queue = ElementFactory.Make("queue");
                var sink = ElementFactory.Make("fakesink");
                sink["sync"] = true;

                _pipeline.Add(queue, sink);
                Element.Link(queue, sink);

                using (var sinkPad = queue.GetStaticPad("sink"))
                {
                    teePad.Link(sinkPad);
                }
                GLib.Timeout.AddSeconds(3, TickCb);
                _linked = true;
            }
        }

        static Random random = new Random();

        static bool TickCb()
        {
            if (_sinks.Count == 0 || random.Next() % 2 == 0)
            {
                SinkStruct sink = new SinkStruct();
                PadTemplate templ = _tee.GetPadTemplate("src_%u");
                Console.Write("Add ... ");
                sink.teePad = _tee.RequestPad(templ);
                sink.queue = ElementFactory.Make("queue");
                sink.conv = ElementFactory.Make("videoconvert");
                sink.sink = ElementFactory.Make("autovideosink");
                sink.removing = 0;

                _pipeline.Add(sink.queue, sink.conv, sink.sink);
                if (!Element.Link(sink.queue, sink.conv, sink.sink))
                {
                    "Cannot link elements".PrintErr();
                    return false;
                }
                sink.queue.SyncStateWithParent();
                sink.conv.SyncStateWithParent();
                sink.sink.SyncStateWithParent();

                using (var sinkPad = sink.queue.GetStaticPad("sink"))
                {
                    sink.teePad.Link(sinkPad);
                }
                Console.WriteLine("added");
                _sinks.AddLast(sink);
            }
            else
            {
                Console.Write("remove ... ");

                var sink = _sinks.Last.Value;
                _sinks.RemoveLast();
                sink.teePad.AddProbe(PadProbeType.Idle, GetUnlinkCb(sink));
            }
            return true;
        }

        static PadProbeCallback GetUnlinkCb(object userData)
        {
            SinkStruct sink = (SinkStruct)userData;
            PadProbeReturn UnlinkCb(Pad pad, PadProbeInfo info)
            {
                if (Interlocked.CompareExchange(ref sink.removing, 0, 1) == 1)
                    return PadProbeReturn.Ok;

                using (var sinkPad = sink.queue.GetStaticPad("sink"))
                {
                    sink.teePad.Unlink(sinkPad);
                }

                _pipeline.Remove(sink.queue);
                _pipeline.Remove(sink.conv);
                _pipeline.Remove(sink.sink);

                sink.sink.SetState(State.Null);
                sink.conv.SetState(State.Null);
                sink.queue.SetState(State.Null);

                sink.queue.Dispose();
                sink.conv.Dispose();
                sink.sink.Dispose();
                _tee.ReleaseRequestPad(sink.teePad);
                sink.teePad.Dispose();

                Console.WriteLine("removed");

                return PadProbeReturn.Remove;
            }

            return UnlinkCb;
        }

        static void MessageCb(object o, MessageArgs args)
        {
            var bus = o as Bus;
            var message = args.Message;
            switch (message.Type)
            {
                case MessageType.Error:
                    {
                        var name = message.Src.PathString;
                        message.ParseError(out GLib.GException err, out string debug);
                        $"ERROR: from element {name}: {err.Message}".PrintErr();
                        if (debug != null)
                            $"Additional debug info:\n{debug}".PrintErr();
                        break;
                    }
                case MessageType.Warning:
                    {
                        var name = message.Src.PathString;
                        message.ParseWarning(out IntPtr err, out string debug);
                        Console.WriteLine("WARNING");
                        break;
                    }
                case MessageType.Eos:
                    {
                        Console.WriteLine("EOS");
                        _loop.Quit();
                        break;
                    }
            }

        }

    }
}
