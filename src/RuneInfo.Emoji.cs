using System.Text;

namespace Yoh.Text.Segmentation
{
    internal static partial class RuneInfo
    {
        internal static bool IsEmoji(Rune rune) =>
            BinarySearch<EmojiRange, Rune>(Emojis, rune).HasValue;

        private readonly struct EmojiRange : IRange<Rune>
        {
            public Rune Start { get; }

            public Rune End { get; }

            public EmojiRange(uint start, uint end) =>
                (Start, End) = (new Rune(start), new Rune(end));
        }

        private static readonly EmojiRange[] Emojis = new EmojiRange[]
        {
            new(0x000000a9, 0x000000a9),
            new(0x000000ae, 0x000000ae),
            new(0x0000203c, 0x0000203c),
            new(0x00002049, 0x00002049),
            new(0x00002122, 0x00002122),
            new(0x00002139, 0x00002139),
            new(0x00002194, 0x00002199),
            new(0x000021a9, 0x000021aa),
            new(0x0000231a, 0x0000231b),
            new(0x00002328, 0x00002328),
            new(0x00002388, 0x00002388),
            new(0x000023cf, 0x000023cf),
            new(0x000023e9, 0x000023f3),
            new(0x000023f8, 0x000023fa),
            new(0x000024c2, 0x000024c2),
            new(0x000025aa, 0x000025ab),
            new(0x000025b6, 0x000025b6),
            new(0x000025c0, 0x000025c0),
            new(0x000025fb, 0x000025fe),
            new(0x00002600, 0x00002605),
            new(0x00002607, 0x00002612),
            new(0x00002614, 0x00002685),
            new(0x00002690, 0x00002705),
            new(0x00002708, 0x00002712),
            new(0x00002714, 0x00002714),
            new(0x00002716, 0x00002716),
            new(0x0000271d, 0x0000271d),
            new(0x00002721, 0x00002721),
            new(0x00002728, 0x00002728),
            new(0x00002733, 0x00002734),
            new(0x00002744, 0x00002744),
            new(0x00002747, 0x00002747),
            new(0x0000274c, 0x0000274c),
            new(0x0000274e, 0x0000274e),
            new(0x00002753, 0x00002755),
            new(0x00002757, 0x00002757),
            new(0x00002763, 0x00002767),
            new(0x00002795, 0x00002797),
            new(0x000027a1, 0x000027a1),
            new(0x000027b0, 0x000027b0),
            new(0x000027bf, 0x000027bf),
            new(0x00002934, 0x00002935),
            new(0x00002b05, 0x00002b07),
            new(0x00002b1b, 0x00002b1c),
            new(0x00002b50, 0x00002b50),
            new(0x00002b55, 0x00002b55),
            new(0x00003030, 0x00003030),
            new(0x0000303d, 0x0000303d),
            new(0x00003297, 0x00003297),
            new(0x00003299, 0x00003299),
            new(0x0001f000, 0x0001f0ff),
            new(0x0001f10d, 0x0001f10f),
            new(0x0001f12f, 0x0001f12f),
            new(0x0001f16c, 0x0001f171),
            new(0x0001f17e, 0x0001f17f),
            new(0x0001f18e, 0x0001f18e),
            new(0x0001f191, 0x0001f19a),
            new(0x0001f1ad, 0x0001f1e5),
            new(0x0001f201, 0x0001f20f),
            new(0x0001f21a, 0x0001f21a),
            new(0x0001f22f, 0x0001f22f),
            new(0x0001f232, 0x0001f23a),
            new(0x0001f23c, 0x0001f23f),
            new(0x0001f249, 0x0001f3fa),
            new(0x0001f400, 0x0001f53d),
            new(0x0001f546, 0x0001f64f),
            new(0x0001f680, 0x0001f6ff),
            new(0x0001f774, 0x0001f77f),
            new(0x0001f7d5, 0x0001f7ff),
            new(0x0001f80c, 0x0001f80f),
            new(0x0001f848, 0x0001f84f),
            new(0x0001f85a, 0x0001f85f),
            new(0x0001f888, 0x0001f88f),
            new(0x0001f8ae, 0x0001f8ff),
            new(0x0001f90c, 0x0001f93a),
            new(0x0001f93c, 0x0001f945),
            new(0x0001f947, 0x0001faff),
            new(0x0001fc00, 0x0001fffd),
        };
    }
}
