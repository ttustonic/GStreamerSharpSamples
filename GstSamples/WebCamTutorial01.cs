//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Enumerate devices and detect added/removed
    /// </summary>
    static class WebCamTutorial01
    {
        public static void Run(string[] args)
        {
            Application.Init(ref args);
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            var devmon = new DeviceMonitor();
//            var caps = new Caps("video/x-raw");
//            var filtId = devmon.AddFilter("Video/Source", caps);
            
            var bus = devmon.Bus;
            bus.AddWatch(OnBusMessage);
            if (!devmon.Start())
            {
                "Device monitor cannot start".PrintErr();
                return;
            }

            Console.WriteLine("Video devices count = " + devmon.Devices.Length);
            foreach (var dev in devmon.Devices)
                DumpDevice(dev);

            var loop = new GLib.MainLoop();
            loop.Run();
        }

        static void DumpDevice(Device d)
        {
            Console.WriteLine($"{d.DeviceClass} : {d.DisplayName} : {d.Name} ");
        }

        static bool OnBusMessage(Bus bus, Message message)
        {
            switch (message.Type)
            {
                case MessageType.DeviceAdded:
                    {
                        var dev = message.ParseDeviceAdded();
                        Console.WriteLine("Device added: ");
                        DumpDevice(dev);
                        break;
                    }
                case MessageType.DeviceRemoved:
                    {
                        var dev = message.ParseDeviceRemoved();
                        Console.WriteLine("Device removed: ");
                        DumpDevice(dev);
                        break;
                    }
            }
            return true;
        }
    }
}
