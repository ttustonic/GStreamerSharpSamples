//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Runtime.InteropServices;
using GLib;
using Gst;
using Gst.Video;
using Gtk;

namespace GstSamples
{
    /// <summary>
    /// GUI toolkit integration
    /// </summary>
    /// <remarks>
    /// Needs Nuget package Gtksharp.win32
    /// </remarks>
    static class BasicTutorial05
    {
        static Element Playbin;
        static Range Slider;
        static TextView StreamsList;
        static long Duration = -1;

        public static void Run(string[] args)
        {
            // Initialize GTK
            Gtk.Application.Init();
            // Initialize Gstreamer
            Gst.Application.Init(ref args);

            // Create the elements
            Playbin = ElementFactory.Make("playbin", "playbin");

            if (Playbin == null)
            {
                "Not all elements could be created".PrintErr();
                return;
            }
            // Set the URI to play.
            Playbin["uri"] = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
//            Playbin["uri"] = @"file:///U:/Video/test.avi";
//            Playbin["uri"] = @"file:///U:/Video/test2.mp4";

            // Connect to interesting signals in playbin
            Playbin.Connect("video-tags-changed", TagsCb);
            Playbin.Connect("audio-tags-changed", TagsCb);
            Playbin.Connect("text-tags-changed", TagsCb);

            // Create the GUI
            CreateUI();

            // Instruct the bus to emit signals for each received message, and connect to the interesting signals
            var bus = Playbin.Bus;
            bus.AddSignalWatch();
            bus.Connect("message::error", ErrorCb);
            bus.Connect("message::eos", EosCb);
            bus.Connect("message::state-changed", StateChangedCb);
            bus.Connect("message::application", ApplicationCb);

            // Start playing
            var ret = Playbin.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                "Unable to set the pipeline to the playing state.".PrintErr();
                return;
            }

            // Register a function that GLib will call every second
            GLib.Timeout.Add(1, RefreshUI);

            // Start the GTK main loop- We will not regain control until gtk_main_quit is called
            Gtk.Application.Run();

            // Free resources
            Playbin.SetState(State.Null);
        }

        /// <summary>
        /// This creates all the GTK+ widgets that compose our application, and registers the callback
        /// </summary>
        static void CreateUI()
        {
            var mainWindow = new Window(WindowType.Toplevel);
            mainWindow.DeleteEvent += HandleDelete;
            var videoWindow = new DrawingArea();
            videoWindow.DoubleBuffered = false;
            videoWindow.Realized += HandleRealized;
            videoWindow.Drawn += HandleDrawn;

            var playButton = new Button(Stock.MediaPlay);//, IconSize.SmallToolbar);
            playButton.Clicked += HandlePlay;

            var pauseButton = new Button(Stock.MediaPause);//, IconSize.SmallToolbar);
            pauseButton.Clicked += HandlePause;

            var stopButton = new Button(Stock.MediaStop);//, IconSize.SmallToolbar);
            stopButton.Clicked += HandleStop;

            Slider = new HScale(0, 100, 1);
            ((Scale)Slider).DrawValue = false;
            Slider.ValueChanged += HandleValueChanged;

            StreamsList = new TextView();
            StreamsList.Editable = false;

            var controls = new HBox(false, 0);
            controls.PackStart(playButton, false, false, 2);
            controls.PackStart(pauseButton, false, false, 2);
            controls.PackStart(stopButton, false, false, 2);
            controls.PackStart(Slider, true, true, 2);

            var mainHBox = new HBox(false, 0);
            mainHBox.PackStart(videoWindow, true, true, 0);
            mainHBox.PackStart(StreamsList, false, false, 2);

            var mainBox = new VBox(false, 0);
            mainBox.PackStart(mainHBox, true, true, 0);
            mainBox.PackStart(controls, false, false, 0);
            mainWindow.Add(mainBox);
            mainWindow.SetDefaultSize(640, 480);

            mainWindow.ShowAll();
        }

