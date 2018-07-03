//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using GLib;
using Gst;
using Gst.Video;
using SysDiag = System.Diagnostics;

namespace WinformSample
{
    /// <summary>
    /// </summary>
    public partial class BasicTutorial5 : Form
    {
        Element _playbin;

        long _duration = -1;
        MainLoop _mainLoop;
        System.Threading.Thread _mainGlibThread;
        IntPtr _videoPanelHandle;
        uint _refreshUiHandle;

        public BasicTutorial5()
        {
            InitializeComponent();

            //Init Gstreamer
            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            videoPanel.HandleCreated += HandleRealized;
            InitGStreamerPipeline();
        }

        void HandleRealized(object sender, EventArgs e)
        {
            var vpanel = sender as Panel;
            _videoPanelHandle = vpanel.Handle;
            Element overlay = ((Gst.Bin)_playbin).GetByInterface(VideoOverlayAdapter.GType);
            VideoOverlayAdapter adapter = new VideoOverlayAdapter(overlay.Handle);
            adapter.WindowHandle = _videoPanelHandle;
            adapter.HandleEvents(true);
        }

        void OnPlayClick(object sender, EventArgs e)
        {
            var setStateRet = _playbin.SetState(Gst.State.Playing);
            Console.WriteLine("SetStatePlaying returned: " + setStateRet.ToString());
        }

        void OnPauseClick(object sender, EventArgs e)
        {
            _playbin.SetState(State.Paused);
        }

        void OnStopClick(object sender, EventArgs e)
        {
            var setStateRet = _playbin.SetState(State.Ready);
            Console.WriteLine("SetStateReady returned: " + setStateRet.ToString());
        }

        void OnSliderValueChanged(object sender, EventArgs e)
        {
            var slider = sender as TrackBar;
            var value = slider.Value;
            _playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, value * Gst.Constants.SECOND);
        }

        void InitGStreamerPipeline()
        {
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();
            //            _mainLoop.Run();

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

            var ret = _playbin.SetState(State.Playing);
            if (ret == StateChangeReturn.Failure)
            {
                Console.WriteLine("Unable to set the pipeline to the playing state.");
                return;
            }
            _refreshUiHandle = GLib.Timeout.Add(1, RefreshUI);
        }

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
            streamsList.Invoke((Action)(() => streamsList.Text = sb.ToString()));
        }

        #endregion

        void TagsCb(object sender, GLib.SignalArgs args)
        {
            var playbin = sender as Element;
            // We are possibly in the Gstreamer working thread, so we notify the main thread of this event through a message in the bus
            var s = new Structure("tags-changed");
            playbin.PostMessage(Gst.Message.NewApplication(playbin, s));
        }

        bool RefreshUI()
        {
            if (_playbin == null)
                return false;
            _playbin.GetState(out State state, out State pending, 100);

            // We do not want to update anything unless we are in the PAUSED or PLAYING states
            if (state != State.Playing && state != State.Paused)
            {
                return true;
            }

            if (_duration < 0)
            {
                if (!_playbin.QueryDuration(Format.Time, out _duration))
                    SysDiag.Debug.WriteLine("Could not query the current duration.");
                else
                {
                    Action updateSlider = () =>
                    {
                        // Set the range of the slider to the clip duration, in SECONDS
                        slider.Minimum = 0;
                        slider.Maximum = (int)(_duration / Gst.Constants.SECOND);
                    };
                    if (slider.InvokeRequired)
                    {
                        slider.Invoke(updateSlider);
                    }
                }
            }
            if (_playbin.QueryPosition(Format.Time, out long current))
            {
                try
                {
                    /* Block the "value-changed" signal, so the slider_cb function is not called
                        * (which would trigger a seek the user has not requested) */
                    slider.ValueChanged -= OnSliderValueChanged;
                    slider.Invoke((Action)(() => slider.Value = (int)(current / Gst.Constants.SECOND)));
                    slider.ValueChanged += OnSliderValueChanged;
                }
                catch (System.ComponentModel.InvalidAsynchronousStateException)
                {
// An error occurred invoking the method.  The destination thread no longer exists
                }
            }
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GLib.Timeout.Remove(_refreshUiHandle);
            _playbin.SetState(State.Ready);
            _playbin.SetState(State.Null);
            _mainLoop.Quit();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                _playbin.Dispose();
                _playbin = null;
            }
            base.Dispose(disposing);
        }


        //void OnFormClosing(object sender, FormClosingEventArgs e)
        //{
        //    //OnStopClick(sender, EventArgs.Empty);
        //    //_mainLoop.Quit();
        //}
    }
}
