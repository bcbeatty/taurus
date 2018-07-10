using NUnit.Framework;
using System;


namespace NUnitRunner
{
    [TestFixture]
    public class NUnitTestClass
    {
        [Test]
        public void TestCase()
        {
            NUnitRunner.Main(new [] { "--iterations", "1", "--target", "../../../NunitSample/bin/Debug/NUnitSample.dll" });//"--filter", "<filter><cat>Reserve</cat></filter>"
        }
    }
}