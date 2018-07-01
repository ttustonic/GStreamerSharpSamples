// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Gst.Video {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[StructLayout(LayoutKind.Sequential)]
	public partial struct VideoResampler : IEquatable<VideoResampler> {

		public int InSize;
		public int OutSize;
		public uint MaxTaps;
		public uint NPhases;
		public uint Offset;
		private IntPtr _phase;
		private IntPtr _n_taps;
		private IntPtr _taps;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst=4)]
		private IntPtr[] _gstGstReserved;

		public static Gst.Video.VideoResampler Zero = new Gst.Video.VideoResampler ();

		public static Gst.Video.VideoResampler New(IntPtr raw) {
			if (raw == IntPtr.Zero)
				return Gst.Video.VideoResampler.Zero;
			return (Gst.Video.VideoResampler) Marshal.PtrToStructure (raw, typeof (Gst.Video.VideoResampler));
		}

		[DllImport("libgstvideo-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_video_resampler_clear(IntPtr raw);

		public void Clear() {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			gst_video_resampler_clear(this_as_native);
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
		}

		[DllImport("libgstvideo-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_video_resampler_init(IntPtr raw, int method, int flags, uint n_phases, uint n_taps, double shift, uint in_size, uint out_size, IntPtr options);

		public bool Init(Gst.Video.VideoResamplerMethod method, Gst.Video.VideoResamplerFlags flags, uint n_phases, uint n_taps, double shift, uint in_size, uint out_size, Gst.Structure options) {
			IntPtr this_as_native = System.Runtime.InteropServices.Marshal.AllocHGlobal (System.Runtime.InteropServices.Marshal.SizeOf (this));
			System.Runtime.InteropServices.Marshal.StructureToPtr (this, this_as_native, false);
			bool raw_ret = gst_video_resampler_init(this_as_native, (int) method, (int) flags, n_phases, n_taps, shift, in_size, out_size, options == null ? IntPtr.Zero : options.Handle);
			bool ret = raw_ret;
			ReadNative (this_as_native, ref this);
			System.Runtime.InteropServices.Marshal.FreeHGlobal (this_as_native);
			return ret;
		}

		static void ReadNative (IntPtr native, ref Gst.Video.VideoResampler target)
		{
			target = New (native);
		}

		public bool Equals (VideoResampler other)
		{
			return true && InSize.Equals (other.InSize) && OutSize.Equals (other.OutSize) && MaxTaps.Equals (other.MaxTaps) && NPhases.Equals (other.NPhases) && Offset.Equals (other.Offset) && _phase.Equals (other._phase) && _n_taps.Equals (other._n_taps) && _taps.Equals (other._taps);
		}

		public override bool Equals (object other)
		{
			return other is VideoResampler && Equals ((VideoResampler) other);
		}

		public override int GetHashCode ()
		{
			return this.GetType ().FullName.GetHashCode () ^ InSize.GetHashCode () ^ OutSize.GetHashCode () ^ MaxTaps.GetHashCode () ^ NPhases.GetHashCode () ^ Offset.GetHashCode () ^ _phase.GetHashCode () ^ _n_taps.GetHashCode () ^ _taps.GetHashCode ();
		}

		private static GLib.GType GType {
			get { return GLib.GType.Pointer; }
		}
#endregion
	}
}
