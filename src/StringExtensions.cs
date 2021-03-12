using System;

namespace Yoh.Text.Segmentation
{
    /// <summary>
    /// Provides text segmentation extension methods for the string and span-related types
    /// as specified in Annex #29 of the Unicode standard.
    /// </summary>
    public static class SegmentationExtensions
    {
        /// <summary>Returns an enumeration of word boundaries for the provided string.</summary>
        /// <param name="source">The source string for which word boundaries are enumerated.</param>
        /// <returns>A word boundary enumerator.</returns>
        public static WordBoundaryEnumerator EnumerateWordBoundaries(this string source) =>
            new WordBoundaryEnumerator(source);

        /// <summary>Returns an enumeration of word boundaries for the provided span.</summary>
        /// <param name="source">The source span for which word boundaries are enumerated.</param>
        /// <returns>A word boundary enumerator.</returns>
        public static WordBoundaryEnumerator EnumerateWordBoundaries(this Span<char> source) =>
            new WordBoundaryEnumerator(source);

        /// <summary>Returns an enumeration of word boundaries for the provided read-only span.</summary>
        /// <param name="source">The source read-only span for which word boundaries are enumerated.</param>
        /// <returns>A word boundary enumerator.</returns>
        public static WordBoundaryEnumerator EnumerateWordBoundaries(this ReadOnlySpan<char> source) =>
            new WordBoundaryEnumerator(source);

        /// <summary>Returns an enumeration of words for the provided string.</summary>
        /// <param name="source">The source string for which words are enumerated.</param>
        /// <returns>A word enumerator.</returns>
        public static WordEnumerator EnumerateWords(this string source) =>
            new WordEnumerator(source);

        /// <summary>Returns an enumeration of words for the provided span.</summary>
        /// <param name="source">The source span for which words are enumerated.</param>
        /// <returns>A word enumerator.</returns>
        public static WordEnumerator EnumerateWords(this Span<char> source) =>
            new WordEnumerator(source);

        /// <summary>Returns an enumeration of words for the provided read-only span.</summary>
        /// <param name="source">The source read-only span for which words are enumerated.</param>
        /// <returns>A word enumerator.</returns>
        public static WordEnumerator EnumerateWords(this ReadOnlySpan<char> source) =>
            new WordEnumerator(source);
    }
}
