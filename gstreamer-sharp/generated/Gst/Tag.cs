// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Gst {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public partial class Tag {

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_exists(IntPtr tag);

		public static bool Exists(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			bool raw_ret = gst_tag_exists(native_tag);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_description(IntPtr tag);

		public static string GetDescription(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			IntPtr raw_ret = gst_tag_get_description(native_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int gst_tag_get_flag(IntPtr tag);

		public static Gst.TagFlag GetFlag(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			int raw_ret = gst_tag_get_flag(native_tag);
			Gst.TagFlag ret = (Gst.TagFlag) raw_ret;
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_nick(IntPtr tag);

		public static string GetNick(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			IntPtr raw_ret = gst_tag_get_nick(native_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_type(IntPtr tag);

		public static GLib.GType GetType(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			IntPtr raw_ret = gst_tag_get_type(native_tag);
			GLib.GType ret = new GLib.GType(raw_ret);
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_is_fixed(IntPtr tag);

		public static bool IsFixed(string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			bool raw_ret = gst_tag_is_fixed(native_tag);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_list_copy_value(IntPtr dest, IntPtr list, IntPtr tag);

		public static bool ListCopyValue(ref GLib.Value dest, Gst.TagList list, string tag) {
			IntPtr native_dest = GLib.Marshaller.StructureToPtrAlloc (dest);
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			bool raw_ret = gst_tag_list_copy_value(native_dest, list == null ? IntPtr.Zero : list.Handle, native_tag);
			bool ret = raw_ret;
			dest = (GLib.Value) Marshal.PtrToStructure (native_dest, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_dest);
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_tag_merge_strings_with_comma(IntPtr dest, IntPtr src);

		public static GLib.Value MergeStringsWithComma(GLib.Value src) {
			GLib.Value dest;
			IntPtr native_dest = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (GLib.Value)));
			IntPtr native_src = GLib.Marshaller.StructureToPtrAlloc (src);
			gst_tag_merge_strings_with_comma(native_dest, native_src);
			dest = (GLib.Value) Marshal.PtrToStructure (native_dest, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_dest);
			Marshal.FreeHGlobal (native_src);
			return dest;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_tag_merge_use_first(IntPtr dest, IntPtr src);

		public static GLib.Value MergeUseFirst(GLib.Value src) {
			GLib.Value dest;
			IntPtr native_dest = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (GLib.Value)));
			IntPtr native_src = GLib.Marshaller.StructureToPtrAlloc (src);
			gst_tag_merge_use_first(native_dest, native_src);
			dest = (GLib.Value) Marshal.PtrToStructure (native_dest, typeof (GLib.Value));
			Marshal.FreeHGlobal (native_dest);
			Marshal.FreeHGlobal (native_src);
			return dest;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_tag_register(IntPtr name, int flag, IntPtr type, IntPtr nick, IntPtr blurb, GstSharp.TagMergeFuncNative func);

		public static void Register(string name, Gst.TagFlag flag, GLib.GType type, string nick, string blurb, Gst.TagMergeFunc func) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			IntPtr native_nick = GLib.Marshaller.StringToPtrGStrdup (nick);
			IntPtr native_blurb = GLib.Marshaller.StringToPtrGStrdup (blurb);
			GstSharp.TagMergeFuncWrapper func_wrapper = new GstSharp.TagMergeFuncWrapper (func);
			gst_tag_register(native_name, (int) flag, type.Val, native_nick, native_blurb, func_wrapper.NativeDelegate);
			GLib.Marshaller.Free (native_name);
			GLib.Marshaller.Free (native_nick);
			GLib.Marshaller.Free (native_blurb);
		}

		public static void Register(string name, Gst.TagFlag flag, GLib.GType type, string nick, string blurb) {
			Register (name, flag, type, nick, blurb, null);
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_tag_register_static(IntPtr name, int flag, IntPtr type, IntPtr nick, IntPtr blurb, GstSharp.TagMergeFuncNative func);

		public static void RegisterStatic(string name, Gst.TagFlag flag, GLib.GType type, string nick, string blurb, Gst.TagMergeFunc func) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			IntPtr native_nick = GLib.Marshaller.StringToPtrGStrdup (nick);
			IntPtr native_blurb = GLib.Marshaller.StringToPtrGStrdup (blurb);
			GstSharp.TagMergeFuncWrapper func_wrapper = new GstSharp.TagMergeFuncWrapper (func);
			gst_tag_register_static(native_name, (int) flag, type.Val, native_nick, native_blurb, func_wrapper.NativeDelegate);
			GLib.Marshaller.Free (native_name);
			GLib.Marshaller.Free (native_nick);
			GLib.Marshaller.Free (native_blurb);
		}

		public static void RegisterStatic(string name, Gst.TagFlag flag, GLib.GType type, string nick, string blurb) {
			RegisterStatic (name, flag, type, nick, blurb, null);
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_check_language_code(IntPtr lang_code);

		public static bool CheckLanguageCode(string lang_code) {
			IntPtr native_lang_code = GLib.Marshaller.StringToPtrGStrdup (lang_code);
			bool raw_ret = gst_tag_check_language_code(native_lang_code);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_lang_code);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_freeform_string_to_utf8(IntPtr data, int size, IntPtr env_vars);

		public static string FreeformStringToUtf8(string data, int size, string env_vars) {
			IntPtr native_data = GLib.Marshaller.StringToPtrGStrdup (data);
			IntPtr native_env_vars = GLib.Marshaller.StringToPtrGStrdup (env_vars);
			IntPtr raw_ret = gst_tag_freeform_string_to_utf8(native_data, size, native_env_vars);
			string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
			GLib.Marshaller.Free (native_data);
			GLib.Marshaller.Free (native_env_vars);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_from_id3_tag(IntPtr id3_tag);

		public static string FromId3Tag(string id3_tag) {
			IntPtr native_id3_tag = GLib.Marshaller.StringToPtrGStrdup (id3_tag);
			IntPtr raw_ret = gst_tag_from_id3_tag(native_id3_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_id3_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_from_id3_user_tag(IntPtr type, IntPtr id3_user_tag);

		public static string FromId3UserTag(string type, string id3_user_tag) {
			IntPtr native_type = GLib.Marshaller.StringToPtrGStrdup (type);
			IntPtr native_id3_user_tag = GLib.Marshaller.StringToPtrGStrdup (id3_user_tag);
			IntPtr raw_ret = gst_tag_from_id3_user_tag(native_type, native_id3_user_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_type);
			GLib.Marshaller.Free (native_id3_user_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_from_vorbis_tag(IntPtr vorbis_tag);

		public static string FromVorbisTag(string vorbis_tag) {
			IntPtr native_vorbis_tag = GLib.Marshaller.StringToPtrGStrdup (vorbis_tag);
			IntPtr raw_ret = gst_tag_from_vorbis_tag(native_vorbis_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_vorbis_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint gst_tag_get_id3v2_tag_size(IntPtr buffer);

		public static uint GetId3v2TagSize(Gst.Buffer buffer) {
			uint raw_ret = gst_tag_get_id3v2_tag_size(buffer == null ? IntPtr.Zero : buffer.Handle);
			uint ret = raw_ret;
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_language_code_iso_639_1(IntPtr lang_code);

		public static string GetLanguageCodeIso6391(string lang_code) {
			IntPtr native_lang_code = GLib.Marshaller.StringToPtrGStrdup (lang_code);
			IntPtr raw_ret = gst_tag_get_language_code_iso_639_1(native_lang_code);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_lang_code);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_language_code_iso_639_2B(IntPtr lang_code);

		public static string GetLanguageCodeIso6392B(string lang_code) {
			IntPtr native_lang_code = GLib.Marshaller.StringToPtrGStrdup (lang_code);
			IntPtr raw_ret = gst_tag_get_language_code_iso_639_2B(native_lang_code);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_lang_code);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_language_code_iso_639_2T(IntPtr lang_code);

		public static string GetLanguageCodeIso6392T(string lang_code) {
			IntPtr native_lang_code = GLib.Marshaller.StringToPtrGStrdup (lang_code);
			IntPtr raw_ret = gst_tag_get_language_code_iso_639_2T(native_lang_code);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_lang_code);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_language_codes();

		public static string[] GetLanguageCodes() {
			IntPtr raw_ret = gst_tag_get_language_codes();
			string[] ret = GLib.Marshaller.NullTermPtrToStringArray (raw_ret, true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_language_name(IntPtr language_code);

		public static string GetLanguageName(string language_code) {
			IntPtr native_language_code = GLib.Marshaller.StringToPtrGStrdup (language_code);
			IntPtr raw_ret = gst_tag_get_language_name(native_language_code);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_language_code);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_license_description(IntPtr license_ref);

		public static string GetLicenseDescription(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			IntPtr raw_ret = gst_tag_get_license_description(native_license_ref);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int gst_tag_get_license_flags(IntPtr license_ref);

		public static Gst.Tags.TagLicenseFlags GetLicenseFlags(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			int raw_ret = gst_tag_get_license_flags(native_license_ref);
			Gst.Tags.TagLicenseFlags ret = (Gst.Tags.TagLicenseFlags) raw_ret;
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_license_jurisdiction(IntPtr license_ref);

		public static string GetLicenseJurisdiction(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			IntPtr raw_ret = gst_tag_get_license_jurisdiction(native_license_ref);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_license_nick(IntPtr license_ref);

		public static string GetLicenseNick(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			IntPtr raw_ret = gst_tag_get_license_nick(native_license_ref);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_license_title(IntPtr license_ref);

		public static string GetLicenseTitle(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			IntPtr raw_ret = gst_tag_get_license_title(native_license_ref);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_license_version(IntPtr license_ref);

		public static string GetLicenseVersion(string license_ref) {
			IntPtr native_license_ref = GLib.Marshaller.StringToPtrGStrdup (license_ref);
			IntPtr raw_ret = gst_tag_get_license_version(native_license_ref);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_license_ref);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_get_licenses();

		public static string[] GetLicenses() {
			IntPtr raw_ret = gst_tag_get_licenses();
			string[] ret = GLib.Marshaller.NullTermPtrToStringArray (raw_ret, true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint gst_tag_id3_genre_count();

		public static uint Id3GenreCount() {
			uint raw_ret = gst_tag_id3_genre_count();
			uint ret = raw_ret;
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_id3_genre_get(uint id);

		public static string Id3GenreGet(uint id) {
			IntPtr raw_ret = gst_tag_id3_genre_get(id);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_image_data_to_image_sample(byte image_data, uint image_data_len, int image_type);

		public static Gst.Sample ImageDataToImageSample(byte image_data, uint image_data_len, Gst.Tags.TagImageType image_type) {
			IntPtr raw_ret = gst_tag_image_data_to_image_sample(image_data, image_data_len, (int) image_type);
			Gst.Sample ret = raw_ret == IntPtr.Zero ? null : (Gst.Sample) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.Sample), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_list_add_id3_image(IntPtr tag_list, byte image_data, uint image_data_len, uint id3_picture_type);

		public static bool ListAddId3Image(Gst.TagList tag_list, byte image_data, uint image_data_len, uint id3_picture_type) {
			bool raw_ret = gst_tag_list_add_id3_image(tag_list == null ? IntPtr.Zero : tag_list.Handle, image_data, image_data_len, id3_picture_type);
			bool ret = raw_ret;
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_exif_buffer(IntPtr buffer, int byte_order, uint base_offset);

		public static Gst.TagList ListFromExifBuffer(Gst.Buffer buffer, int byte_order, uint base_offset) {
			IntPtr raw_ret = gst_tag_list_from_exif_buffer(buffer == null ? IntPtr.Zero : buffer.Handle, byte_order, base_offset);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_exif_buffer_with_tiff_header(IntPtr buffer);

		public static Gst.TagList ListFromExifBufferWithTiffHeader(Gst.Buffer buffer) {
			IntPtr raw_ret = gst_tag_list_from_exif_buffer_with_tiff_header(buffer == null ? IntPtr.Zero : buffer.Handle);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_id3v2_tag(IntPtr buffer);

		public static Gst.TagList ListFromId3v2Tag(Gst.Buffer buffer) {
			IntPtr raw_ret = gst_tag_list_from_id3v2_tag(buffer == null ? IntPtr.Zero : buffer.Handle);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_vorbiscomment(byte data, UIntPtr size, byte id_data, uint id_data_length, IntPtr vendor_string);

		public static Gst.TagList ListFromVorbiscomment(byte data, ulong size, byte id_data, uint id_data_length, string vendor_string) {
			IntPtr native_vendor_string = GLib.Marshaller.StringToPtrGStrdup (vendor_string);
			IntPtr raw_ret = gst_tag_list_from_vorbiscomment(data, new UIntPtr (size), id_data, id_data_length, native_vendor_string);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			GLib.Marshaller.Free (native_vendor_string);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_vorbiscomment_buffer(IntPtr buffer, byte id_data, uint id_data_length, IntPtr vendor_string);

		public static Gst.TagList ListFromVorbiscommentBuffer(Gst.Buffer buffer, byte id_data, uint id_data_length, string vendor_string) {
			IntPtr native_vendor_string = GLib.Marshaller.StringToPtrGStrdup (vendor_string);
			IntPtr raw_ret = gst_tag_list_from_vorbiscomment_buffer(buffer == null ? IntPtr.Zero : buffer.Handle, id_data, id_data_length, native_vendor_string);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			GLib.Marshaller.Free (native_vendor_string);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_from_xmp_buffer(IntPtr buffer);

		public static Gst.TagList ListFromXmpBuffer(Gst.Buffer buffer) {
			IntPtr raw_ret = gst_tag_list_from_xmp_buffer(buffer == null ? IntPtr.Zero : buffer.Handle);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_new_from_id3v1(byte data);

		public static Gst.TagList ListNewFromId3v1(byte data) {
			IntPtr raw_ret = gst_tag_list_new_from_id3v1(data);
			Gst.TagList ret = raw_ret == IntPtr.Zero ? null : (Gst.TagList) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.TagList), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_to_exif_buffer(IntPtr taglist, int byte_order, uint base_offset);

		public static Gst.Buffer ListToExifBuffer(Gst.TagList taglist, int byte_order, uint base_offset) {
			IntPtr raw_ret = gst_tag_list_to_exif_buffer(taglist == null ? IntPtr.Zero : taglist.Handle, byte_order, base_offset);
			Gst.Buffer ret = raw_ret == IntPtr.Zero ? null : (Gst.Buffer) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.Buffer), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_to_exif_buffer_with_tiff_header(IntPtr taglist);

		public static Gst.Buffer ListToExifBufferWithTiffHeader(Gst.TagList taglist) {
			IntPtr raw_ret = gst_tag_list_to_exif_buffer_with_tiff_header(taglist == null ? IntPtr.Zero : taglist.Handle);
			Gst.Buffer ret = raw_ret == IntPtr.Zero ? null : (Gst.Buffer) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.Buffer), true);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_to_vorbiscomment_buffer(IntPtr list, byte id_data, uint id_data_length, IntPtr vendor_string);

		public static Gst.Buffer ListToVorbiscommentBuffer(Gst.TagList list, byte id_data, uint id_data_length, string vendor_string) {
			IntPtr native_vendor_string = GLib.Marshaller.StringToPtrGStrdup (vendor_string);
			IntPtr raw_ret = gst_tag_list_to_vorbiscomment_buffer(list == null ? IntPtr.Zero : list.Handle, id_data, id_data_length, native_vendor_string);
			Gst.Buffer ret = raw_ret == IntPtr.Zero ? null : (Gst.Buffer) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.Buffer), true);
			GLib.Marshaller.Free (native_vendor_string);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_list_to_xmp_buffer(IntPtr list, bool read_only, IntPtr schemas);

		public static Gst.Buffer ListToXmpBuffer(Gst.TagList list, bool read_only, string schemas) {
			IntPtr native_schemas = GLib.Marshaller.StringToPtrGStrdup (schemas);
			IntPtr raw_ret = gst_tag_list_to_xmp_buffer(list == null ? IntPtr.Zero : list.Handle, read_only, native_schemas);
			Gst.Buffer ret = raw_ret == IntPtr.Zero ? null : (Gst.Buffer) GLib.Opaque.GetOpaque (raw_ret, typeof (Gst.Buffer), true);
			GLib.Marshaller.Free (native_schemas);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gst_tag_parse_extended_comment(IntPtr ext_comment, IntPtr key, IntPtr lang, IntPtr value, bool fail_if_no_key);

		public static bool ParseExtendedComment(string ext_comment, string key, string lang, string value, bool fail_if_no_key) {
			IntPtr native_ext_comment = GLib.Marshaller.StringToPtrGStrdup (ext_comment);
			IntPtr native_key = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr native_lang = GLib.Marshaller.StringToPtrGStrdup (lang);
			IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
			bool raw_ret = gst_tag_parse_extended_comment(native_ext_comment, native_key, native_lang, native_value, fail_if_no_key);
			bool ret = raw_ret;
			GLib.Marshaller.Free (native_ext_comment);
			GLib.Marshaller.Free (native_key);
			GLib.Marshaller.Free (native_lang);
			GLib.Marshaller.Free (native_value);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gst_tag_register_musicbrainz_tags();

		public static void RegisterMusicbrainzTags() {
			gst_tag_register_musicbrainz_tags();
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_to_id3_tag(IntPtr gst_tag);

		public static string ToId3Tag(string gst_tag) {
			IntPtr native_gst_tag = GLib.Marshaller.StringToPtrGStrdup (gst_tag);
			IntPtr raw_ret = gst_tag_to_id3_tag(native_gst_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_gst_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_to_vorbis_comments(IntPtr list, IntPtr tag);

		public static string[] ToVorbisComments(Gst.TagList list, string tag) {
			IntPtr native_tag = GLib.Marshaller.StringToPtrGStrdup (tag);
			IntPtr raw_ret = gst_tag_to_vorbis_comments(list == null ? IntPtr.Zero : list.Handle, native_tag);
			string[] ret = (string[]) GLib.Marshaller.ListPtrToArray (raw_ret, typeof(GLib.List), true, true, typeof(string));
			GLib.Marshaller.Free (native_tag);
			return ret;
		}

		[DllImport("libgstreamer-1.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gst_tag_to_vorbis_tag(IntPtr gst_tag);

		public static string ToVorbisTag(string gst_tag) {
			IntPtr native_gst_tag = GLib.Marshaller.StringToPtrGStrdup (gst_tag);
			IntPtr raw_ret = gst_tag_to_vorbis_tag(native_gst_tag);
			string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
			GLib.Marshaller.Free (native_gst_tag);
			return ret;
		}

#endregion
	}
}
