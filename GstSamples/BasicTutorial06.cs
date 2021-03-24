//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Media formats and Pad Capabilities
    /// </summary>
    static class BasicTutorial06
    {
        public static void Run(string[] args)
        {
            // Initialize Gstreamer
            Application.Init(ref args);
            GLib.GType.Register(Gst.Bitmask.GType, typeof(Gst.Bitmask));
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            // Create the element factories
            ElementFactory sourceFactory = ElementFactory.Find("audiotestsrc");
            ElementFactory sinkFactory = ElementFactory.Find("autoaudiosink");

            if (sourceFactory == null || sinkFactory == null)
            {
                "Not all element factories could be created".PrintErr();
                return;
            }

            // Print information about the pad templates of these factories
            Console.WriteLine("Source factory info");
            PrintPadTemplateInformation(sourceFactory);
            Console.WriteLine("Sink factory info");
            PrintPadTemplateInformation(sinkFactory);

            // Ask the factories to instantiate actual elements
            var source = sourceFactory.Create("source");
            var sink = sinkFactory.Create("sink");

            // Create the empty pipeline
            var pipeline = new Pipeline("test-pipeline");

            if (pipeline == null || source == null || sink == null)
            {
                "Not all elements could be created".PrintErr();
                return;
            }
            pipeline.Add(source, sink);

            // Build the pipeline
            if (!source.Link(sink))
            {
                "Elements could not be linked".PrintErr();
                return;
            }

            // Print initial negotiated caps (in NULL state)
            Console.WriteLine("In NULL state:");
            PrintPadCapabilities(sink, "sink");

            // Start playing
            var ret = pipeline.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                "Unable to set the pipeline to the playing state (check the bus for error messages).".PrintErr();
            }

            // Wait until error, EOS or State Change
            var bus = pipeline.Bus;
            bool terminate = false;
            do
            {
                var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos | MessageType.StateChanged);
                if (msg == null)
                    continue;
                // Parse message
                switch (msg.Type)
                {
                    case MessageType.Eos:
                        Console.WriteLine("End of stream reached");
                        terminate = true;
                        break;
                    case MessageType.Error:
                        msg.ParseError(out GLib.GException error, out string debug);
                        $"Error received from element {msg.Src.Name}: {error.Message}".PrintErr();
                        String.Format("Debugging information {0}", debug ?? "(none)").PrintErr();
                        terminate = true;
                        break;
                    case MessageType.StateChanged:
                        // We are only interested in state-changed messages from the pipeline
                        if (msg.Src != pipeline)
                            continue;
                        msg.ParseStateChanged(out State oldState, out State newState, out State pending);
                        Console.WriteLine($"\nPipeline state changed from {Element.StateGetName(oldState)} to {Element.StateGetName(newState)}");
                        // Print the current capabilities of the sink element
                        PrintPadCapabilities(sink, "sink");
                        break;
                    default:
                        // We should not reach here because we only asked for ERRORs, EOS and STATE_CHANGED
                        "Unexpected message received".PrintErr();
                        break;
                }
            } while (!terminate);

            // Free resources
            pipeline.SetState(State.Null);
        }

        #region Functions below print the Capabilities in a human-friendly format

        [DllImport("gstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern ulong gst_value_get_bitmask(IntPtr value);

        static bool PrintField(uint field_id, GLib.Value value)
        {
            string quark=String.Empty;
            IntPtr ptr = g_quark_to_string(field_id);

            try
            {
                quark = GLib.Marshaller.Utf8PtrToString(ptr);
                Console.WriteLine($"{quark}: {value.Val}");
            }
            catch (Exception ex)
            {
                IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc(value);
                ulong raw_ret = gst_value_get_bitmask(native_value);
                ulong ret = raw_ret;
                Marshal.FreeHGlobal(native_value);


                var bitMaskValue = Gst.Value.GetBitmask(value);

                //https://github.com/mono/gtk-sharp/blob/master/glib/Value.cs#L318
                //https://github.com/GtkSharp/GtkSharp/blob/develop/Source/Libs/GLibSharp/Value.cs#L315
                var typeField = typeof(GLib.Value)
                    .GetField("type", BindingFlags.NonPublic | BindingFlags.Instance);
                var typeFieldValue = (IntPtr)typeField.GetValue(value);

                var lookupType = GLib.GType.LookupType(typeFieldValue);
                var ciInvalid = lookupType.GetConstructor(new[] { typeof(GLib.Value) });
                var ciOk = lookupType.GetConstructor(new[] { typeof(IntPtr) });
                var valuePtr = GLib.Marshaller.StructureToPtrAlloc(value);
                Bitmask bitMask = (Bitmask)ciOk.Invoke(new object[] { valuePtr });

                $"{quark} : {ex.Message} : {bitMaskValue} ".PrintErr();
            }
            return true;
        }

        static void PrintCaps(Caps caps, string pfx)
        {
            if (caps == null)
                return;
            if (caps.IsAny)
            {
                Console.WriteLine($"{pfx}ANY");
                return;
            }
            if (caps.IsEmpty)
            {
                Console.WriteLine($"{pfx}EMPTY");
                return;
            }
            for (uint i = 0; i < caps.Size; i++)
            {

                var structure = caps[i];
                Console.WriteLine($"{pfx}{structure.Name}");
                structure.Foreach(PrintField);
            }
        }

        /// <summary>
        /// Prints information about a Pad Template, including its Capabilities
        /// </summary>
        static void PrintPadTemplateInformation(ElementFactory factory)
        {
            Console.WriteLine($"Pad templates for {factory.Name}");
            if (factory.NumPadTemplates == 0)
            {
                Console.WriteLine(" none");
                return;
            }

            var pads = factory.StaticPadTemplates;
            foreach (var pad in pads)
            {
                Console.WriteLine($"    {pad.Direction} template: '{pad.NameTemplate}'");
                Console.WriteLine($"    Availability {pad.Presence}");
                if (pad.StaticCaps.String != null)
                {
                    Console.WriteLine("      Capabilities:");
                    var caps = pad.StaticCaps.Caps;
                    PrintCaps(caps, "      ");
                }
            }
        }

        /// <summary>
        /// Shows the CURRENT capabilities of the requested pad in the given element
        /// </summary>
        static void PrintPadCapabilities(Element element, string padName)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"Pad caps {padName} {element.GetType().Name} ");
            var pad = element.GetStaticPad(padName);
            if (pad == null)
            {
                $"Cound not retrieve pad {padName}".PrintErr();
                return;
            }
            /* Retrieve negotiated caps (or acceptable caps if negotiation is not finished yet) */
            var caps = pad.CurrentCaps ?? pad.QueryCaps();

            /* Print and free */
            Console.WriteLine($"Pads for the {padName} pad");
            PrintCaps(caps, "      ");
        }
        #endregion

        [DllImport("glib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr g_quark_to_string(uint quark);
    }
}
