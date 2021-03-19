using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Yoh.Text.Segmentation.Benchmarks
{
    [DisassemblyDiagnoser(printSource: true)]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        public static void Main(string[] args) =>
            BenchmarkRunner.Run<Benchmarks>();

        private const string Value = "The quick (“brown”) fox can’t jump 32.3 feet, right?";

        [Benchmark]
        public int EnumerateWordBoundaries()
        {
            var count = 0;
            foreach (var current in Value.EnumerateWordBoundaries())
                count += 1;
            return count;
        }

        [Benchmark]
        public int EnumerateWords()
        {
            var count = 0;
            foreach (var current in Value.EnumerateWords())
                count += 1;
            return count;
        }
    }
}
