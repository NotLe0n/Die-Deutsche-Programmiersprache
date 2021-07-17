using System.Collections.Generic;

namespace DDP
{
    public static class Extentions
    {
        public static bool IsAlpha(this char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_' || c == 'ä' || c == 'ö' || c == 'ü' || c == 'ß';
        }

        public static bool IsAlphaNumeric(this char c)
        {
            return IsAlpha(c) || char.IsDigit(c);
        }

        public static TValue Put<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }

            return value;
        }

        public static object Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.TryGetValue(key, out TValue val))
            {
                return val;
            }
            else
            {
                return null;
            }
        }

        public static string Stringify(object obj)
        {
            if (obj == null) return "nix";

            if (obj is bool boolean)
            {
                return boolean ? "wahr" : "falsch";
            }

            return obj.ToString();
        }
    }
}
