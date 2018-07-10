using NUnit.Framework;

namespace NUnitSample
{

    [TestFixture]
    public class TestFixture1
    {

        [Test]
        public void TestOneEquals1_Success()
        {
            Assert.That(1, Is.EqualTo(1));
        }
        [Test]
        public void TestTwoEquals2_Success()
        {
            Assert.That(2, Is.EqualTo(2));
        }
    }
}
