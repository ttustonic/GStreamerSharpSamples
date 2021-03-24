//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using GLib;
using Gst;
using Gst.Video;

namespace WpfSamples
{
    /// <summary>
    /// Basic tutorial 5 using pure WPF, without WindowsFormsHost
    /// </summary>
    public partial class BasicTutorial05NoWinform : Window
    {
        Element _playbin;
        long _duration = -1;
        MainLoop _mainLoop;
        System.Threading.Thread _mainGlibThread;
        IntPtr _windowHandle;
        VideoOverlayAdapter _adapter;
        (int x, int y, int w, int h) _videoRect;

        public BasicTutorial05NoWinform()
        {
            InitializeComponent();
            //Init Gstreamer
            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();
            InitGStreamerPipeline();
            videoPanel.SizeChanged += VideoPanel_SizeChanged;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            var setStateRet = _playbin.SetState(State.Null);
            _playbin.Dispose();
            _mainLoop.Quit();
            base.OnClosed(e);
        }

        void VideoPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size size = new Size(this.Width, this.Height);
            var p = videoPanel.TransformToAncestor(this).Transform(new Point(0, 0));
            var s = videoPanel.RenderSize;
            _videoRect = ((int)p.X, (int)p.Y, (int)s.Width, (int)s.Height);
            if (_adapter != null)
                _adapter.SetRenderRectangle(_videoRect.x, _videoRect.y, _videoRect.w, _videoRect.h);
        }

        protected override void OnActivated(System.EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            base.OnActivated(e);
        }

        void InitGStreamerPipeline()
        {
            // Create the elements
            _playbin = ElementFactory.Make("playbin", "playbin");

            if (_playbin == null)
            {
                Console.WriteLine("Not all elements could be created");
                return;
            }
            // Set the URI to play.
            _playbin["uri"] = @"https://dash.akamaized.net/akamai/bbb_30fps/bbb_30fps_1920x1080_8000k.mpd";

            // Connect to interesting signals in playbin
            _playbin.Connect("video-tags-changed", TagsCb);
            _playbin.Connect("audio-tags-changed", TagsCb);
            _playbin.Connect("text-tags-changed", TagsCb);

            var bus = _playbin.Bus;
            bus.AddSignalWatch();
            bus.EnableSyncMessageEmission();
            bus.SyncMessage += OnBusSyncMessage;

            bus.Connect("message::error", ErrorCb);
            bus.Connect("message::eos", EosCb);
            bus.Connect("message::state-changed", StateChangedCb);
            bus.Connect("message::application", ApplicationCb);

            GLib.Timeout.Add(1, RefreshUI);
        }

        bool _isRender = false;
        void OnBusSyncMessage(object o, SyncMessageArgs sargs)
        {
            Gst.Message msg = sargs.Message;

            if (!Gst.Video.Global.IsVideoOverlayPrepareWindowHandleMessage(msg))
                return;

            Element src = msg.Src as Element;
            if (src == null)
                return;

            try
            {
                src["force-aspect-ratio"] = true;
            }
            catch (PropertyNotFoundException)
            { /* Don't care */ }

            Element overlay = (src as Gst.Bin)?.GetByInterface(VideoOverlayAdapter.GType);
            if (overlay == null)
            {
                Console.WriteLine("Overlay is null");
                return;
            }

            _adapter = new VideoOverlayAdapter(overlay.Handle);
            _adapter.WindowHandle = _windowHandle;
            _adapter.SetRenderRectangle(_videoRect.x, _videoRect.y, _videoRect.w, _videoRect.h);
            _adapter.HandleEvents(true);
            _isRender = true;
        }

        #region Play events

        void OnPlayClick(object sender, RoutedEventArgs e)
        {
            var setStateRet = _playbin.SetState(Gst.State.Playing);
            Console.WriteLine("SetStatePlaying returned: " + setStateRet.ToString());
        }

        void OnPauseClick(object sender, RoutedEventArgs e)
        {
            _playbin.SetState(State.Paused);
        }

