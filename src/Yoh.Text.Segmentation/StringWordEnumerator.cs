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

        internal StringWordEnumerator(ReadOnlySpan<char> source)
        {
            _source = source;
            _current = default;
            _currentRune = default;
        }

        public ReadOnlySpan<char> Current => _current;

        public StringWordEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_source.IsEmpty)
            {
                _current = default;
                return false;
            }

            var state = State.Start;
            var savedRune = default(RuneCategory);
            var savedIndex = 0;

            var handleFormatExtend = true;
            const State BreakMask = State.BreakConsumeLast | State.BreakConsumeNone;
            const State StateMask = ~BreakMask;

            var previousZwt = _currentRune.Category == RuneWordCategory.Zwj;
            var currentRune = _currentRune;
            var currentIndex = 0;

            while (currentIndex < _source.Length)
            {
                if (currentRune.Category == RuneWordCategory.None)
                    currentRune = DecodeRune(_source, currentIndex);

                _currentRune = default;

                if (state != State.Start)
                {
                    if (currentRune.Category == RuneWordCategory.Extend ||
                        currentRune.Category == RuneWordCategory.Format ||
                        currentRune.Category == RuneWordCategory.Zwj)
                    {
                        handleFormatExtend = false;
                        goto Continue;
                    }
                }

                if (previousZwt && RuneInfo.IsEmoji(currentRune.Rune))
                {
                    state = State.Emoji;
                    goto Continue;
                }

                state = state switch
                {
                    State.Start => currentRune.Category switch
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
                    State.AHLetter or State.HLetter => currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        RuneWordCategory.DoubleQuote when state == State.HLetter => FormatExtend(State.FormatExtendRequireHLetter),
                        RuneWordCategory.SingleQuote when state == State.HLetter => State.FormatExtendAcceptQLetter,
                        RuneWordCategory.SingleQuote or
                        RuneWordCategory.MidLetter or
                        RuneWordCategory.MidNumLet => FormatExtend(State.FormatExtendRequireAHLetter),
                        _ => State.BreakConsumeNone | state
                    },
                    State.Numeric => currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        RuneWordCategory.SingleQuote or
                        RuneWordCategory.MidNum or
                        RuneWordCategory.MidNumLet => FormatExtend(State.FormatExtendRequireNumeric),
                        _ => State.BreakConsumeNone | state
                    },
                    State.Katakana => currentRune.Category switch
                    {
                        RuneWordCategory.Katakana => State.Katakana,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        _ => State.BreakConsumeNone
                    },
                    State.ExtendNumLet => currentRune.Category switch
                    {
                        RuneWordCategory.ALetter => State.AHLetter,
                        RuneWordCategory.HLetter => State.HLetter,
                        RuneWordCategory.Numeric => State.Numeric,
                        RuneWordCategory.Katakana => State.Katakana,
                        RuneWordCategory.ExtendNumLet => State.ExtendNumLet,
                        _ => State.BreakConsumeNone
                    },
                    State.RegionalIndicatorFull => State.BreakConsumeNone,
                    State.RegionalIndicatorHalf => currentRune.Category == RuneWordCategory.RegionalIndicator
                        ? State.RegionalIndicatorFull
                        : State.BreakConsumeNone,
                    State.FormatExtendRequireNumeric when currentRune.Category == RuneWordCategory.Numeric => State.Numeric,
                    State.FormatExtendRequireAHLetter or State.FormatExtendAcceptQLetter when currentRune.Category == RuneWordCategory.ALetter => State.AHLetter,
                    State.FormatExtendRequireAHLetter or State.FormatExtendAcceptQLetter when currentRune.Category == RuneWordCategory.HLetter => State.HLetter,
                    State.FormatExtendRequireHLetter when currentRune.Category == RuneWordCategory.HLetter => State.HLetter,
                    State.FormatExtendAcceptNone or State.FormatExtendAcceptQLetter => State.BreakConsumeNone,
                    State.WSegSpace => currentRune.Category == RuneWordCategory.WSegSpace && handleFormatExtend
                        ? State.WSegSpace
                        : State.BreakConsumeNone,
                    State.Emoji => State.BreakConsumeNone,
                    State.Zwj => State.BreakConsumeNone,
                    _ => State.BreakConsumeLast | state
                };

                State HandleStart(ref StringWordEnumerator self)
                {
                    var index = currentIndex + currentRune.Length;
                    var rune = DecodeRune(self._source, index);
                    if (rune.Category == RuneWordCategory.Extend ||
                        rune.Category == RuneWordCategory.Format ||
                        rune.Category == RuneWordCategory.Zwj)
                    {
                        self._currentRune = rune;
                        return State.FormatExtendAcceptNone;
                    }

                    return State.BreakConsumeLast;
                }

                State HandleStartCr(ref StringWordEnumerator self)
                {
                    var index = currentIndex + currentRune.Length;
                    var rune = DecodeRune(self._source, index);
                    if (rune.Category == RuneWordCategory.Lf)
                    {
                        currentRune = rune;
                        currentIndex = index;
                    }

                    return State.BreakConsumeLast;
                }

                State FormatExtend(State state)
                {
                    Debug.Assert(
                        state == State.FormatExtendRequireAHLetter ||
                        state == State.FormatExtendRequireHLetter ||
                        state == State.FormatExtendRequireNumeric);

                    savedRune = currentRune;
                    savedIndex = currentIndex;

                    return state;
                }

                static RuneCategory DecodeRune(ReadOnlySpan<char> source, int index)
                {
                    return Rune.DecodeFromUtf16(source.Slice(index), out var rune, out _) == OperationStatus.Done
                        ? new RuneCategory(rune, RuneInfo.GetWordCategory(rune))
                        : new RuneCategory(rune, RuneWordCategory.Any);
                }

                if ((state & BreakMask) != default)
                {
                    break;
                }

            Continue:
                previousZwt = currentRune.Category == RuneWordCategory.Zwj;

                currentIndex += currentRune.Length;
                currentRune = _currentRune;
            }

            var stateNoBreak = state & StateMask;
            if (stateNoBreak == State.FormatExtendRequireAHLetter ||
                stateNoBreak == State.FormatExtendRequireHLetter ||
                stateNoBreak == State.FormatExtendRequireNumeric)
            {
                _currentRune = savedRune;
                currentIndex = savedIndex;
            }
            else
            {
                if (state.HasFlag(State.BreakConsumeLast))
                {
                    _currentRune = default;
                    currentIndex += currentRune.Length;
                }
                else
                {
                    _currentRune = currentRune;
                }
            }

            _current = _source[0..currentIndex];
            _source = _source[currentIndex..];

            return true;
        }

        [Flags]
        private enum State
        {
            Start,
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
            BreakConsumeLast = 1 << 30,
            BreakConsumeNone = 1 << 31,
        }

        private readonly struct RuneCategory
        {
            public Rune Rune { get; }
            public RuneWordCategory Category { get; }
            public int Length => Rune.Utf16SequenceLength;

            public RuneCategory(Rune rune, RuneWordCategory category) =>
                (Rune, Category) = (rune, category);
        }
    }
}
