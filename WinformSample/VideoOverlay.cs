//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.IO;
using System.Windows.Forms;
using Gst;
using Gst.Video;

namespace WinformSample
{
    public partial class VideoOverlay : Form
    {
        bool _pipelineOK = false;
        Element _playbin;
        IntPtr _videoPanelHandle;
        System.Threading.Thread _mainGlibThread;
        GLib.MainLoop _mainLoop;
        bool _updatingScale;
        // prevent updating Trackbar while dragging
        bool _isMouseDrag = false;

        public VideoOverlay()
        {
            InitializeComponent();

            //Init Gstreamer
            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();

            _videoPanelHandle = videoPanel.Handle;
            GLib.Timeout.Add(1000, new GLib.TimeoutHandler(UpdatePos));
        }

        #region Play events

        void OnPlayClick(object sender, EventArgs e)
        {
            if ((_playbin != null) && _pipelineOK)
                _playbin.SetState(Gst.State.Playing);
        }

        void OnPauseClick(object sender, EventArgs e)
        {
            if ((_playbin != null) && _pipelineOK)
                _playbin.SetState(Gst.State.Paused);
        }

        void OnStopClick(object sender, EventArgs e)
        {
            var setStateRet = _playbin.SetState(State.Ready);
            Console.WriteLine("SetStateReady returned: " + setStateRet.ToString());
        }

        void OnOpenClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) ;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _pipelineOK = false;
                if (_playbin != null)
                    _playbin.SetState(State.Null);
                else
                    _playbin = ElementFactory.Make("playbin", "playbin");
                if (_playbin == null)
                {
                    throw new Exception("Unable to create element playbin");
                }
                scale.Value = 0;

                _playbin.Bus.EnableSyncMessageEmission();
                _playbin.Bus.AddSignalWatch();
                _playbin.Bus.SyncMessage += OnBusSyncMessage;
                _playbin.Bus.Message += OnBusMessage;

                var filePath = openFileDialog.FileName;
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Text = this.Name + " : " + fileName;

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        _playbin["uri"] = "file://" + filePath;
                        break;
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        _playbin["uri"] = "file:///" + filePath.Replace("\\", "/");
                        break;
                }

                StateChangeReturn sret = _playbin.SetState(Gst.State.Playing);

                if (sret == StateChangeReturn.Async)
                {
                    sret = _playbin.GetState(out var state, out var pending, Gst.Constants.SECOND * 5L);
                }
                if (sret == StateChangeReturn.Success)
                {
                    Console.WriteLine("State change successful");
                    _pipelineOK = true;
                }
                else
                {
                    Console.WriteLine($"State change failed for {filePath} ({sret})\n");
                }
            }
        }
        #endregion

        void OnBusMessage(object o, MessageArgs margs)
        {
            Gst.Message message = margs.Message;
            switch (message.Type)
            {
                case MessageType.Error:
                    message.ParseError(out GLib.GException err, out string debug);
                    Console.WriteLine($"Error message: {debug}");
                    _pipelineOK = false;
                    break;
                case MessageType.Eos:
                    Console.WriteLine("EOS");
                    break;
            }
        }

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

            VideoOverlayAdapter adapter = new VideoOverlayAdapter(overlay.Handle);
            adapter.WindowHandle = _videoPanelHandle;
            adapter.HandleEvents(true);
        }

        void OnScaleValueChanged(object sender, EventArgs e)
        {
            if (_isMouseDrag)
                return;
            if (_updatingScale)
                return;
// Moved to Mouse up event, to prevent seeking while dragging.
            //var scale = sender as TrackBar;
            //var value = scale.Value;
            //Console.WriteLine($"Trying to seek to {value}");
            //if ((_playbin != null)
            //    && _pipelineOK
            //    && _playbin.QueryDuration(Format.Time, out long duration)
            //    && duration != -1)
            //{
            //    var pos = duration * value / 100;
            //    Console.WriteLine("Seek to {0}/{1} ({2}%)", pos, duration, scale.Value);
            //    _playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, pos);
            //}
        }

        void OnScaleMouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDrag = true;
        }

        void OnScaleMouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDrag = false;
            if (_updatingScale)
                return;
            var trackBar = sender as TrackBar;
            var value = trackBar.Value;
            if ((_playbin != null)
                && _pipelineOK
                && _playbin.QueryDuration(Format.Time, out long duration)
                && duration != -1)
            {
                var pos = duration * value / 100;
                Console.WriteLine("Seek to {0}/{1} ({2}%)", pos, duration, trackBar.Value);
                _playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, pos);
            }
        }

        void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _mainLoop.Quit();
        }

        bool UpdatePos()
        {
            if ((_playbin != null) && _pipelineOK &&
                _playbin.QueryDuration(Gst.Format.Time, out long duration) &&
                _playbin.QueryPosition(Gst.Format.Time, out long pos))
            {
                var tsPos = new TimeSpan(pos / 100).RoundSeconds(0);
                var tsDur = new TimeSpan(duration / 100).RoundSeconds(0);
                _lbl.Text = String.Format("{0} / {1}", tsPos.ToString("g"), tsDur.ToString("g"));

                if (_isMouseDrag)
                    return true;

                _updatingScale = true;
                var scval = (int)(pos * scale.Maximum / duration);
                scale.Value = scval;
                _updatingScale = false;
            }
            return true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (_playbin != null)
                {
                    _playbin.SetState(State.Null);
                    _playbin.Dispose();
                    _playbin = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
