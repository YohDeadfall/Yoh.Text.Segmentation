using System;

namespace Yoh.Text.Segmentation
{
    public static class StringExtensions
    {
        public static WordBoundaryEnumerator EnumerateWordBoundaries(this string source) =>
            new WordBoundaryEnumerator(source);

        public static WordBoundaryEnumerator EnumerateWordBoundaries(this Span<char> source) =>
            new WordBoundaryEnumerator(source);

        public static WordBoundaryEnumerator EnumerateWordBoundaries(this ReadOnlySpan<char> source) =>
            new WordBoundaryEnumerator(source);

        public static WordEnumerator EnumerateWords(this string source) =>
            new WordEnumerator(source);

        public static WordEnumerator EnumerateWords(this Span<char> source) =>
            new WordEnumerator(source);

        public static WordEnumerator EnumerateWords(this ReadOnlySpan<char> source) =>
            new WordEnumerator(source);
    }
}
