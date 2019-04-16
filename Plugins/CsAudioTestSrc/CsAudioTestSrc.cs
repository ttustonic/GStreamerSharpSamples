using System;
using System.Collections.Generic;
using System.Linq;
using GLib;
using Gst;
using Gst.Audio;
using RGiesecke.DllExport;
using static Gst.Constants;
using static GstImports;
using static InitPlugin;

// https://github.com/GStreamer/gst-python/tree/master/examples/plugins/python

[TypeInitializer(typeof(CsAudioTestSrc), nameof(CsAudioTestSrc.Init))]
public class CsAudioTestSrc : Gst.Base.BaseSrc
{
    const string LONGNAME = "csaudiotestsrc";
    const string CLASSIFICATION = "Src";
    const string DESCRIPTION = "C# test src element";
    const string AUTHOR = "Tom";

    const uint SAMPLESPERBUFFER = 1024;
    const int DEFAULT_FREQ = 440;
    const float DEFAULT_VOLUME = 0.8f;
    const bool DEFAULT_MUTE = false;
    const bool DEFAULT_IS_LIVE = false;

    ulong _nextSample = 0;
    ulong _nextByte = 0;
    ulong _nextTime = 0;
    int _accumulator = 0;
    int _generateSamplesPerBuffer = 0;
    AudioInfo _info = new AudioInfo();

    static GLib.GType _gtype = GLib.GType.Invalid;
    static Type _thisType = typeof(CsAudioTestSrc);

    static Caps OCaps = Caps.FromString($"audio/x-raw, format=F32LE, layout=interleaved, rate=44100, channels=2");

    [GLib.Property("freq", "freqNick", "Frequency of test signal")]
    public uint Freq { get; set; } = DEFAULT_FREQ;

    [GLib.Property("volume", "volNick", "Volume of test signal")]
    public float Volume { get; set; } = DEFAULT_VOLUME;

    [GLib.Property("mute", "muteNick", "Mute the test signal")]
    public bool Mute { get; set; } = DEFAULT_MUTE;

    //bool _isLive = DEFAULT_IS_LIVE;
    //[GLib.Property("mute", "Is live", "Whether to act as a live source")]
    //public new bool IsLive
    //{
    //    get { return _isLive ; }
    //    set { _isLive = value; }
    //}

    #region Constructors

    static CsAudioTestSrc()
    {
        GtkSharp.GstreamerSharp.ObjectManager.Initialize();
    }

    public CsAudioTestSrc(IntPtr raw) : base(raw)
    {
        this.Format = Format.Time;
    }

    public CsAudioTestSrc() : base(IntPtr.Zero)
    {
        CreateNativeObject(new string[0], new GLib.Value[0]);
        this.Format = Format.Time;
    }
    #endregion

    #region Overrides

    protected override bool OnSetCaps(Caps caps)
    {
        _info.FromCaps(caps);
        Blocksize = (uint)_info.Bpf * SAMPLESPERBUFFER;
        return true;
    }

    protected override bool OnStart()
    {
        _nextSample = 0;
        _nextByte = 0;
        _nextTime = 0;
        _accumulator = 0;        
        _generateSamplesPerBuffer = (int)SAMPLESPERBUFFER;
        return true;
    }

    protected override bool OnQuery(Query query)
    {
// HACK!
        query.Refcount = 1;
        if (query.Type == QueryType.Latency)
        {
            var latency = Util.Uint64Scale((ulong)_generateSamplesPerBuffer, SECOND, (ulong)_info.Rate);
            var isLive = IsLive;
            query.SetLatency(IsLive, latency, CLOCK_TIME_NONE);
            return true;
        }
        else
            return base.OnQuery(query);
    }

    protected override void OnGetTimes(Gst.Buffer buffer, out ulong start, out ulong end)
    {
        start = CLOCK_TIME_NONE;
        end = CLOCK_TIME_NONE;
        if (IsLive)
        {
            var ts = buffer.Pts;
            if (ts != CLOCK_TIME_NONE)
            {
                var duration = buffer.Duration;
                if (duration != CLOCK_TIME_NONE)
                    end = ts + duration;
                start = ts;
            }
        }
    }

    protected override FlowReturn OnCreate(ulong offset, uint size, out Gst.Buffer buf)
    {
        int intSize = (int)size;
        if (intSize == -1)
        {
            throw new Exception();
        }

        var samples = (int)(size / _info.Bpf);
        this._generateSamplesPerBuffer = samples;
        var numBytes = samples * _info.Bpf;
        byte[] data;
        var nextSample = _nextSample + (ulong)samples;
        var nextByte = _nextByte + (ulong)numBytes;
        var nextTime = Util.Uint64Scale(_nextSample, SECOND, (ulong)_info.Rate);

        if (!Mute)
        {
            var fact = 2 * Math.PI * Freq / _info.Rate;
/*
            var sin = Enumerable.Range((int)_accumulator, samples)
                .Select(r => (float)(Math.Sin(fact * r) * Volume));
            var zip = sin.Zip(sin, (a, b) => new[] { a, b })
                .Aggregate((a, b) => a.Concat(b).ToArray());
*/
            float[] zip1 = new float[samples * 2];
            for (var i=0;i<samples;i++)
            {
                var r = _accumulator + i;
                var s = (float)(Math.Sin(fact * r) * Volume);
                zip1[2*i] = s;
                zip1[2 * i + 1] = s;
            }
            data = zip1.SelectMany(z => BitConverter.GetBytes(z)).ToArray();
        }
        else
        {
            data = Enumerable.Repeat<byte>(0, (int)numBytes).ToArray();
        }

        buf = new Gst.Buffer(data);
        buf.Offset = this._nextSample;
        buf.OffsetEnd = nextSample;
        buf.Pts = this._nextTime;
        buf.Duration = nextTime - this._nextTime;
        _nextTime = nextTime;
        _nextSample = nextSample;
        _nextByte = nextByte;
        _accumulator += samples;
        _accumulator %= (int)(_info.Rate / Freq);
        return FlowReturn.Ok;
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

        PadTemplate srcTmpl = new PadTemplate("src",
                                        PadDirection.Src,
                                        PadPresence.Always,
                                        OCaps);
        gst_element_class_add_pad_template(ptr, srcTmpl.OwnedHandle);
    }

    #endregion

    #region Plugin stuff
    public static bool RegisterPlugin(Plugin plugin)
    {
        Console.WriteLine("Register csaudiotestsrc");
        var name = _thisType.Name.ToLower();
        return Element.Register(plugin, name, 0, CsAudioTestSrc.GType);
    }

    public static bool plugin_init(Plugin plugin)
    {
        return RegisterPlugin(plugin);
    }

    [DllExport("gst_plugin_csaudiotestsrc_get_desc")]
    public static IntPtr gst_plugin_csaudiotestsrc_get_desc()
    {
        var pluginDesc = GetPluginDesc(_thisType.Name.ToLower(),
            "test csaudiotestsrc", plugin_init, "0.0.1", "LGPL",
            _thisType.Name.ToLower(),
            _thisType.Name.ToLower(),
            "http://test.com"
            );
        return pluginDesc;
    }
    #endregion
}