        /// <summary>
        /// This function is called periodically to refresh the GUI
        /// </summary>
        static bool RefreshUI()
        {
            Playbin.GetState(out State state, out State pending, 100);

            // We do not want to update anything unless we are in the PAUSED or PLAYING states
            if (state != State.Playing && state != State.Paused)
                return true;

            // If we didn't know it yet, query the stream duration
            if (Duration < 0)
            {
                if (!Playbin.QueryDuration(Format.Time, out Duration))
                    "Could not query the current duration.".PrintErr();
                else
                {
                    // Set the range of the slider to the clip duration, in SECONDS
                    Slider.SetRange(0, Duration / (double)Gst.Constants.SECOND);
                }
            }
            if (Playbin.QueryPosition(Format.Time, out long current))
            {
                /* Block the "value-changed" signal, so the slider_cb function is not called
                 * (which would trigger a seek the user has not requested) */
                Slider.ValueChanged -= HandleValueChanged;
                Slider.Value = (double)current/Gst.Constants.SECOND ;
                // Re-enable the signal
                Slider.ValueChanged += HandleValueChanged;
            }
            return true;
        }

        #region Bus events
        /// <summary>
        /// This function is called when an error message is posted on the bus 
        /// </summary>
        static void ErrorCb(object o, GLib.SignalArgs args)
        {
            Bus bus = o as Bus;
            Message msg = (Message)args.Args[0];
            msg.ParseError(out GException err, out string debug);

            $"Error received from element {msg.Src.Name}: {err.Message}".PrintErr();
            String.Format("Debugging information: {0}", debug ?? "(none)").PrintErr();

            Playbin.SetState(State.Ready);
        }

        static void ApplicationCb(object o, SignalArgs args)
        {
            var msg = (Message)args.Args[0];
            if (msg.Structure.Name == "tags-changed")
            {
                // If the message is the "tags-changed" (only one we are currently issuing), update the stream info GUI
                AnalyzeStreams();
            }
        }

        static void StateChangedCb(object o, SignalArgs args)
        {
            var msg = (Message)args.Args[0];
            msg.ParseStateChanged(out State oldState, out State newState, out State pendingState);
            if (msg.Src == Playbin)
            {
//                State = newState;
                Console.WriteLine("State set to {0}", Element.StateGetName(newState));
                if (oldState == State.Ready && newState == State.Paused)
                {
                    // For extra responsiveness, we refresh the GUI as soon as we reach the PAUSED state
                    RefreshUI();
                }
            }
        }

        static void EosCb(object o, SignalArgs args)
        {
            Console.WriteLine("End-Of-Stream reached.");
            Playbin.SetState(State.Ready);
        }

        static void AnalyzeStreams()
        {
            TagList tags ;
            string totalStr, str;

            // Clean current contents of the widget
            var text = StreamsList.Buffer;
            text.Text = String.Empty;

            StreamsList.Buffer.Text = String.Empty;
            // read some properties
            int nVideo = (int)Playbin["n-video"];
            int nAudio = (int)Playbin.GetProperty("n-audio");
            int nText = (int)Playbin["n-text"];

            for (int i = 0; i < nVideo; i++)
            {
                // Retrieve the streams video tags
                tags = (TagList)Playbin.Emit("get-video-tags", i);
                if (tags == null)
                    continue;
                totalStr = $"video stream {i}\n";
                StreamsList.Buffer.InsertAtCursor(totalStr);
                tags.GetString(Gst.Constants.TAG_VIDEO_CODEC, out str);
                totalStr = String.Format("  codec {0}\n", str ?? "unknown");
                StreamsList.Buffer.InsertAtCursor(totalStr);
            }

            for (int i = 0; i < nAudio; i++)
            {
                // Retrieve the stream's audio tags
                tags = (TagList)Playbin.Emit("get-audio-tags", i);
                if (tags == null)
                    continue;
                totalStr = $"audio stream {i}\n";
                StreamsList.Buffer.InsertAtCursor(totalStr);
                if (tags.GetString(Gst.Constants.TAG_AUDIO_CODEC, out str))
                {
                    totalStr = $"  codec: {str}\n";
                    StreamsList.Buffer.InsertAtCursor(totalStr);
                }
                if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                {
                    totalStr = $"  language: {str}\n";
                    StreamsList.Buffer.InsertAtCursor(totalStr);
                }
                if (tags.GetUint(Gst.Constants.TAG_BITRATE, out uint rate))
                {
                    totalStr = $"  bitrate: {rate}\n";
                    StreamsList.Buffer.InsertAtCursor(totalStr);
                }
            }

            for (int i = 0; i < nText; i++)
            {
                tags = (TagList)Playbin.Emit("get-text-tags", i);
                if (tags == null)
                    continue;
                totalStr = $"subtitle stream {i}\n";
                StreamsList.Buffer.InsertAtCursor(totalStr);
                if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                {
                    totalStr = $"  language: {str}\n";
                    StreamsList.Buffer.InsertAtCursor(totalStr);
                }
            }
        }
        #endregion

