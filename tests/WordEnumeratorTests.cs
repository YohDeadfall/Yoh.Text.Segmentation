using System;
using System.Collections.Generic;
using Xunit;

namespace Yoh.Text.Segmentation.Tests
{
    public class WordEnumeratorTests
    {
        [Theory]
        [MemberData(nameof(WordsData))]
        public void EnumerateWords(string input, IEnumerable<string> output)
        {
            var actual = input.EnumerateWords().GetEnumerator();
            var expected = output.GetEnumerator();

            while (true)
            {
                var actualNext = actual.MoveNext();
                var expectedNext = expected.MoveNext();

                Assert.Equal(expectedNext, actualNext);

                if (!actualNext)
                    break;

                var actualCurrent = actual.Current;
                var expectedCurrent = expected.Current.AsSpan();

                Assert.True(expectedCurrent.SequenceEqual(actualCurrent));
            }
        }

        public static IEnumerable<object[]> WordsData { get; } = new[]
        {
            new object[]
            {
                "The quick (“brown”) fox can’t jump 32.3 feet, right?",
                new [] { "The", "quick", "brown", "fox", "can’t", "jump", "32.3", "feet", "right" }
            },
        };
    }
}
