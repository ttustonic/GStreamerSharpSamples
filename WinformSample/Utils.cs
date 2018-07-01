using System;
using static System.ConsoleColor;

namespace WinformSample
{
    public static class Utils
    {
        public static TimeSpan RoundSeconds(this TimeSpan span, int nDigits)
        {
            return TimeSpan.FromSeconds(Math.Round(span.TotalSeconds, nDigits));
        }

        public static int PrintErr(this string format, params object[] args)
        {
            var s = String.Format(format, args);
            PrintColor(s, Red);
            return -1;
        }

        public static void PrintYellow(this string format, params object[] args)
        {
            PrintColor(String.Format(format, args), Yellow);
        }

        public static void PrintGreen(this string format, params object[] args)
        {
            PrintColor(String.Format(format, args), Green);
        }

        public static void PrintMagenta(this string format, params object[] args)
        {
            PrintColor(String.Format(format, args), Magenta);
        }

        public static void PrintColor(this string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ResetColor();
        }

    }
}