        #region Gui events
        /// <summary>
        /// This function is called when the main window is closed
        /// </summary>
        static void HandleDelete(object o, DeleteEventArgs args)
        {
            HandleStop(o, args);
            Gtk.Application.Quit();
        }

        /// <summary>
        /// This function is called when the GUI toolkit creates the physical window that will hold the video.
        /// At this point we can retrieve its handler (which has a different meaning depending on the windowing system)
        /// and pass it to GStreamer through the VideoOverlay interface.
        /// </summary>
        static void HandleRealized(object sender, EventArgs e)
        {
            var widget = sender as Widget;
            var window = widget.Window;
            IntPtr windowHandle = IntPtr.Zero;

            // Retrieve window handler from GDK
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    windowHandle = gdk_x11_window_get_xid(window.Handle);
                    break;
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    windowHandle = gdk_win32_window_get_handle(window.Handle);
                    break;
            }

            Element overlay = null;
            if (Playbin is Gst.Bin)
                overlay = ((Gst.Bin)Playbin).GetByInterface(VideoOverlayAdapter.GType);

            VideoOverlayAdapter adapter = new VideoOverlayAdapter(overlay.Handle);
            adapter.WindowHandle = windowHandle;
            adapter.HandleEvents(true);
        }

        /// <summary>
        /// This function is called everytime the video window needs to be redrawn (due to damage/exposure,
        /// rescaling, etc). GStreamer takes care of this in the PAUSED and PLAYING states, otherwise,
        /// we simply draw a black rectangle to avoid garbage showing up.
        /// </summary>
        static void HandleDrawn(object o, DrawnArgs args)
        {
            var widget = o as Widget;
            Playbin.GetState(out State state, out State pending, 100);

            if (state != State.Paused && state != State.Playing)
            {
                var window = widget.Window;
                var allocation = widget.Allocation;

                var cr = Gdk.CairoHelper.Create(window);
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, allocation.Width, allocation.Height);
                cr.Fill();
                cr.Dispose();
            }
            args.RetVal = false;
        }


        #endregion

        static void TagsCb(object sender, GLib.SignalArgs args)
        {
            var playbin = sender as Element;
            // We are possibly in the Gstreamer working thread, so we notify the main thread of this event through a message in the bus
            var s = new Structure("tags-changed");
            playbin.PostMessage(Message.NewApplication(playbin, s));
        }

        /// <summary>
        /// This function is called when the PLAY button is clicked
        /// </summary>
        static void HandlePlay(object sender, System.EventArgs e)
        {
            StateChangeReturn setStateRet;
            setStateRet = Playbin.SetState(State.Playing);
            Console.WriteLine("SetStatePlaying returned: " + setStateRet.ToString());
        }

        /// <summary>
        /// This function is called when the PAUSE button is clicked
        /// </summary>
        static void HandlePause(object sender, EventArgs e)
        {
            Playbin.SetState(State.Paused);
        }

        /// <summary>
        // This method is called when the STOP button is clicked
        /// </summary>
        static void HandleStop(object sender, EventArgs e)
        {
            StateChangeReturn setStateRet;
            setStateRet = Playbin.SetState(State.Ready);
            Console.WriteLine("SetStateReady returned: " + setStateRet.ToString());
        }

        static void HandleValueChanged(object sender, EventArgs e)
        {
            var range = sender as Range;
            var value = range.Value;
            Playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, (long)(value * Gst.Constants.SECOND));
        }

        #region Imports
        [DllImport("libgdk-3.so.0")]
        static extern IntPtr gdk_x11_window_get_xid(IntPtr handle);

        [DllImport("libgdk-3-0.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gdk_win32_window_get_handle(IntPtr handle);

        [DllImport("libX11.so.6")]
        static extern int XInitThreads();

        #endregion

    }
}
