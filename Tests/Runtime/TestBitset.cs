using NUnit.Framework;

namespace Grid {
    [TestFixture]
    public class TestBitset {

        [Test]
        public void TestSetProperty() {
            uint bs = 0;
            Bitset.Set(ref bs, 4, true);
            Bitset.Set(ref bs, 6, false);
            Bitset.Set(ref bs, 4, false);
            Bitset.Set(ref bs, 2, true);
            Bitset.Set(ref bs, 1, true);
            Bitset.Set(ref bs, 0, true);
            Bitset.Set(ref bs, 1, false);
            
            Assert.AreEqual(true, Bitset.Get(bs, 0));
            Assert.AreEqual(false, Bitset.Get(bs, 1));
            Assert.AreEqual(true, Bitset.Get(bs, 2));
            Assert.AreEqual(false, Bitset.Get(bs, 3));
            Assert.AreEqual(false, Bitset.Get(bs, 4));
            Assert.AreEqual(false, Bitset.Get(bs, 5));
            Assert.AreEqual(false, Bitset.Get(bs, 6));
            Assert.AreEqual(false, Bitset.Get(bs, 7));
        }
    }
}