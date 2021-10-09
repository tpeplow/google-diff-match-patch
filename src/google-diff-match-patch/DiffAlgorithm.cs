using System;
using System.Collections.Generic;
using System.Threading;

namespace DiffMatchPatch
{
    internal static class DiffAlgorithm
    {
        public static List<Diff> Compute(ReadOnlyMemory<char> text1, ReadOnlyMemory<char> text2, bool checklines, CancellationToken token, bool optimizeForSpeed)
        {
            var diffAlgorithmInstance = new DiffAlgorithmInstance();
            diffAlgorithmInstance.Compute(text1, text2, checklines, token, optimizeForSpeed);
            return diffAlgorithmInstance.Diffs;
        }

        public static List<Diff> MyersDiffBisect(ReadOnlyMemory<char> text1, ReadOnlyMemory<char> text2, CancellationToken token, bool optimizeForSpeed)
        {
            var diffAlgorithmInstance = new DiffAlgorithmInstance();
            diffAlgorithmInstance.MyersDiffBisect(text1, text2, token, optimizeForSpeed);
            return diffAlgorithmInstance.Diffs;
        }
    }
}
