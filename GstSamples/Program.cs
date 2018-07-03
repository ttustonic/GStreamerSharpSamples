namespace GstSamples
{
    class Program
    {
        public static void Main(string[] args)
        {
            GtkSharp.GstreamerSharp.ObjectManager.Initialize();
            GLib.GType.Register(Gst.WebRTC.WebRTCSessionDescription.GType, typeof(Gst.WebRTC.WebRTCSessionDescription));
            Gst.Application.Init();
            GLib.GType.Register(Gst.WebRTC.WebRTCSessionDescription.GType, typeof(Gst.WebRTC.WebRTCSessionDescription));

            WebRtcSendRcv.Run(args); // 2858);//, "wss://webrtc.nirbheek.in:8443");
        }
    }
}