// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Gst.Video {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[Flags]
	[GLib.GType (typeof (Gst.Video.VideoChromaSiteGType))]
	public enum VideoChromaSite {

		Unknown = 0,
		None = 1,
		Jpeg = 1,
		HCosited = 2,
		Mpeg2 = 2,
		VCosited = 4,
		Cosited = 6,
		AltLine = 8,
		Dv = 14,
	}

	internal class VideoChromaSiteGType {
		[DllImport ("libgstvideo-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_video_chroma_site_get_type ();

		public static GLib.GType GType {
			get {
				return new GLib.GType (gst_video_chroma_site_get_type ());
			}
		}
	}
#endregion
}
