using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;
using DiffMatchPatch;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class SpeedTestBenchmark
    {
        readonly string _file1;
        readonly string _file2;

        public SpeedTestBenchmark()
        {
            _file1 = File.ReadAllText("speedtest1.txt");
            _file2 = File.ReadAllText("speedtest2.txt");
        }
        
        [Benchmark]
        public void CheckLines_True_SpeedOptimize_True()
        {
            var difflist = Diff.Compute(_file1, _file2, true, CancellationToken.None, true);
        }
        
        [Benchmark]
        public void CheckLines_True_SpeedOptimize_False()
        {
            var difflist = Diff.Compute(_file1, _file2, true, CancellationToken.None, false);
        }
        
        [Benchmark]
        public void CheckLines_False_SpeedOptimize_True()
        {
            var difflist = Diff.Compute(_file1, _file2, false, CancellationToken.None, true);
        }
        
        [Benchmark]
        public void CheckLines_False_SpeedOptimize_False()
        {
            var difflist = Diff.Compute(_file1, _file2, false, CancellationToken.None, false);
        }
    }
}