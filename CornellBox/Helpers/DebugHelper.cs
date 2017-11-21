using System;
using System.Diagnostics.Contracts;

namespace CornellBox.Helpers
{
    public static class DebugHelper
    {
        public static void CheckRange<T>(T x, T min, T max, string message) where T:IComparable
        {
            var a = x.CompareTo(min);
            var b = x.CompareTo(max);

            Contract.Assert(a != -1 && b != 1, message);
        }
    }
}
