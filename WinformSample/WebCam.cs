//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gst;
using Gst.Video;
using static System.Diagnostics.Debug;

namespace WinformSample
{
    public partial class WebCam : Form
    {
        DeviceMonitor _devMon;
        Pipeline _pipeline;

        IntPtr _videoPanelHandle;
        System.Threading.Thread _mainGlibThread;
        GLib.MainLoop _mainLoop;

        BindingList<string> _devices = new BindingList<string>();

        static Pipeline CreatePipeline(string name)
        {
            var pipe = (Pipeline)Parse.Launch("ksvideosrc name=camsource ! videoconvert name=convert ! videoscale ! autovideosink name=sink");
            return pipe;
        }

        public WebCam()
        {
            InitializeComponent();
        
            listBox1.DataSource = _devices;

            Gst.Application.Init();
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();
            _videoPanelHandle = videoPanel.Handle;
            InitGStreamerPipeline();
        }

        void HandleRealized(object sender, EventArgs e)
        {
            var vpanel = sender as Panel;
            _videoPanelHandle = vpanel.Handle;
        }

        void InitGStreamerPipeline()
        {
            _mainLoop = new GLib.MainLoop();
            _mainGlibThread = new System.Threading.Thread(_mainLoop.Run);
            _mainGlibThread.Start();

            _pipeline = CreatePipeline("camera-pipeline");
            Assert(_pipeline != null, "Pipeline could not be created");

            _pipeline.Bus.EnableSyncMessageEmission();
            _pipeline.Bus.SyncMessage += OnBusSyncMessage;
            _pipeline.Bus.AddWatch(OnBusMessage);

            _devMon = new DeviceMonitor();
            var caps = new Caps("video/x-raw");
            var filtId = _devMon.AddFilter("Video/Source", caps);

            foreach (var d in _devMon.Devices.Select(d => d.DisplayName))
                _devices.Add(d);

            var cam = _devMon.Devices.FirstOrDefault(d => d.DeviceClass == "Video/Source");
            if (cam != null)
                ShowCamera(cam.DisplayName);

            Assert(_devMon.Start(), "Device monitor cannot start");
            _devMon.Bus.AddWatch(OnBusMessage);
        }

        bool OnBusMessage(object o, Gst.Message message)
        {
            var msgSrc = message.Src as Element;
            switch (message.Type)
            {
                case MessageType.Eos:
                    Console.WriteLine("EOS");
                    break;
                case MessageType.Error:
                    message.ParseError(out GLib.GException err, out string debug);
                    $"Error received from element {message.Src.Name}: {err.Message}".PrintErr();
                    String.Format("Debugging information {0}", debug ?? "(none)").PrintErr();
                    break;
                case MessageType.DeviceAdded:
                    {
                        var dev = message.ParseDeviceAdded();
                        _devices.Add(dev.DisplayName);
                        var cam = _pipeline.GetByName("camsource");
                        Console.WriteLine(cam["device-name"]);
                        if ((string)cam["device-name"] == null)
                            ShowCamera(dev.DisplayName);
                        break;
                    }
                case MessageType.DeviceRemoved:
                    {
                        string selectedDevice = (string)listBox1.SelectedItem;
                        var dev = message.ParseDeviceRemoved();
                        _devices.Remove(dev.DisplayName);
                        if (listBox1.Items.Count == 0)
                            ShowCamera(null);
                        else if (selectedDevice == dev.DisplayName)
                        {
                            var newDev = _devMon.Devices.Where(d => d.DeviceClass == "Video/Source").FirstOrDefault();
                            ShowCamera(newDev.DisplayName);
                        }
                        break;
                    }
                default:
                    break;
            }
            return true;
        }

        void OnBusSyncMessage(object o, SyncMessageArgs args)
        {
            Bus bus = o as Bus;
            Gst.Message msg = args.Message;

            if (!Gst.Video.Global.IsVideoOverlayPrepareWindowHandleMessage(msg))
                return;
            Element src = msg.Src as Element;
            if (src == null)
                return;
            src = _pipeline;

            Element overlay = null
                ?? (src as Gst.Bin)?.GetByInterface(VideoOverlayAdapter.GType);
            Assert(overlay != null, "Overlay is null");

            VideoOverlayAdapter adapter = new VideoOverlayAdapter(overlay.Handle);
            adapter.WindowHandle = _videoPanelHandle;
            adapter.HandleEvents(true);
        }

        void ShowCamera(string name)
        {
            State state, pending;
            StateChangeReturn ret;
            $"Show camera {name}".PrintYellow();

            var camSource = _pipeline.GetByName("camsource");
            camSource.SetState(State.Null);
            if (name != null)
            {
                ret = camSource.GetState(out state, out pending, 100);
                camSource["device-name"] = name;
                camSource.SyncStateWithParent();
                ret = camSource.GetState(out state, out pending, 100);
                $"Camsource state = {state} Pending = {pending}".PrintMagenta();

                ret = _pipeline.GetState(out state, out pending, 100);
                if (state != State.Playing)
                {
                    if (_pipeline.SetState(State.Playing) == StateChangeReturn.Failure)
                    {
                        "Cannot set pipeline to playing state".PrintErr();
                        return;
                    }
                }
            }
        }

        void OnListBoxClick(object sender, EventArgs e)
        {
            ShowCamera(listBox1.SelectedItem.ToString());
        }
    }
}
