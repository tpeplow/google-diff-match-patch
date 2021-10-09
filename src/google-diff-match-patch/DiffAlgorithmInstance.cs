using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DiffMatchPatch
{
    internal class DiffAlgorithmInstance
    {
        public List<Diff> Diffs { get; } = new List<Diff>();
        
        /// <summary>
        /// Find the differences between two texts.  Simplifies the problem by
        /// stripping any common prefix or suffix off the texts before diffing.
        /// </summary>
        /// <param name="text1">Old string to be diffed.</param>
        /// <param name="text2">New string to be diffed.</param>
        /// <param name="checklines">Speedup flag.  If false, then don't run a line-level diff first to identify the changed areas. If true, then run a faster slightly less optimal diff.</param>
        /// <param name="token">Cancellation token for cooperative cancellation</param>
        /// <param name="optimizeForSpeed">Should optimizations be enabled?</param>
        /// <returns></returns>
        public ListTailSegment<Diff> Compute(ReadOnlyMemory<char> text1, ReadOnlyMemory<char> text2, bool checklines, CancellationToken token, bool optimizeForSpeed)
        {
            var diffs = Diffs.ToTailSegment();
            if (text1.Length == text2.Length && text1.Length == 0)
                return diffs;

            var commonlength = TextUtil.CommonPrefix(text1.Span, text2.Span);

            if (commonlength == text1.Length && commonlength == text2.Length)
            {
                // equal
                diffs.Add(Diff.Equal(text1));
                return diffs;
            }

            // Trim off common prefix (speedup).
            var commonprefix = text1.Slice(0, commonlength);
            text1 = text1.Slice(commonlength);
            text2 = text2.Slice(commonlength);

            // Trim off common suffix (speedup).
            commonlength = TextUtil.CommonSuffix(text1.Span, text2.Span);
            var commonsuffix = text1.Slice(text1.Length - commonlength);
            text1 = text1.Slice(0, text1.Length - commonlength);
            text2 = text2.Slice(0, text2.Length - commonlength);

            // Compute the diff on the middle block.
            diffs = ComputeImpl(text1, text2, checklines, token, optimizeForSpeed);

            // Restore the prefix and suffix.
            if (commonprefix.Length != 0)
            {
                diffs.Insert(0, Diff.Equal(commonprefix));
            }
            if (commonsuffix.Length != 0)
            {
                diffs.Add(Diff.Equal(commonsuffix));
            }

            diffs.CleanupMerge();
            return diffs;
        }

        /// <summary>
        /// Find the differences between two texts.  Assumes that the texts do not
        /// have any common prefix or suffix.
        /// </summary>
        /// <param name="text1">Old string to be diffed.</param>
        /// <param name="text2">New string to be diffed.</param>
        /// <param name="checklines">Speedup flag.  If false, then don't run a line-level diff first to identify the changed areas. If true, then run a faster slightly less optimal diff.</param>
        /// <param name="token">Cancellation token for cooperative cancellation</param>
        /// <param name="optimizeForSpeed">Should optimizations be enabled?</param>
        /// <returns></returns>
        private ListTailSegment<Diff> ComputeImpl(
            ReadOnlyMemory<char> text1,
            ReadOnlyMemory<char> text2,
            bool checklines, CancellationToken token, bool optimizeForSpeed)
        {
            var diffs = Diffs.ToTailSegment();

            if (text1.Length == 0)
            {
                // Just add some text (speedup).
                diffs.Add(Diff.Insert(text2));
                return diffs;
            }

            if (text2.Length == 0)
            {
                // Just delete some text (speedup).
                diffs.Add(Diff.Delete(text1));
                return diffs;
            }

            var longtext = text1.Length > text2.Length ? text1 : text2;
            var shorttext = text1.Length > text2.Length ? text2 : text1;
            var i = longtext.Span.IndexOf(shorttext.Span, StringComparison.Ordinal);
            if (i != -1)
            {
                // Shorter text is inside the longer text (speedup).
                var op = text1.Length > text2.Length ? Operation.Delete : Operation.Insert;
                diffs.Add(Diff.Create(op, longtext.Slice(0, i)));
                diffs.Add(Diff.Equal(shorttext));
                diffs.Add(Diff.Create(op, longtext.Slice(i + shorttext.Length)));
                return diffs;
            }

            if (shorttext.Length == 1)
            {
                // Single character string.
                // After the previous speedup, the character can't be an equality.
                diffs.Add(Diff.Delete(text1));
                diffs.Add(Diff.Insert(text2));
                return diffs;
            }

            // Don't risk returning a non-optimal diff if we have unlimited time.
            if (optimizeForSpeed)
            {
                // Check to see if the problem can be split in two.
                var result = TextUtil.HalfMatch(text1, text2);
                if (!result.IsEmpty)
                {
                    // A half-match was found, sort out the return data.
                    // Send both pairs off for separate processing.
                    diffs = Compute(result.Prefix1, result.Prefix2, checklines, token, optimizeForSpeed);
                    diffs.Add(Diff.Equal(result.CommonMiddle));
                    Compute(result.Suffix1, result.Suffix2, checklines, token, optimizeForSpeed);

                    return diffs;
                }
            }
            if (checklines && text1.Length > 100 && text2.Length > 100)
            {
                return LineDiff(text1.ToString(), text2.ToString(), token, optimizeForSpeed);
            }

            return MyersDiffBisect(text1, text2, token, optimizeForSpeed);
        }

        /// <summary>
        /// Do a quick line-level diff on both strings, then rediff the parts for
        /// greater accuracy. This speedup can produce non-minimal Diffs.
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <param name="token"></param>
        /// <param name="optimizeForSpeed"></param>
        /// <returns></returns>
        private ListTailSegment<Diff> LineDiff(string text1, string text2, CancellationToken token, bool optimizeForSpeed)
        {
            // Scan the text on a line-by-line basis first.
            var compressor = new LineToCharCompressor();
            text1 = compressor.Compress(text1, char.MaxValue * 2 / 3);
            text2 = compressor.Compress(text2, char.MaxValue);
            
            var diffs = Compute(text1.AsMemory(), text2.AsMemory(), false, token, optimizeForSpeed);
            for (var i = 0; i < diffs.Count; i++)
            {
                var diff = diffs[i];
                diffs[i] = diff.Replace(compressor.Decompress(diff.Text));
            }

            // Eliminate freak matches (e.g. blank lines)
            diffs.CleanupSemantic();

            // Rediff any replacement blocks, this time character-by-character.
            // Add a dummy entry at the end.
            diffs.Add(Diff.Equal(string.Empty));
            var pointer = 0;
            var countDelete = 0;
            var countInsert = 0;
            var insertBuilder = new StringBuilder();
            var deleteBuilder = new StringBuilder();
            while (pointer < diffs.Count)
            {
                switch (diffs[pointer].Operation)
                {
                    case Operation.Insert:
                        countInsert++;
                        insertBuilder.Append(diffs[pointer].Text);
                        break;

                    case Operation.Delete:
                        countDelete++;
                        deleteBuilder.Append(diffs[pointer].Text);
                        break;

                    case Operation.Equal:
                        // Upon reaching an equality, check for prior redundancies.
                        if (countDelete >= 1 && countInsert >= 1)
                        {
                            // todo
                            
                            // Delete the offending records and add the merged ones.
                            var diffsWithinLine = new DiffAlgorithmInstance().Compute(deleteBuilder.ToString().AsMemory(), insertBuilder.ToString().AsMemory(), false, token, optimizeForSpeed);
                            var count = countDelete + countInsert;
                            var index = pointer - count;
                            diffs.Splice(index, count, diffsWithinLine);
                            pointer = index + diffsWithinLine.Count;
                        }
                        countInsert = 0;
                        countDelete = 0;
                        deleteBuilder.Clear();
                        insertBuilder.Clear();
                        break;
                }
                pointer++;
            }
            diffs.RemoveAt(diffs.Count - 1);  // Remove the dummy entry at the end.

            return diffs;
        }

        /// <summary>
        /// Find the 'middle snake' of a diff, split the problem in two
        /// and return the recursively constructed diff.
        /// See Myers 1986 paper: An O(ND) Difference Algorithm and Its Variations.
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <param name="token"></param>
        /// <param name="optimizeForSpeed"></param>
        /// <returns></returns>
        internal ListTailSegment<Diff> MyersDiffBisect(ReadOnlyMemory<char> text1, ReadOnlyMemory<char> text2, CancellationToken token, bool optimizeForSpeed)
        {
            var text1Span = text1.Span;
            var text2Span = text2.Span;
            // Cache the text lengths to prevent multiple calls.
            var text1Length = text1.Length;
            var text2Length = text2.Length;
            var maxD = (text1Length + text2Length + 1) / 2;
            var vOffset = maxD;
            var vLength = 2 * maxD;
            var v1 = ArrayPool<int>.Shared.Rent(vLength);
            var v2 = ArrayPool<int>.Shared.Rent(vLength);

            void ReturnRentedArrays()
            {
                ArrayPool<int>.Shared.Return(v1);
                ArrayPool<int>.Shared.Return(v2);
            }
            
            for (var x = 0; x < vLength; x++)
            {
                v1[x] = -1;
            }
            for (var x = 0; x < vLength; x++)
            {
                v2[x] = -1;
            }
            v1[vOffset + 1] = 0;
            v2[vOffset + 1] = 0;
            var delta = text1Length - text2Length;
            // If the total number of characters is odd, then the front path will
            // collide with the reverse path.
            var front = delta % 2 != 0;
            // Offsets for start and end of k loop.
            // Prevents mapping of space beyond the grid.
            var k1Start = 0;
            var k1End = 0;
            var k2Start = 0;
            var k2End = 0;
            for (var d = 0; d < maxD; d++)
            {
                // Bail out if cancelled.
                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Walk the front path one step.
                for (var k1 = -d + k1Start; k1 <= d - k1End; k1 += 2)
                {
                    var k1Offset = vOffset + k1;
                    int x1;
                    if (k1 == -d || k1 != d && v1[k1Offset - 1] < v1[k1Offset + 1])
                    {
                        x1 = v1[k1Offset + 1];
                    }
                    else
                    {
                        x1 = v1[k1Offset - 1] + 1;
                    }
                    var y1 = x1 - k1;
                    while (x1 < text1Length && y1 < text2Length
                                            && text1Span[x1] == text2Span[y1])
                    {
                        x1++;
                        y1++;
                    }
                    v1[k1Offset] = x1;
                    if (x1 > text1Length)
                    {
                        // Ran off the right of the graph.
                        k1End += 2;
                    }
                    else if (y1 > text2Length)
                    {
                        // Ran off the bottom of the graph.
                        k1Start += 2;
                    }
                    else if (front)
                    {
                        var k2Offset = vOffset + delta - k1;
                        if (k2Offset >= 0 && k2Offset < vLength && v2[k2Offset] != -1)
                        {
                            // Mirror x2 onto top-left coordinate system.
                            var x2 = text1Length - v2[k2Offset];
                            if (x1 >= x2)
                            {
                                ReturnRentedArrays();
                                // Overlap detected.
                                return BisectSplit(text1, text2, x1, y1, token, optimizeForSpeed);
                            }
                        }
                    }
                }

                // Walk the reverse path one step.
                for (var k2 = -d + k2Start; k2 <= d - k2End; k2 += 2)
                {
                    var k2Offset = vOffset + k2;
                    int x2;
                    if (k2 == -d || k2 != d && v2[k2Offset - 1] < v2[k2Offset + 1])
                    {
                        x2 = v2[k2Offset + 1];
                    }
                    else
                    {
                        x2 = v2[k2Offset - 1] + 1;
                    }
                    var y2 = x2 - k2;
                    while (x2 < text1Length && y2 < text2Length
                                            && text1Span[text1Length - x2 - 1]
                                            == text2Span[text2Length - y2 - 1])
                    {
                        x2++;
                        y2++;
                    }
                    v2[k2Offset] = x2;
                    if (x2 > text1Length)
                    {
                        // Ran off the left of the graph.
                        k2End += 2;
                    }
                    else if (y2 > text2Length)
                    {
                        // Ran off the top of the graph.
                        k2Start += 2;
                    }
                    else if (!front)
                    {
                        var k1Offset = vOffset + delta - k2;
                        if (k1Offset >= 0 && k1Offset < vLength && v1[k1Offset] != -1)
                        {
                            var x1 = v1[k1Offset];
                            var y1 = vOffset + x1 - k1Offset;
                            // Mirror x2 onto top-left coordinate system.
                            x2 = text1Length - v2[k2Offset];
                            if (x1 >= x2)
                            {
                                ReturnRentedArrays();
                                // Overlap detected.
                                return BisectSplit(text1, text2, x1, y1, token, optimizeForSpeed);
                            }
                        }
                    }
                }
            }

            ReturnRentedArrays();
            // Diff took too long and hit the deadline or
            // number of Diffs equals number of characters, no commonality at all.
            var diffs = Diffs.ToTailSegment();
            diffs.Add(Diff.Delete(text1));
            diffs.Add(Diff.Insert(text2));
            return diffs;
        }

        /// <summary>
        /// Given the location of the 'middle snake', split the diff in two parts
        /// and recurse.
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <param name="x">Index of split point in text1.</param>
        /// <param name="y">Index of split point in text2.</param>
        /// <param name="token"></param>
        /// <param name="optimizeForSpeed"></param>
        /// <returns></returns>
        private ListTailSegment<Diff> BisectSplit(ReadOnlyMemory<char> text1, ReadOnlyMemory<char> text2, int x, int y, CancellationToken token, bool optimizeForSpeed)
        {
            var text1A = text1.Slice(0, x);
            var text2A = text2.Slice(0, y);
            var text1B = text1.Slice(x);
            var text2B = text2.Slice(y);

            // Compute both Diffs serially.
            var diffs = Compute(text1A, text2A, false, token, optimizeForSpeed);
            Compute(text1B, text2B, false, token, optimizeForSpeed);

            return diffs;
        }
    }
}