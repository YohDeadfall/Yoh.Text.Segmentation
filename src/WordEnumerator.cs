using System;

namespace Yoh.Text.Segmentation
{
    public ref struct WordEnumerator
    {
        private WordBoundaryEnumerator _boundaries;

        internal WordEnumerator(ReadOnlySpan<char> source) =>
            _boundaries = new WordBoundaryEnumerator(source);

        public ReadOnlySpan<char> Current => _boundaries.Current;

        public WordEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
        Next:
            if (_boundaries.MoveNext())
            {
                foreach (var c in _boundaries.Current)
                    if (char.IsLetterOrDigit(c)) return true;
                goto Next;
            }

            return false;
        }
    }
}