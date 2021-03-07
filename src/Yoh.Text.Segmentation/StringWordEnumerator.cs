using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Yoh.Text.Segmentation
{
    public ref struct StringWordEnumerator
    {
        private ReadOnlySpan<char> _source;
        private ReadOnlySpan<char> _current;
        private RuneCategory _currentRune;
        private int _currentIndex;

        internal StringWordEnumerator(ReadOnlySpan<char> source)
        {
            _source = source;
            _current = default;
            _currentRune = default;
            _currentIndex = default;
        }

        public ReadOnlySpan<char> Current => _current;

        public StringWordEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if ((uint)_currentIndex >= _source.Length)
            {
                _current = default;
                return false;
            }

            var state = State.Start;
            var savedRune = default(RuneCategory);
            int savedIndex = default;
            int startIndex = _currentIndex;

            var handleFormatExtend = true;
            const State BreakMask = State.BreakConsumeLast | State.BreakConsumeNone;
            const State StateMask = ~BreakMask;

            while (_currentIndex < _source.Length)
            {
                var previousCategory = _currentRune.Category;

                if (_currentRune.Category == RuneWordCategory.None)
                    _currentRune = DecodeRune(_source);
                
                if (state != State.Start)
                {
                    if (_currentRune.Category == RuneWordCategory.Extend ||
                        _currentRune.Category == RuneWordCategory.Format ||
                        _currentRune.Category == RuneWordCategory.Zwj)
                    {
                        handleFormatExtend = false;
                        continue;
                    }
                }

                if (previousCategory == RuneWordCategory.Zwj &&
                    RuneInfo.IsEmoji(_currentRune.Rune))
                {
                    state = State.Emoji;
                    continue;
                }

                state = state switch
                {
                    State.Start => _currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.Katakana => State.Katakana,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        RuneWordCategory.RegionalIndicator => State.RegionalIndicatorHalf,
                        RuneWordCategory.Zwj => State.Zwj,
                        RuneWordCategory.WSegSpace => State.WSegSpace,
                        RuneWordCategory.Cr => HandleStartCr(ref this),
                        RuneWordCategory.Lf => State.BreakConsumeLast,
                        RuneWordCategory.Newline => State.BreakConsumeLast,
                        _ => HandleStart(ref this)
                    },
                    State.AHLetter or State.HLetter => _currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        RuneWordCategory.SingleQuote when state == State.HLetter => State.FormatExtendAcceptQLetter,
                        RuneWordCategory.SingleQuote or
                        RuneWordCategory.MidLetter or
                        RuneWordCategory.MidNum => FormatExtend(ref this, State.FormatExtendRequireAHLetter, ref savedRune, ref savedIndex),
                        _ => State.BreakConsumeNone | state
                    },
                    State.Numeric => _currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        RuneWordCategory.SingleQuote or
                        RuneWordCategory.MidLetter or
                        RuneWordCategory.MidNum => FormatExtend(ref this, State.FormatExtendRequireNumeric, ref savedRune, ref savedIndex),
                        _ => State.BreakConsumeNone | state
                    },
                    State.Katakana => _currentRune.Category switch
                    {
                        RuneWordCategory.Katakana => State.Katakana,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        _ => State.BreakConsumeNone
                    },
                    State.ExtendNumLet => _currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.Katakana => State.Katakana,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        _ => State.BreakConsumeNone  
                    },
                    State.RegionalIndicatorFull => State.BreakConsumeNone,
                    State.RegionalIndicatorHalf => _currentRune.Category == RuneWordCategory.RegionalIndicator
                        ? State.RegionalIndicatorFull
                        : State.BreakConsumeNone,
                    State.FormatExtendRequireNumeric when _currentRune.Category == RuneWordCategory.Numeric => State.Numeric,
                    State.FormatExtendRequireAHLetter or State.FormatExtendAcceptQLetter when _currentRune.Category == RuneWordCategory.ALetter => State.AHLetter,
                    State.FormatExtendRequireAHLetter or State.FormatExtendAcceptQLetter when _currentRune.Category == RuneWordCategory.HLetter => State.HLetter,
                    State.FormatExtendRequireHLetter when _currentRune.Category == RuneWordCategory.HLetter => State.HLetter,
                    State.FormatExtendAcceptNone or State.FormatExtendAcceptQLetter => State.BreakConsumeNone,
                    State.WSegSpace => _currentRune.Category == RuneWordCategory.WSegSpace && handleFormatExtend
                        ? State.WSegSpace
                        : State.BreakConsumeNone,
                    State.Emoji => State.BreakConsumeNone,
                    State.Zwj => State.BreakConsumeNone,
                    _ => State.BreakConsumeLast
                };

                if ((state & BreakMask) != default)
                    break;

                static State HandleStart(ref StringWordEnumerator self)
                {
                    var rune = DecodeRune(self._source);
                    if (rune.Category == RuneWordCategory.Extend ||
                        rune.Category == RuneWordCategory.Format ||
                        rune.Category == RuneWordCategory.Zwj)
                    {
                        self._currentRune = rune;
                        return State.FormatExtendAcceptNone;
                    }

                    return State.BreakConsumeLast;
                }

                static State HandleStartCr(ref StringWordEnumerator self)
                {
                    var rune = DecodeRune(self._source);
                    if (rune.Category == RuneWordCategory.Lf)
                    {
                        self._currentRune = default;
                        self._currentIndex += rune.Rune.Utf8SequenceLength;
                    }

                    return State.BreakConsumeNone;
                }

                static State FormatExtend(ref StringWordEnumerator self, State state, ref RuneCategory savedRune, ref int savedIndex)
                {
                    Debug.Assert(
                        state == State.FormatExtendRequireAHLetter ||
                        state == State.FormatExtendRequireHLetter ||
                        state == State.FormatExtendRequireNumeric);

                    savedRune = self._currentRune;
                    savedIndex = self._currentIndex;

                    return state;
                }

                static RuneCategory DecodeRune(ReadOnlySpan<char> source)
                {
                    return Rune.DecodeFromUtf16(source, out var rune, out _) == OperationStatus.Done
                        ? new RuneCategory(rune, RuneInfo.GetWordCategory(rune))
                        : new RuneCategory(rune, RuneWordCategory.Any);
                }
            }

            switch (state & StateMask)
            {
                case State.AHLetter:
                case State.HLetter:
                case State.Numeric:
                    _currentRune = savedRune;
                    _currentIndex = savedIndex;
                    break;
            }

            if (state.HasFlag(State.BreakConsumeLast))
            {
                _currentIndex += _currentRune.Rune.Utf16SequenceLength;
                _currentRune = default;
            }

            _current = _source[startIndex.._currentIndex];
            return true;
        }

        [Flags]
        private enum State
        {
            Start,
            BreakConsumeLast = 1 << 30,
            BreakConsumeNone = 1 << 31,
            AHLetter,
            HLetter,
            Numeric,
            Katakana,
            ExtendNumLet,
            RegionalIndicatorFull,
            RegionalIndicatorHalf,
            RegionalIndicatorUnknown,
            FormatExtendAcceptAny,
            FormatExtendAcceptNone,
            FormatExtendAcceptQLetter,
            FormatExtendRequireAHLetter,
            FormatExtendRequireHLetter,
            FormatExtendRequireNumeric,
            Zwj,
            Emoji,
            WSegSpace,
        }

        private readonly struct RuneCategory
        {
            public readonly Rune Rune;
            public readonly RuneWordCategory Category;

            public RuneCategory(Rune rune, RuneWordCategory category) =>
                (Rune, Category) = (rune, category);
        }
    }
}