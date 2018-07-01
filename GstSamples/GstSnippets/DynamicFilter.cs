using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Dynamic pipelines example, uridecodebin with a switching video filter
    /// <para>
    /// https://github.com/sdroege/gst-snippets/blob/master/dynamic-filter.c
    /// </para>
    /// </summary>
    static class DynamicFilter
    {
        static GLib.MainLoop _loop;
        static Pipeline _pipeline;
        /// <summary>filesrc</summary>
        static Element _src;
        /// <summary>decodebin</summary>
        static Element _dbin;
        /// <summary>videoconvert</summary>
        static Element _conv;
        /// <summary>videoscale</summary>
        static Element _scale;
        ///// <summary>navseek: Seek based on left-right arrows</summary>
        static Element _navseek;
        /// <summary>queue</summary>
        static Element _queue;
        /// <summary>autovideosink </summary>
        static Element _sink;
        /// <summary>videoconvert </summary>
        static Element _conv2;
        /// <summary>any effectv effect</summary>
        static Element _filter;

        static Pad _dbinSrcpad;
        static bool _linked = false;
        static int _inIdleProbe = 0 ;

        static List<string> _effects = new List<string>() ;

        public static IEnumerable<string> Filters
        {
            get
            {
                int start = 0;
                while (true)
                {
                    for (int i = start; i < _effects.Count; i++)
                        yield return _effects[i];
                    start = 0;
                }
            }
        }

        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Gst.Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            string fileName = @"U:/Video/test.avi";

            // Get a list of all visualization plugins 
            Gst.Registry registry = Registry.Get();
            var list = registry.FeatureFilter(FilterEffectFeatures, false);
            _effects.AddRange(list.Select(pf => pf.Name));
            foreach (var ef in _effects)
                Console.WriteLine(ef);

            _pipeline = new Pipeline();
            _src = ElementFactory.Make("filesrc");
            _dbin = ElementFactory.Make("decodebin");
            _conv = ElementFactory.Make("videoconvert");
            _scale = ElementFactory.Make("videoscale");
            _navseek = ElementFactory.Make("navseek");
            _queue = ElementFactory.Make("queue");
            _sink = ElementFactory.Make("autovideosink");

            if (new[] { _pipeline, _src, _dbin, _conv, _scale, _navseek,  _queue, _sink }.Any(e => e == null))
            {
                "Failed to create elements".PrintErr();
                return;
            }
            _src["location"] = fileName;

            _pipeline.Add(_src, _dbin, _conv, _scale, _navseek, _queue, _sink);
            if (!Element.Link(_src, _dbin) || !Element.Link(_conv, _scale, _navseek, _queue, _sink))
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
            _dbinSrcpad?.Dispose();
            _pipeline.Dispose();
        }

        static void PadAddedCb(object o, PadAddedArgs args)
        {
            if (_linked)
                return;
            var src = (Element)o;
            var pad = args.NewPad;

            var caps = pad.CurrentCaps;
            var s = caps.GetStructure(0);
            var name = s.Name;

            if (name.StartsWith("video/x-raw"))
            {
                var sinkpad = _conv.GetStaticPad("sink");
                Console.WriteLine(String.Format("Received new pad '{0}' from '{1}':", pad.Name, src.Name));

                if (pad.Link(sinkpad) != PadLinkReturn.Ok)
                {
                    "Failed to link dbin with conb".PrintErr();
                    _loop.Quit();
                }
                sinkpad.Dispose();
                _dbinSrcpad = (Pad)pad.Ref();
                GLib.Timeout.AddSeconds(5, TimeoutCb);
                _linked = true;
            }
            caps.Dispose();
        }

        static bool TimeoutCb()
        {
            _inIdleProbe = 0;
            _dbinSrcpad.AddProbe(PadProbeType.Idle, PadProbeCb);
            return true;
        }

        static IEnumerator<string> _filtersEnumerator = Filters.GetEnumerator();

        static PadProbeReturn PadProbeCb(Pad pad, PadProbeInfo info)
        {
            if (Interlocked.CompareExchange(ref _inIdleProbe, 0, 1) == 1)
                return PadProbeReturn.Ok;

            // Insert or remove filter ?
            if (_conv2 == null)
            {
                _conv2 = ElementFactory.Make("videoconvert");
// circle all effectv filters
                _filtersEnumerator.MoveNext();
                var eff = _filtersEnumerator.Current;
                Console.WriteLine(eff);
                _filter = ElementFactory.Make(eff);

                _pipeline.Add(_conv2, _filter);
                _conv2.SyncStateWithParent();
                _filter.SyncStateWithParent();

                if (!Element.Link(_conv2, _filter))
                {
                    "Failed to link conv2 with filter".PrintErr();
                    _loop.Quit();
                }

                using (var sinkPad = _conv.GetStaticPad("sink"))
                {
                    _dbinSrcpad.Unlink(sinkPad);
                }

                using (var sinkPad = _conv2.GetStaticPad("sink"))
                {
                    if (_dbinSrcpad.Link(sinkPad) != PadLinkReturn.Ok)
                    {
                        "failed to link src with conv2".PrintErr();
                        _loop.Quit();
                    }
                }

                using (var srcPad = _filter.GetStaticPad("src"))
                {
                    using (var sinkPad = _conv.GetStaticPad("sink"))
                    {
                        if (srcPad.Link(sinkPad) != PadLinkReturn.Ok)
                        {
                            "Failed to link filter with conv".PrintErr();
                            _loop.Quit();
                        }
                    }
                }
            }
            else
            {
                using (var sinkPad1 = _conv2.GetStaticPad("sink"))
                {
                    _dbinSrcpad.Unlink(sinkPad1);
                }

                using (var sinkPad2 = _conv.GetStaticPad("sink"))
                {
                    var srcPad = _filter.GetStaticPad("src");

                    srcPad.Unlink(sinkPad2);
                    srcPad.Dispose();

                    _pipeline.Remove(_filter);
                    _pipeline.Remove(_conv2);
                    _filter.SetState(State.Null);
                    _conv2.SetState(State.Null);

                    _filter.Dispose();
                    _conv2.Dispose();
                    _filter = null;
                    _conv2 = null;

                    if (_dbinSrcpad.Link(sinkPad2) != PadLinkReturn.Ok)
                    {
                        "Failed to link src with conv".PrintErr();
                        _loop.Quit();
                    }
                }
            }
            return PadProbeReturn.Remove;
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

        /// <summary>
        /// Return TRUE if this is a effectv element
        /// </summary>
        static bool FilterEffectFeatures(PluginFeature feature)
        {
            if (!(feature is ElementFactory))
                return false;
            var factory = (ElementFactory)feature;
            var klass = factory.GetMetadata(Gst.Constants.ELEMENT_METADATA_KLASS);
            return (klass.Contains("Filter/Effect/Video") && factory.PluginName == "effectv");
        }

    }
}
