using System;
using DiffMatchPatch;

namespace DiffMatchPatchTests
{
    internal static class TextUtilWrapper
    {
        public static HalfMatchResult HalfMatch(string text1, string text2)
        {
            var result = TextUtil.HalfMatch(text1.AsMemory(), text2.AsMemory());
            return result;
        }

        public static HalfMatchResult CreateHalfMatchResult(string prefix1, string suffix1, string prefix2, string suffix2, string commonMiddle)
        {
            return new HalfMatchResult(
                prefix1.AsMemory(),
                suffix1.AsMemory(),
                prefix2.AsMemory(),
                suffix2.AsMemory(),
                commonMiddle.AsMemory());
        }
    }
}