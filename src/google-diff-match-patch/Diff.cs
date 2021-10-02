using System;
using System.Collections.Generic;
using System.Threading;

namespace DiffMatchPatch
{
    public struct Diff
    {
        internal static Diff Create(Operation operation, string text) => new Diff(operation, text);
        internal static Diff Create(Operation operation, ReadOnlyMemory<char> text) => new Diff(operation, text);

        internal static Diff Equal(string text) => Create(Operation.Equal, text);
        internal static Diff Equal(ReadOnlyMemory<char> text) => Create(Operation.Equal, text);

        internal static Diff Insert(string text) => Create(Operation.Insert, text);
        internal static Diff Insert(ReadOnlyMemory<char> text) => Create(Operation.Insert, text);

        internal static Diff Delete(string text) => Create(Operation.Delete, text);
        internal static Diff Delete(ReadOnlyMemory<char> text) => Create(Operation.Delete, text);

        // One of: INSERT, DELETE or EQUAL.
        public Operation Operation { get; }

        public string Text => StringText ?? ReadonlyMemoryText.ToString();
        
        // The text associated with this diff operation.
        public string StringText { get; }
        
        public ReadOnlyMemory<char> ReadonlyMemoryText { get; }

        public string FormattedText => Text.Replace("\r\n", "\n").Replace("\n", "\u00b6\n").Replace("\t", "\u00BB").Replace(" ", "\u00B7");

        public bool WhitespaceOnlyDiff => string.IsNullOrWhiteSpace(Text);

        private Diff(Operation operation, string text)
        {
            // Construct a diff with the specified operation and text.
            Operation = operation;
            StringText = text;
            ReadonlyMemoryText = null;
        }

        private Diff(Operation operation, ReadOnlyMemory<char> readonlyMemoryText)
        {
            Operation = operation;
            StringText = null;
            ReadonlyMemoryText = readonlyMemoryText;
        }

        /// <summary>
        /// Generate a human-readable version of this Diff.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Diff(" + Operation + ",\"" + FormattedText.Replace("\n", "") + "\")\n";
        }

        /// <summary>
        /// Is this Diff equivalent to another Diff?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => !ReferenceEquals(obj, null) && Equals((Diff)obj);

        public bool Equals(Diff obj) => obj.Operation == Operation && obj.Text == Text;

        public static bool operator ==(Diff left, Diff right) => left.Equals(right);

        public static bool operator !=(Diff left, Diff right) => !(left == right);

        public override int GetHashCode() => Text.GetHashCode() ^ Operation.GetHashCode();

        internal Diff Replace(string toString) => Create(Operation, toString);

        internal Diff Copy() => Create(Operation, Text);

        /// <summary>
        /// Find the differences between two texts.
        /// </summary>
        /// <param name="text1">Old string to be diffed</param>
        /// <param name="text2">New string to be diffed</param>
        /// <param name="timeoutInSeconds">if specified, certain optimizations may be enabled to meet the time constraint, possibly resulting in a less optimal diff</param>
        /// <param name="checklines">If false, then don't run a line-level diff first to identify the changed areas. If true, then run a faster slightly less optimal diff.</param>
        /// <returns></returns>
        public static List<Diff> Compute(string text1, string text2, float timeoutInSeconds = 0f, bool checklines = true)
        {
            using (var cts = timeoutInSeconds <= 0
                ? new CancellationTokenSource()
                : new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds))
                )
            {
                return Compute(text1, text2, checklines, cts.Token, timeoutInSeconds > 0);
            }
        }

        public static List<Diff> Compute(string text1, string text2, bool checkLines, CancellationToken token, bool optimizeForSpeed)
            => DiffAlgorithm.Compute(text1.AsMemory(), text2.AsMemory(), checkLines, token, optimizeForSpeed);
    }
}
