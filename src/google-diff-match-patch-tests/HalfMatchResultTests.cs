using NUnit.Framework;

namespace DiffMatchPatchTests
{
    [TestFixture]
    public class HalfMatchResultTests
    {
        [Test]
        public void WhenAreEqual_Equals()
        {
            // do the substring to make sure we don't accidentally point to the same memory
            var result = TextUtilWrapper.CreateHalfMatchResult("121234".Substring(0,5), "123121", "a", "z", "1234123451234");
            var result2 = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "1234123451234");

            Assert.AreEqual(result, result2);
        }

        [Test]
        public void WhenAreEqual_HashCode()
        {
            var result = TextUtilWrapper.CreateHalfMatchResult("121234".Substring(0,5), "123121", "a", "z", "1234123451234");
            var result2 = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "1234123451234");

            Assert.AreEqual(result.GetHashCode(), result2.GetHashCode());
        }

        [Test]
        public void WhenAreNotEqual_Equals()
        {
            var result = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "1234123451234");
            var result2 = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "a");

            Assert.AreNotEqual(result, result2);
        }
        
        [Test]
        public void WhenAreNotEqual_GetHashCode()
        {
            var result = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "1234123451234");
            var result2 = TextUtilWrapper.CreateHalfMatchResult("12123", "123121", "a", "z", "a");

            Assert.AreNotEqual(result.GetHashCode(), result2.GetHashCode());
        }
    }
}