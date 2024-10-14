using System;

namespace HprServiceReference
{
    internal static class HprServiceExtensions
    {
        internal static bool Aktiv(this Periode periode)
            => periode.Fra < DateTime.Now && (periode.Til == null || periode.Til > DateTime.Now);
    }
}