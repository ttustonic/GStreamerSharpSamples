//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Linq;
using System.Threading;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Change camera on add / remove
    /// </summary>
    class WebCamTutorial02
    {
        static Pipeline _pipeline;
        static DeviceMonitor _devMon;

        static Pipeline CreatePipeline(string name)
        {
//            var pipe = (Pipeline)Parse.Launch("ksvideosrc name=camsource ! decodebin name = decode ! videoconvert name=convert ! videoscale ! autovideosink name=sink");
            var pipe = (Pipeline)Parse.Launch("ksvideosrc name=camsource ! videoconvert name=convert ! videoscale ! autovideosink name=sink");
            return pipe;
        }

        public static void Run(string[] args)
        {
            Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            _pipeline = CreatePipeline("cam_pipeline");

            var bus = _pipeline.Bus;
            bus.AddWatch(OnBusMessage);

            _devMon = new DeviceMonitor();
            var caps = new Caps("video/x-raw");
            var filtId = _devMon.AddFilter("Video/Source", caps);

            if (!_devMon.Start())
            {
                "Device monitor cannot start".PrintErr();
                return;
            }

            if (_devMon.Devices.Length == 0)
                Console.WriteLine("No video sources");
            else
            {
                Console.WriteLine($"Video devices count = {_devMon.Devices.Length}");
                foreach (var dev in _devMon.Devices)
                    DumpDevice(dev);
                var cam = _devMon.Devices.FirstOrDefault(d => d.DeviceClass == "Video/Source");
                if (cam != null)
                {
                    Console.WriteLine("Cam found");
                    ShowCamera(cam.DisplayName);
                }
            }

            var devMonBus = _devMon.Bus;
            devMonBus.AddWatch(OnBusMessage);

            var loop = new GLib.MainLoop();
            loop.Run();
        }
        
        static void ShowCamera(string name)
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

        static bool OnBusMessage(Bus bus, Message message)
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
                        Console.WriteLine("Device added: ");
                        DumpDevice(dev);
                        if (dev.DeviceClass == "Video/Source")
                            ShowCamera(dev.DisplayName);
                        break;
                    }
                case MessageType.DeviceRemoved:
                    {
                        var dev = message.ParseDeviceRemoved();
                        Console.WriteLine("Device removed: ");
                        DumpDevice(dev);
                        var newDev = _devMon.Devices.Where(d => d.DeviceClass == "Video/Source").FirstOrDefault();
                        if (newDev == null)
                        {
                            "No video sources".PrintErr();
                            ShowCamera(null);
                        }
                        else
                        {
                            var devName = newDev.DisplayName;
                            ShowCamera(devName);
                        }
                        break;
                    }
                case MessageType.StateChanged:
                    var src = (Element)message.Src;
                    message.ParseStateChanged(out State oldState, out State newState, out State pending);
                    Console.WriteLine($"State for {src.Name} changed from {Element.StateGetName(oldState)} to {Element.StateGetName(newState)}");
                    break;
                default:
                    break;
            }
            return true;
        }

        static void DumpDevice(Device d)
        {
            Console.WriteLine($"{d.DeviceClass} : {d.DisplayName} : {d.Name} : {d.PathString} ");
        }
    }
}
