using System;
using System.Runtime.InteropServices;
using GLib;
using Gst;
using Gst.Base;
using RGiesecke.DllExport;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static GstImports;
using static InitPlugin;

// https://github.com/GStreamer/gst-python/tree/master/examples/plugins/python
[TypeInitializer(typeof(CsVideoMixer), nameof(CsVideoMixer.Init))]
public class CsVideoMixer : Gst.Base.Aggregator
{
    const int WIDTH = 320;
    const int HEIGHT = 200;

    const string LONGNAME = "csvideomixer";
    const string CLASSIFICATION = "Video/Mixer";
    const string DESCRIPTION = "C# video mixer";
    const string AUTHOR = "Tom";
    static GLib.GType _gtype = GLib.GType.Invalid;
    static Type _thisType = typeof(CsVideoMixer);

    static Caps ICaps = Caps.FromString($"video/x-raw,format=RGBA,width={WIDTH},height={HEIGHT},framerate=(fraction)30/1");
    static Caps OCaps = Caps.FromString($"video/x-raw,format=RGBA,width={WIDTH},height={HEIGHT},framerate=(fraction)30/1");

    #region Constructors

    static CsVideoMixer()
    {
        GtkSharp.GstreamerSharp.ObjectManager.Initialize();
    }

    public CsVideoMixer(IntPtr raw) : base(raw)
    { }

    public CsVideoMixer() : base(IntPtr.Zero)
    {
        CreateNativeObject(new string[0], new GLib.Value[0]);
    }
    #endregion

    #region Overrides
    protected override FlowReturn OnAggregate(bool timeout)
    {
        ulong pts = 0;
        bool eos = false;
        Image<Rgba32> outImg;
        using (outImg = new Image<Rgba32>(Configuration.Default, WIDTH, HEIGHT))
        {
            ForeachSinkPad(mixBuffers);
            var f = outImg.Frames[0];
            var span = f.GetPixelSpan();
            var bytes = MemoryMarshal.AsBytes(span).ToArray();
            var outBuff = new Gst.Buffer(null, (ulong)bytes.LongLength, AllocationParams.Zero);
            outBuff.Fill(0, bytes);
            outBuff.Pts = pts;
            var ret = FinishBuffer(outBuff);
        }

        return eos ? FlowReturn.Eos : FlowReturn.Ok;

        bool mixBuffers(Element agg, Pad pad)
        {
            var aggPad = pad as AggregatorPad;
            var buff = aggPad.PopBuffer();
            var isMap = buff.Map(out MapInfo info, MapFlags.Read);
            using (var img = Image.LoadPixelData<Rgba32>(info.Data, WIDTH, HEIGHT))
            {
                outImg.Mutate((ctx) => ctx.DrawImage(img, PixelColorBlendingMode.Lighten, 0.5f));
                pts = buff.Pts;
                eos = false;
            }
            buff.Unmap(info);
            return true;
        }
    }
    #endregion

    #region Init class
    public static new GType GType
    {
        get
        {
            if (_gtype == GType.Invalid)
                _gtype = RegisterGType(_thisType);
            return _gtype;
        }
    }

    static void Init(GType gType, Type type)
    {
        var ptr = gType.GetClassPtr();
        gst_element_class_set_metadata(ptr, LONGNAME, CLASSIFICATION, DESCRIPTION, AUTHOR);
        PadTemplate sinkTmpl = new PadTemplate("sink_%u",
                                        PadDirection.Sink,
                                        PadPresence.Request,
                                        ICaps, AggregatorPad.GType);
        PadTemplate srcTmpl = new PadTemplate("src",
                                        PadDirection.Src,
                                        PadPresence.Always,
                                        OCaps, AggregatorPad.GType);

        gst_element_class_add_pad_template(ptr, srcTmpl.OwnedHandle);
        gst_element_class_add_pad_template(ptr, sinkTmpl.OwnedHandle);
    }

    #endregion

    #region Plugin stuff
    public static bool RegisterPlugin(Plugin plugin)
    {
        Console.WriteLine("Register csvideomixer");
        var name = _thisType.Name.ToLower();
        var isReg = Element.Register(plugin, name, 0, CsVideoMixer.GType);
        return isReg;
    }

    public static bool plugin_init(Plugin plugin)
    {
        return RegisterPlugin(plugin);
    }

    [DllExport("gst_plugin_csvideomixer_get_desc", CallingConvention.StdCall)]
    public static IntPtr gst_plugin_csvideomixer_get_desc()
    {
        var pluginDesc = GetPluginDesc(_thisType.Name.ToLower(),
            "test csvideomixer", plugin_init, "0.0.1", "LGPL",
            _thisType.Name.ToLower(),
            _thisType.Name.ToLower(),
            "http://test.com"
            );
        return pluginDesc;
    }

    #endregion
}

