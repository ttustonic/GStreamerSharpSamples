using System;
using System.Runtime.InteropServices;
using GLib;
using Gst;
using RGiesecke.DllExport;
using static GstImports;
using static InitPlugin;

// https://github.com/GStreamer/gst-python/tree/master/examples/plugins/python
[TypeInitializer(typeof(SimpleIdentity), nameof(SimpleIdentity.Init))]
public class SimpleIdentity : Gst.Base.BaseTransform
{
    const string LONGNAME = "simpleidentity";
    const string CLASSIFICATION = "Transform";
    const string DESCRIPTION = "simple identity";
    const string AUTHOR = "Tom";
    static GLib.GType _gtype = GLib.GType.Invalid;
    static Type _thisType = typeof(SimpleIdentity);

    #region Constructors

    static SimpleIdentity()
    {
        GtkSharp.GstreamerSharp.ObjectManager.Initialize();
    }

    public SimpleIdentity(IntPtr raw) : base(raw)
    {
    }

    public SimpleIdentity() : base(IntPtr.Zero)
    {
        CreateNativeObject(new string[0], new GLib.Value[0]);
    }
    #endregion

    protected override FlowReturn OnTransform(Gst.Buffer inbuf, Gst.Buffer outbuf)
    {
        Console.WriteLine($"{inbuf.Dts} : {inbuf.Pts}");
        return FlowReturn.Ok;
    }

    #region Init class
    static void Init(GType gType, Type type)
    {
        var ptr = gType.GetClassPtr();
        gst_element_class_set_metadata(ptr, LONGNAME, CLASSIFICATION, DESCRIPTION, AUTHOR);
        PadTemplate srcTmpl = new PadTemplate("src", PadDirection.Src, PadPresence.Always, Caps.NewAny());
        PadTemplate sinkTmpl = new PadTemplate("sink", PadDirection.Sink, PadPresence.Always, Caps.NewAny());
        gst_element_class_add_pad_template(ptr, srcTmpl.OwnedHandle);
        gst_element_class_add_pad_template(ptr, sinkTmpl.OwnedHandle);
    }

    public static new GType GType
    {
        get
        {
            if (_gtype == GLib.GType.Invalid)
                _gtype = RegisterGType(_thisType);
            return _gtype;
        }
    }
    #endregion

    #region Plugin stuff
    public static bool RegisterPlugin(Plugin plugin)
    {
        Console.WriteLine("Register simpleidentity");
        var name = _thisType.Name.ToLower();
        var isReg = Gst.Element.Register(plugin, name, 0, SimpleIdentity.GType);
        return isReg;
    }

    public static bool plugin_init(Plugin plugin)
    {
        return RegisterPlugin(plugin);
    }

    [DllExport("gst_plugin_simpleidentity_get_desc", CallingConvention.StdCall)]
    public static IntPtr gst_plugin_simpleidentity_get_desc()
    {
        var pluginDesc = GetPluginDesc(_thisType.Name.ToLower(),
            "test simple identity", plugin_init, "0.0.1", "LGPL",
            _thisType.Name.ToLower(),
            _thisType.Name.ToLower(),
            "http://test.com"
            );
        return pluginDesc;
    }
    #endregion

}

