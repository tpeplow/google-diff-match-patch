using System;
using System.Collections.Generic;
using System.Linq;
using DiffMatchPatch;
using NUnit.Framework;

namespace DiffMatchPatchTests
{
    [TestFixture]
    public class ListTailSegmentTests
    {
        List<int> _completeList;

        [SetUp]
        public void Setup()
        {
            _completeList = new List<int>
            {
                0,
                1,
                2,
                3,
                4,
                5
            };
        }

        [Test]
        public void when_enumerating_segment()
        {
            CollectionAssert.AreEqual(_completeList.ToTailSegment(0).ToArray(), new[] { 0, 1, 2, 3, 4, 5 });
            CollectionAssert.AreEqual(_completeList.ToTailSegment(2).ToArray(), new[] { 2, 3, 4, 5 });
            CollectionAssert.AreEqual(_completeList.ToTailSegment(_completeList.Count - 1).ToArray(), new[] { 5 });
            CollectionAssert.AreEqual(_completeList.ToTailSegment(_completeList.Count).ToArray(), Array.Empty<int>());
        }

        [Test]
        public void when_reading_list()
        {
            Assert.That(_completeList.ToTailSegment(0)[0], Is.EqualTo(0));
            Assert.That(_completeList.ToTailSegment(0)[1], Is.EqualTo(1));
            Assert.That(_completeList.ToTailSegment(1)[0], Is.EqualTo(1));
            Assert.That(_completeList.ToTailSegment(2)[0], Is.EqualTo(2));
        }

        [Test]
        public void when_counting()
        {
            Assert.That(_completeList.ToTailSegment(0).Count, Is.EqualTo(6));
            Assert.That(_completeList.ToTailSegment(6).Count, Is.EqualTo(0));
            Assert.That(_completeList.ToTailSegment(2).Count, Is.EqualTo(4));
        }
        
        [Test]
        public void when_updating_list()
        {
            _completeList.ToTailSegment(2)[0] = -1;
            _completeList.ToTailSegment(2)[1] = -2;
            
            Assert.That(_completeList[2], Is.EqualTo(-1));
            Assert.That(_completeList[3], Is.EqualTo(-2));
        }

        [Test]
        public void when_adding()
        {
            _completeList.ToTailSegment(3).Add(10);
            Assert.That(_completeList[6], Is.EqualTo(10));
        }
        
        [Test]
        public void when_inserting()
        {
            _completeList.ToTailSegment(0).Insert(0,-1);
            _completeList.ToTailSegment(_completeList.Count).Insert(0,-2);
            _completeList.ToTailSegment(1).Insert(4, -3);
            
            CollectionAssert.AreEqual(_completeList, new[] { -1, 0, 1, 2, 3, -3, 4, 5, -2 });
        }

        [Test]
        public void when_removing()
        {
            Assert.That(_completeList.ToTailSegment(0).Remove(0), Is.True);
            Assert.That(_completeList.ToTailSegment(3).Remove(1), Is.False);
            Assert.That(_completeList.ToTailSegment(3).Remove(4), Is.True);
            CollectionAssert.AreEqual(_completeList, new[] { 1, 2, 3, 5 });
        }

        [Test]
        public void when_inserting_range()
        {
            _completeList.ToTailSegment(0).InsertRange(1, new[] { -1, -2});
            _completeList.ToTailSegment(_completeList.Count).InsertRange(0, new[] { -5, -6});
            _completeList.ToTailSegment(_completeList.Count - 4 ).InsertRange(1, new[] { -3, -4});

            CollectionAssert.AreEqual(_completeList, new[] { 0, -1, -2, 1, 2, 3, 4, -3, -4, 5, -5, -6 });
        }

        [Test]
        public void when_removing_range()
        {
            _completeList.ToTailSegment(0).RemoveRange(0, 1);
            _completeList.ToTailSegment(1).RemoveRange(1,2);

            CollectionAssert.AreEqual(_completeList, new[] { 1, 2, 5 });
        }
    }
}