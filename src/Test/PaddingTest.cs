using System;
using NUnit.Framework;

namespace MemoryFrame.Test
{
    [TestFixture]
    public class PaddingTest
    {
        [Test]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, false)]
        [TestCase(4, true)]
        [TestCase(5, false)]
        [TestCase(6, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        [TestCase(10, false)]
        [TestCase(11, false)]
        [TestCase(12, false)]
        [TestCase(13, false)]
        [TestCase(14, false)]
        [TestCase(15, false)]
        [TestCase(16, true)]
        [TestCase(17, false)]
        [TestCase(256, true)]
        [TestCase(257, false)]
        public void IsPowerOfTwo(int n, bool expectedResult)
        {
            bool result = Padding.IsPowerOfTwo(n);
            Assert.AreEqual(expectedResult, result, "IsPowerOfTwo");
        }

        [Test]
        public void GetRowPitch_InputValidation_Throws_ArgumentException()
        {
            int nonValidPaddingPitch = (0x10) - 1;

            Assert.Throws<ArgumentException>(() => Padding.GetRowPitch(10, nonValidPaddingPitch));
        }

        [Test]
        [TestCase(0, 0x01, 0)]
        [TestCase(1, 0x01, 1)]
        [TestCase(2, 0x01, 2)]

        [TestCase(0, 0x02, 0)]
        [TestCase(1, 0x02, 2)]
        [TestCase(2, 0x02, 2)]
        [TestCase(3, 0x02, 4)]

        [TestCase(0, 0x04, 0)]
        [TestCase(1, 0x04, 4)]
        [TestCase(2, 0x04, 4)]
        [TestCase(3, 0x04, 4)]
        [TestCase(4, 0x04, 4)]
        [TestCase(5, 0x04, 8)]
        [TestCase(6, 0x04, 8)]
        [TestCase(7, 0x04, 8)]
        [TestCase(8, 0x04, 8)]
        [TestCase(9, 0x04, 12)]

        [TestCase(0, 0x10, 0)]
        [TestCase(1, 0x10, 16)]
        [TestCase(8, 0x10, 16)]
        [TestCase(15, 0x10, 16)]
        [TestCase(16, 0x10, 16)]
        [TestCase(17, 0x10, 32)]
        [TestCase(31, 0x10, 32)]
        [TestCase(32, 0x10, 32)]
        [TestCase(33, 0x10, 48)]
        public void GetRowPitch_InputValidation(int width, int paddingPitch, int expectedRowPitch)
        {
            int result = Padding.GetRowPitch(width, paddingPitch);
            Assert.AreEqual(expectedRowPitch, result, "GetRowPitch");
        }
    }
}
