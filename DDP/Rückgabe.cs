using System;

namespace DDP
{
    class Rückgabe : Exception
    {
        public readonly object wert;

        public Rückgabe(object wert)
        {
            this.wert = wert;
        }
    }
}
