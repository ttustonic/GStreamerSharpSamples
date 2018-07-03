using System;
using System.Collections.Generic;
using System.Dynamic;
using static System.ConsoleColor;

namespace GstSamples
{
    public static class Utils
    {
        public static int PrintErr(this string format, params object[] args)
        {
            var s = String.Format(format, args);
            PrintColor(s, Red);
            return -1;
        }

        public static void PrintYellow(this string s)
        {
            PrintColor(s, Yellow);
        }

        public static void PrintGreen(this string s)
        {
            PrintColor(s, Green);
        }

        public static void PrintMagenta(this string s)
        {
            PrintColor(s, Magenta);
        }

        public static void PrintColor(this string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public static bool HasProperty(this ExpandoObject obj, string propertyName)
        {
            return ((IDictionary<String, object>)obj).ContainsKey(propertyName);
        }
    }
}
