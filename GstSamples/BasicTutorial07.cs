//   Copyright (C) 2018 Tomislav Tustonic <ttustonic@outlook.com>
using System;
using System.Linq;
using Gst;

namespace GstSamples
{
    /// <summary>
    /// Multithreading and Pad Availability
    /// </summary>
    static class BasicTutorial7
    {
        public static void Run(string[] args)
        {
            Application.Init(ref args);

            var audioSource = ElementFactory.Make("audiotestsrc", "audio_source");
            var tee = ElementFactory.Make("tee", "tee");
            var audioQueue = ElementFactory.Make("queue", "audio_queue");
            var audioConvert = ElementFactory.Make("audioconvert", "audio_convert");
            var audioResample = ElementFactory.Make("audioresample", "audio_resample");
            var audioSink = ElementFactory.Make("autoaudiosink", "audio_sink");
            var videoQueue = ElementFactory.Make("queue", "video_queue");
            var visual = ElementFactory.Make("wavescope", "visual");
            var videoConvert = ElementFactory.Make("videoconvert", "csp");
            var videoSink = ElementFactory.Make("autovideosink", "video_sink");

            var pipeline = new Pipeline("test-pipeline");

            if ((new[] { audioSource, tee, audioQueue, audioConvert, audioResample, audioSink, videoQueue, visual, videoConvert, videoSink }).Any(e => e == null))
            {
                "Not all elements could be created".PrintErr();
                return;
            }

            audioSource["freq"] = 215.0f;
            visual["shader"] = 0;
            visual["style"] = 1;

            pipeline.Add(audioSource, tee, audioQueue, audioConvert, audioResample, audioSink, videoQueue, visual, videoConvert, videoSink);

            /* Link all elements that can be automatically linked because they have "Always" pads */
            if (!audioSource.Link(tee) ||
                !Element.Link(audioQueue, audioConvert, audioResample, audioSink) ||
                !Element.Link(videoQueue, visual, videoConvert, videoSink))
            {
                "Elements could not be linked".PrintErr();
                return;
            }
/*
            var teeSrcPadTemplate = tee.GetPadTemplate ("src_%u");
            var teeAudioPad = tee.RequestPad (teeSrcPadTemplate, null, null);
            Console.WriteLine ("Obtained request pad {0} for audio branch.", teeAudioPad.Name);
            var queueAudioPad = audioQueue.GetStaticPad ("sink");
            var teeVideoPad = tee.RequestPad (teeSrcPadTemplate, null, null);
            Console.WriteLine ("Obtained request pad {0} for video branch.", teeVideoPad.Name);
            var queueVideoPad = videoQueue.GetStaticPad ("sink");
*/
            var teeAudioPad = tee.GetRequestPad("src_%u"); // from gst-inspect
            Console.WriteLine("Obtained request pad {0} for audio branch.", teeAudioPad.Name);
            var teeVideoPad = tee.GetRequestPad("src_%u");
            Console.WriteLine("Obtained request pad {0} for video branch.", teeVideoPad.Name);

            var queueAudioPad = audioQueue.GetStaticPad("sink");
            var queueVideoPad = videoQueue.GetStaticPad("sink");

            if (teeAudioPad.Link(queueAudioPad) != PadLinkReturn.Ok ||
                teeVideoPad.Link(queueVideoPad) != PadLinkReturn.Ok)
            {
                "Tee could not be linked".PrintErr();
                return;
            }

            // Start playing the pipeline
            pipeline.SetState(State.Playing);

            var bus = pipeline.Bus;
            // Wait until error or EOS
            var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Error | MessageType.Eos);

            // Release the request pads from the Tee, and unref them
            tee.ReleaseRequestPad(teeAudioPad);
            tee.ReleaseRequestPad(teeVideoPad);

            // Free resources
            pipeline.SetState(State.Null);
        }
    }
}
