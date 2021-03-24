//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using GLib;
using Gst;
using Gst.Video;
using Microsoft.Win32;

namespace WpfSamples
{
    /// <summary>
    /// Interaction logic for VideoOverlay.xaml
    /// </summary>
    public partial class VideoOverlay : Window
    {
        bool _pipelineOK = false;
        Element _playbin;
        bool _updatingScale;
        // prevent updating Slider while dragging
        bool _isMouseDrag = false;
        MainLoop _mainLoop;
        System.Threading.Thread _mainGlibThread;
        IntPtr _windowHandle;
        VideoOverlayAdapter _adapter;
        (int x, int y, int w, int h) _videoRect;

        public VideoOverlay()
        {
            InitializeComponent();
            //Init Gstreamer
            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();
            videoPanel.SizeChanged += VideoPanel_SizeChanged;
            GLib.Timeout.Add(500, new GLib.TimeoutHandler(UpdatePos));
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
        }

        #region Play events

        void OnOpenClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (openFileDialog.ShowDialog() ?? false)
            {
                InitGStreamerPipeline();

                var filePath = openFileDialog.FileName;
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                Title = Name + " : " + fileName;

                _playbin["uri"] = "file:///" + filePath.Replace("\\", "/");

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
            if (_updatingScale)
                return;

            var slider = sender as Slider;
            var value = slider.Value;
            if ((_playbin != null)
                && _pipelineOK
                && _playbin.QueryDuration(Format.Time, out long duration)
                && duration != -1)
            {
                var pos = (long)(duration * value);
                var tsPos = new TimeSpan(pos / 100).RoundSeconds(0);
                Console.WriteLine($"Seek to {pos}/{duration} ({value}%)");
                _playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, pos);
            }
        }

        #endregion

        bool UpdatePos()
        {
            if ((_playbin != null) && _pipelineOK &&
                _playbin.QueryDuration(Format.Time, out long duration) &&
                _playbin.QueryPosition(Format.Time, out long pos))
            {
                var tsPos = new TimeSpan(pos / 100).RoundSeconds(0);
                var tsDur = new TimeSpan(duration / 100).RoundSeconds(0);
                Dispatcher.Invoke(() =>
                    lbl.Content = String.Format("{0} / {1}", tsPos.ToString("g"), tsDur.ToString("g"))
                    );
                if (_isMouseDrag)
                    return true;

                _updatingScale = true;
                var scval = (double)pos / duration;
                Dispatcher.Invoke(() =>
                {
                    scale.Value = scval;
                });

                _updatingScale = false;
            }
            return true;
        }
        void OnBusMessage(object o, MessageArgs margs)
        {
            Message message = margs.Message;
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

    }
}
