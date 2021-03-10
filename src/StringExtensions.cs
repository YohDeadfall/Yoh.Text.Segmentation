using System;

namespace Yoh.Text.Segmentation
{
    public static class StringExtensions
    {
        public static StringWordEnumerator EnumerateWords(this string source) =>
            new StringWordEnumerator(source);

        public static StringWordEnumerator EnumerateWords(this Span<char> source) =>
            new StringWordEnumerator(source);

        public static StringWordEnumerator EnumerateWords(this ReadOnlySpan<char> source) =>
            new StringWordEnumerator(source);
    }
}
