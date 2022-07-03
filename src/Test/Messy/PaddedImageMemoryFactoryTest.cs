using NUnit.Framework;
using System.Numerics;

namespace MemoryFrame.Test.Messy
{
    [TestFixture]
    public class PaddedImageMemoryFactoryTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CreatePooled_RowPitch_PaddingPitch_NonDefault()
        {
            int customPaddingPitch = Vector<double>.Count * 2;
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width: 1, height: 1, paddingPitch: customPaddingPitch);

            Assert.AreEqual(customPaddingPitch, ownedImageMemory.Image.RowPitch, "Vector size is custom provided value");
        }

        [Test]
        public void CreatePooled_RowPitch_PaddingPitch_8()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(width: 11, height: 3 , paddingPitch: 8);

            Assert.AreEqual(16, ownedImageMemory.Image.RowPitch, "Vector size aligned row pitch");
        }

        [Test]
        public void CreatePooled_RowPitch_PaddingPitch_1()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(width: 11, height: 3, paddingPitch: 1);

            Assert.AreEqual(11, ownedImageMemory.Image.RowPitch, "Vector size of 1");
        }

        [Test]
        public void CreatePooled_RowPitch_Default_Equals_PaddingPitch()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(width: 1, height: 1);

            Assert.AreEqual(Vector<float>.Count, ownedImageMemory.Image.RowPitch, "Vector size must used as default for the row pitch.");
        }

        [Test]
        public void CreateTransient_RowPadding_PaddingPitch_8()
        {
            var image2D = ImageFactory.Padded.CreateTransient<float>(width: 11, height: 3, paddingPitch: 8);

            Assert.AreEqual(16, image2D.PaddedRows[0].Length, "Vector size aligned padded row");
        }

        [Test]
        public void CreateTransient_RowPadding_PaddingPitch_1()
        {
            var image2D = ImageFactory.Padded.CreateTransient<float>(width: 11, height: 3, paddingPitch: 1);

            Assert.AreEqual(11, image2D.PaddedRows[0].Length, "Vector size of 1");
        }

        [Test]
        public void CreateTransient_RowPadding_Default_Equals_PaddingPitch()
        {
            var image2D = ImageFactory.Padded.CreateTransient<float>(width: 1, height: 1);

            Assert.AreEqual(Vector<float>.Count, image2D.PaddedRows[0].Length, "Vector size must used as default for the row pitch.");
        }

        [Test]
        public void CreatePooledCopy_RowPitch_Vector_Padded()
        {
            int floatVectorCount = Vector<float>.Count;
            int paddingPitch = floatVectorCount * 4;
            int width = paddingPitch + 1;
            int height = 3;
            int rowPitch_ref = Padding.GetRowPitch(width, floatVectorCount);
            var image2D = ImageFactory.Padded.CreateTransient<float>(width: width, height: height, paddingPitch: paddingPitch);
            using var ownedImageMemory = ImageFactory.Padded.CreatePooledCopy(image2D);

            int rowPitch = ownedImageMemory.Image.RowPitch;
            Assert.AreEqual(rowPitch_ref, rowPitch, "RowPitch");
        }

        [Test]
        public void CreatePooledCopy_CopyMemory_RowPitch()
        {
            var image2D = ImageFactory.Padded.CreateTransient<double>(width: 11, height: 3, paddingPitch: 8);
            image2D[1][0] = 2.1;
            image2D[2][10] = 3.11;
            using var ownedImageMemory = ImageFactory.Padded.CreatePooledCopy(image2D);

            int rowPitch = ownedImageMemory.Image.RowPitch;
            Assert.AreEqual(2.1, ownedImageMemory.Image.Memory.Span[rowPitch], "RowPitch align");
            Assert.AreEqual(3.11, ownedImageMemory.Image.Memory.Span[rowPitch * 2 + 10], "RowPitch align");
        }
    }
}