        void OnStopClick(object sender, RoutedEventArgs e)
        {
            var setStateRet = _playbin.SetState(State.Ready);
            Console.WriteLine("SetStateReady returned: " + setStateRet.ToString());
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        #endregion

        #region Bus events
        /// <summary>
        /// This function is called when an error message is posted on the bus
        /// </summary>
        void ErrorCb(object o, GLib.SignalArgs args)
        {
            Message msg = (Message)args.Args[0];
            msg.ParseError(out GException err, out string debug);

            Console.WriteLine($"Error received from element {msg.Src.Name}: {err.Message}");
            Console.WriteLine("Debugging information: {0}", debug ?? "(none)");

            _playbin.SetState(State.Ready);
        }

        void EosCb(object o, SignalArgs args)
        {
            Console.WriteLine("End-Of-Stream reached.");
            _playbin.SetState(State.Ready);
        }

        void StateChangedCb(object o, SignalArgs args)
        {
            var msg = (Gst.Message)args.Args[0];
            msg.ParseStateChanged(out State oldState, out State newState, out State pendingState);
            if (msg.Src == _playbin)
            {
                Console.WriteLine($"State set to {Element.StateGetName(newState)}");
                if (oldState == State.Ready && newState == State.Paused)
                {
                    // For extra responsiveness, we refresh the GUI as soon as we reach the PAUSED state
                    RefreshUI();
                }
            }
        }

        void ApplicationCb(object o, SignalArgs args)
        {
            var msg = (Gst.Message)args.Args[0];
            if (msg.Structure.Name == "tags-changed")
            {
                // If the message is the "tags-changed" (only one we are currently issuing), update the stream info GUI
                AnalyzeStreams();
            }
        }

        #endregion

        void TagsCb(object sender, GLib.SignalArgs args)
        {
            var playbin = sender as Element;
            // We are possibly in the Gstreamer working thread, so we notify the main thread of this event through a message in the bus
            var s = new Structure("tags-changed");
            playbin.PostMessage(Gst.Message.NewApplication(playbin, s));
        }

        void AnalyzeStreams()
        {
            string str;
            // read some properties
            int nVideo = (int)_playbin["n-video"];
            int nAudio = (int)_playbin.GetProperty("n-audio");
            int nText = (int)_playbin["n-text"];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nVideo; i++)
            {
                // Retrieve the streams video tags
                var tags = (TagList)_playbin.Emit("get-video-tags", i);
                if (tags == null)
                    continue;
                sb.AppendLine($"video stream {i}");
                tags.GetString(Gst.Constants.TAG_VIDEO_CODEC, out str);
                sb.AppendFormat("  codec {0}\n", str ?? "unknown");
                ((Opaque)tags).Dispose();
            }
            for (int i = 0; i < nAudio; i++)
            {
                // Retrieve the stream's audio tags
                var tags = (TagList)_playbin.Emit("get-audio-tags", i);
                if (tags == null)
                    continue;
                sb.AppendLine($"audio stream {i}");
                if (tags.GetString(Gst.Constants.TAG_AUDIO_CODEC, out str))
                    sb.AppendLine($"  codec: {str}");
                if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                    sb.AppendLine($"  language: {str}");
                if (tags.GetUint(Gst.Constants.TAG_BITRATE, out uint rate))
                    sb.AppendLine($"  bitrate: {rate}");
                ((Opaque)tags).Dispose();
            }
            for (int i = 0; i < nText; i++)
            {
                var tags = (TagList)_playbin.Emit("get-text-tags", i);
                if (tags == null)
                    continue;
                sb.AppendLine($"subtitle stream {i}");
                if (tags.GetString(Gst.Constants.TAG_LANGUAGE_CODE, out str))
                    sb.AppendLine($"  language: {str}");
                ((Opaque)tags).Dispose();
            }
            streamsList.Dispatcher.Invoke(() => streamsList.Text = sb.ToString());
        }

        bool RefreshUI()
        {
            _playbin.GetState(out State state, out State pending, 100);

            // We do not want to update anything unless we are in the PAUSED or PLAYING states
            if (state != State.Playing && state != State.Paused)
                return true;
            if (_duration < 0)
            {
                if (!_playbin.QueryDuration(Format.Time, out _duration))
                    Console.WriteLine("Could not query the current duration.");
                else
                {
                    Action updateSlider = () =>
                    {
                        // Set the range of the slider to the clip duration, in SECONDS
                        slider.Minimum = 0;
                        slider.Maximum = (int)(_duration / Gst.Constants.SECOND);
                    };

                    if (slider.Dispatcher.CheckAccess())
                        updateSlider();
                    else
                        slider.Dispatcher.Invoke(updateSlider);
                }
            }
            return true;
        }
    }
}
