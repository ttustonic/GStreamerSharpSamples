// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GstSharp {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void MiniObjectFreeFunctionNative(IntPtr obj);

	internal class MiniObjectFreeFunctionInvoker {

		MiniObjectFreeFunctionNative native_cb;
		IntPtr __data;
		GLib.DestroyNotify __notify;

		~MiniObjectFreeFunctionInvoker ()
		{
			if (__notify == null)
				return;
			__notify (__data);
		}

		internal MiniObjectFreeFunctionInvoker (MiniObjectFreeFunctionNative native_cb) : this (native_cb, IntPtr.Zero, null) {}

		internal MiniObjectFreeFunctionInvoker (MiniObjectFreeFunctionNative native_cb, IntPtr data) : this (native_cb, data, null) {}

		internal MiniObjectFreeFunctionInvoker (MiniObjectFreeFunctionNative native_cb, IntPtr data, GLib.DestroyNotify notify)
		{
			this.native_cb = native_cb;
			__data = data;
			__notify = notify;
		}

		internal Gst.MiniObjectFreeFunction Handler {
			get {
				return new Gst.MiniObjectFreeFunction(InvokeNative);
			}
		}

		void InvokeNative (Gst.MiniObject obj)
		{
			native_cb (obj == null ? IntPtr.Zero : obj.Handle);
		}
	}

	internal class MiniObjectFreeFunctionWrapper {

		public void NativeCallback (IntPtr obj)
		{
			try {
				managed (obj == IntPtr.Zero ? null : (Gst.MiniObject) GLib.Opaque.GetOpaque (obj, typeof (Gst.MiniObject), false));
				if (release_on_call)
					gch.Free ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		bool release_on_call = false;
		GCHandle gch;

		public void PersistUntilCalled ()
		{
			release_on_call = true;
			gch = GCHandle.Alloc (this);
		}

		internal MiniObjectFreeFunctionNative NativeDelegate;
		Gst.MiniObjectFreeFunction managed;

		public MiniObjectFreeFunctionWrapper (Gst.MiniObjectFreeFunction managed)
		{
			this.managed = managed;
			if (managed != null)
				NativeDelegate = new MiniObjectFreeFunctionNative (NativeCallback);
		}

		public static Gst.MiniObjectFreeFunction GetManagedDelegate (MiniObjectFreeFunctionNative native)
		{
			if (native == null)
				return null;
			MiniObjectFreeFunctionWrapper wrapper = (MiniObjectFreeFunctionWrapper) native.Target;
			if (wrapper == null)
				return null;
			return wrapper.managed;
		}
	}
#endregion
}