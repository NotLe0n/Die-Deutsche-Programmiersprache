﻿using System.Linq;

namespace DDP
{
    public static class Erweiterungen
    {
        public static bool IstDeutsch(this char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_' || c == 'ä' || c == 'ö' || c == 'ü' || c == 'ß';
        }

        public static bool IsAlphaNumeric(this char c)
        {
            return IstDeutsch(c) || char.IsDigit(c);
        }

        public static bool IstNumerisch(this string str)
        {
            return str.ToCharArray().All(x => char.IsDigit(x));
        }

        public static string Stringify(object obj)
        {
            if (obj == null) return "nix";

            if (obj is bool boolean)
            {
                return boolean ? "wahr" : "falsch";
            }

            if (obj is System.Array)
            {
                var arr = obj as object[];

                string str = "[";
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] is string) str += '"';
                    str += Stringify(arr[i]);
                    if (arr[i] is string) str += '"';

                    if (i + 1 < arr.Length)
                        str += "; ";
                }
                return str + "]";
            }

            return obj.ToString();
        }
    }
}
