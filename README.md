# GStreamerSharpSamples
Tutorials and samples for GStreamerSharp

C# tutorials translated to C# from https://gstreamer.freedesktop.org/documentation/tutorials/. These versions are different than the official samples (https://github.com/GStreamer/gstreamer-sharp/tree/master/samples).

Additionally, there are a few demos of using web cameras with GStreamer Sharp and WinForms and WPF versions of Basic tutorial 5 and VideoOverlay samples (https://github.com/GStreamer/gstreamer-sharp/blob/master/samples/VideoOverlay.cs).

Some of the samples didn't work with the official GstSharp NuGet package (https://www.nuget.org/packages/GstSharp/), so there's also a gstreamer-sharp, recompiled as .Net standard 2.0 project, which depends on GioSharp package (https://www.nuget.org/packages/GioSharp).

Also, UI projects need a NuGet package GtkSharp.Win32 (https://www.nuget.org/packages/GtkSharp.Win32/), while command line samples work fine with the MSYS2 build of GTK. MSYS2 installation and build are described here: http://grbd.github.io/posts/2016/06/30/building-gtk3-gtksharp-under-windows-manual-build/





