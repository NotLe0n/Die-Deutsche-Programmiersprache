using System.Collections.Generic;

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
