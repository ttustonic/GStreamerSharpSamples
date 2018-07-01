//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Audio visualization
    /// </summary>
    static class PlaybackTutorial06
    {
        public static void Run(string[] args)
        {
            // Initialize GStreamer
            Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            // Get a list of all visualization plugins 
            Gst.Registry registry = Registry.Get();
            var list = registry.FeatureFilter(FilterVisFeatures, false);

            // Print their names
            Console.WriteLine("Available visualization plugins:");

            ElementFactory selectedFactory = null;
            foreach (var walk in list)
            {
                ElementFactory factory = (ElementFactory)walk;
                var name = factory.Name;
                Console.WriteLine($"  {name}");
                if (selectedFactory == null && name.StartsWith("goom"))
                {
                    selectedFactory = factory;
                }
            }

            /* Don't use the factory if it's still empty
             * e.g. no visualization plugins found */
            if (selectedFactory == null)
            {
                Console.WriteLine("No visualisation plugins found");
                return;
            }

            // We have now selected a factory for the visualization element
            Console.WriteLine($"Selected {selectedFactory.Name} ");
            var visPlugin = selectedFactory.Create();
            if (visPlugin == null)
                return;

//  Not found
//            using (var pipeline = (Pipeline)Parse.Launch("playbin uri=http://1live.akacast.akamaistream.net/7/706/119434/v1/gnl.akacast.akamaistream.net/1live"))
            using (var pipeline = (Pipeline)Parse.Launch("playbin uri=http://radio.hbr1.com:19800/ambient.ogg"))
            {
                // Set the visualization flag
                var flags = (GstPlayFlags)pipeline["flags"];
                flags |= GstPlayFlags.Vis;
                pipeline["flags"] = (uint)flags;

                // set vis plugin for playbin
                pipeline["vis-plugin"] = visPlugin;

                var ret = pipeline.SetState(State.Playing);
                if (ret == StateChangeReturn.Failure)
                {
                    "Unable to set the pipeline to the playing state.".PrintErr();
                    return;
                }
                using (var bus = pipeline.Bus)
                {
                    var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);
                    if (msg != null)
                        msg.Dispose();
                }
                pipeline.SetState(State.Null);
            }
        }

        /// <summary>
        /// Return TRUE if this is a Visualization element
        /// </summary>
        static bool FilterVisFeatures(PluginFeature feature)
        {
            if (!(feature is ElementFactory))
                return false;
            var factory = (ElementFactory)feature;
            return (factory.GetMetadata(Gst.Constants.ELEMENT_METADATA_KLASS).Contains("Visualization"));
        }
    }
}
