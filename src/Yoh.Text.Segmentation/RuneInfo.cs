using System;
using System.Runtime.CompilerServices;

namespace Yoh.Text.Segmentation
{
    internal static partial class RuneInfo
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TRange? BinarySearch<TRange, TValue>(ReadOnlySpan<TRange> ranges, TValue value)
            where TRange : struct, IRange<TValue>
            where TValue : struct, IComparable<TValue>
        {
            int lo = 0;
            int hi = ranges.Length - 1;
            
            while (lo <= hi)
            {
                int pivot = lo + ((hi - lo) >> 1);
                var range = ranges[pivot];

                if (value.CompareTo(range.Start) < 0)
                {
                    hi = pivot - 1;
                    continue;
                }

                if (value.CompareTo(range.End) > 0)
                {
                    lo = pivot + 1;
                    continue;
                }
                
                return range;
            }

            return null;
        }

        private interface IRange<T>
            where T : IComparable<T>
        {
            T Start { get; }
            T End { get; }
        }
    }
}