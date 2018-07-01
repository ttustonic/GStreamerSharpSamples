//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;

namespace GstSamples
{
    [Flags]
    public enum GstPlayFlags: uint
    {
        Video = 0x00000001, // Render the video stream
        Audio = 0x00000002, // Render the audio stream
        Text = 0x00000003, //  Render subtitles
        Vis = 0x00000008, // Render visualisation when no video is present
        SoftVolume = 0x00000010, // Use software volume
        NativeAudio = 0x00000020, // Only use native audio formats
        NativeVideo = 0x00000040, // Only use native video formats
        Download = 0x00000080, // Attempt progressive download buffering
        Buffering = 0x00000100, // Buffer demuxed/parsed data
        Deinterlace = 0x00000200, // Deinterlace video if necessary
        SoftColorbalance = 0x00000400, // Use software color balance
        ForceFilters = 0x00000800 // Force audio/video filter(s) to be applied
    }
}
