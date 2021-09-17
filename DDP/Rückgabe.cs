using System;

namespace DDP
{
    internal class Rückgabe : Exception
    {
        public readonly object wert;

        public Rückgabe(object wert)
        {
            this.wert = wert;
        }
    }
}
