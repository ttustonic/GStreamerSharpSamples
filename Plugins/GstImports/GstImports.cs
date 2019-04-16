using System;
using System.Runtime.InteropServices;
using Gst;

internal static class GstImports
{
    [DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void gst_element_class_set_static_metadata(IntPtr klass, string longname, string classification, string description, string author);
    [DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void gst_element_class_set_metadata(IntPtr klass, string longname, string classification, string description, string author);
    [DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void gst_element_class_add_pad_template(IntPtr klass, IntPtr templ);
}

internal static class InitPlugin
{
    public static IntPtr GetPluginDesc(string name, string desc,
        Gst.PluginInitFunc plugin_init,
        string version, string license, string source, string package,
        string origin
        )
    {
        var p = new PluginDesc();
        p.MajorVersion = Gst.PbUtils.Constants.PLUGINS_BASE_VERSION_MAJOR;
        p.MinorVersion = Gst.PbUtils.Constants.PLUGINS_BASE_VERSION_MINOR;
        p.Name = name;
        p.Description = desc;
        p.PluginInit = plugin_init;
        p.Version = version;
        p.License = license;
        p.Source = source;
        p.Package = package;
        p.Origin = origin;
        p.ReleaseDatetime = null;

        var pluginDesc = Marshal.AllocHGlobal(Marshal.SizeOf(p));
        Marshal.StructureToPtr(p, pluginDesc, false);
        return pluginDesc;
    }
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool PluginInitFuncNative(IntPtr plugin);

internal class PluginInitFuncWrapper
{
    public bool NativeCallback(IntPtr plugin)
    {
        try
        {
            bool __ret = managed(GLib.Object.GetObject(plugin) as Gst.Plugin);
            if (release_on_call)
                gch.Free();
            return __ret;
        }
        catch (Exception e)
        {
            GLib.ExceptionManager.RaiseUnhandledException(e, false);
            return false;
        }
    }

    bool release_on_call = false;
    GCHandle gch;

    public void PersistUntilCalled()
    {
        release_on_call = true;
        gch = GCHandle.Alloc(this);
    }

    internal PluginInitFuncNative NativeDelegate;
    Gst.PluginInitFunc managed;

    public PluginInitFuncWrapper(PluginInitFunc managed)
    {
        this.managed = managed;
        if (managed != null)
            NativeDelegate = new PluginInitFuncNative(NativeCallback);
    }

    public static PluginInitFunc GetManagedDelegate(PluginInitFuncNative native)
    {
        if (native == null)
            return null;
        PluginInitFuncWrapper wrapper = (PluginInitFuncWrapper)native.Target;
        if (wrapper == null)
            return null;
        return wrapper.managed;
    }
}

[StructLayout(LayoutKind.Sequential)]
public partial struct PluginDesc
{
    public int MajorVersion;
    public int MinorVersion;
    public string Name;
    public string Description;
    PluginInitFuncNative _plugin_init;
    public string Version;
    public string License;
    public string Source;
    public string Package;
    public string Origin;
    public string ReleaseDatetime;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    private IntPtr[] _gstGstReserved;

    public PluginInitFunc PluginInit
    {
        get
        {
            return PluginInitFuncWrapper.GetManagedDelegate(_plugin_init);
        }
        set
        {
            PluginInitFuncWrapper init_func_wrapper = new PluginInitFuncWrapper(value);
            _plugin_init = init_func_wrapper.NativeDelegate;
        }
    }

    public static PluginDesc Zero = new PluginDesc();

    public static PluginDesc New(IntPtr raw)
    {
        if (raw == IntPtr.Zero)
            return PluginDesc.Zero;
        return (PluginDesc)Marshal.PtrToStructure(raw, typeof(PluginDesc));
    }
}
