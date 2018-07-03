//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using GLib;
using Gst;
using Gst.Video;
using SysDiag = System.Diagnostics;

namespace WpfSamples
{
    /// <summary>
    /// Interaction logic for BasicTutorial05.xaml
    /// </summary>
    public partial class BasicTutorial05 : Window
    {
        Element _playbin;
        long _duration = -1;
        MainLoop _mainLoop;
        System.Threading.Thread _mainGlibThread;
        IntPtr _videoPanelHandle;

        public BasicTutorial05()
        {
            InitializeComponent();
            //Init Gstreamer
            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            videoPanel.HandleCreated += HandleRealized;
            InitGStreamerPipeline();
        }

        protected override void OnClosed(EventArgs e)
        {
            var setStateRet = _playbin.SetState(State.Null);
            _playbin.Dispose();
            _mainLoop.Quit();
            base.OnClosed(e);
        }

        void HandleRealized(object sender, EventArgs e)
        {
            var vpanel = sender as System.Windows.Forms.Panel;
            _videoPanelHandle = vpanel.Handle;
            Element overlay = ((Gst.Bin)_playbin).GetByInterface(VideoOverlayAdapter.GType);
            VideoOverlayAdapter adapter = new VideoOverlayAdapter(overlay.Handle);
            adapter.WindowHandle = _videoPanelHandle;
            adapter.HandleEvents(true);
        }

        void InitGStreamerPipeline()
        {
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();

            // Create the elements
            _playbin = ElementFactory.Make("playbin", "playbin");

            if (_playbin == null)
            {
                Console.WriteLine("Not all elements could be created");
                return;
            }
            // Set the URI to play.
            _playbin["uri"] = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4";
//            _playbin["uri"] = @"file:///U:/Video/test2.mp4";
//            _playbin["uri"] = @"file:///U:/Video/sintel_trailer-480p.webm";

            // Connect to interesting signals in playbin
            _playbin.Connect("video-tags-changed", TagsCb);
            _playbin.Connect("audio-tags-changed", TagsCb);
            _playbin.Connect("text-tags-changed", TagsCb);

            var bus = _playbin.Bus;
            bus.AddSignalWatch();
            bus.Connect("message::error", ErrorCb);
            bus.Connect("message::eos", EosCb);
            bus.Connect("message::state-changed", StateChangedCb);
            bus.Connect("message::application", ApplicationCb);

            GLib.Timeout.Add(1, RefreshUI);
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
            var slider = sender as Slider;
            var value = slider.Value;
            _playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, (long)value * Gst.Constants.SECOND);
        }

        #endregion

        #region Bus events
        /// <summary>
        /// This function is called when an error message is posted on the bus 
        /// </summary>
        void ErrorCb(object o, GLib.SignalArgs args)
        {
            Bus bus = o as Bus;
            Gst.Message msg = (Gst.Message)args.Args[0];
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
                //                State = newState;
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
