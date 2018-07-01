using System;

namespace WpfSamples
{
    public static class Utils
    {
        public static TimeSpan RoundSeconds(this TimeSpan span, int nDigits)
        {
            return TimeSpan.FromSeconds(Math.Round(span.TotalSeconds, nDigits));
        }
    }
}
