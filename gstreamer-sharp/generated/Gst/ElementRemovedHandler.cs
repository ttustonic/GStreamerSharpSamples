// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Gst {

	using System;

	public delegate void ElementRemovedHandler(object o, ElementRemovedArgs args);

	public class ElementRemovedArgs : GLib.SignalArgs {
		public Gst.Element Element{
			get {
				return (Gst.Element) Args [0];
			}
		}

	}
}
