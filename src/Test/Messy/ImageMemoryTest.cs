using NUnit.Framework;

namespace MemoryFrame.Test.Messy
{
    [TestFixture]
    public class ImageMemoryTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AsReadOnlyImage2D_Memory_Size()
        {
            int width = 11;
            int height = 3;
            int paddingPitch = 8;
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width, height, paddingPitch);

            var image = ownedImageMemory.Image;
            Assert.AreEqual(image.RowPitch * height, image.Memory.Length, "Memory.Length");
        }

        [Test]
        public void AsImage2D_ShareMemory_RowPitch()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width: 11, height: 3, paddingPitch: 8, clearMemory: true);

            IImageMemory<double> image = ownedImageMemory.Image;
            var image2D = image.AsWritableImage2D();

            image.Memory.Span[0] = 1.1;
            image.Memory.Span[image.RowPitch] = 2.1;

            Assert.AreEqual(1.1, image2D[0][0], "Pixel count");
            Assert.AreEqual(2.1, image2D[1][0], "Pixel count");
        }

        [Test]
        public void AsReadOnlyImage2D_ShareMemory_RowPitch()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width: 11, height: 3, paddingPitch: 8, clearMemory: true);

            IImageMemory<double> image = ownedImageMemory.Image;
            var image2D = image.AsReadOnlyImage2D();

            image.Memory.Span[0] = 1.1;
            image.Memory.Span[image.RowPitch] = 2.1;

            Assert.AreEqual(1.1, image2D[0][0], "Pixel count");
            Assert.AreEqual(2.1, image2D[1][0], "Pixel count");
        }

        [Test]
        public void CompactSlice_CompactSlice()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width: 30, height: 10, paddingPitch: 8, clearMemory: true);

            IImageMemory<double> image = ownedImageMemory.Image;
            
            Assert.AreEqual(32, image.RowPitch, "RowPitch");

            var image2D_30_10 = image.AsWritableImage2D();
            image2D_30_10[0][0] = 1.1;
            image2D_30_10[0][3] = 1.4;
            image2D_30_10[2][3] = 3.4;      // Top left pixel after slicing
            image2D_30_10[5][13] = 6.15;    // Bottom right pixel after slicing

            // ===
            image.CompactSlice(3, 2, 11, 4);
            // ===

            var image2D_11_4 = image.AsWritableImage2D();

            Assert.AreEqual(16, image.RowPitch, "RowPitch");
            Assert.AreEqual(3.4, image2D_11_4[0][0], "First pixel");
            Assert.AreEqual(6.15, image2D_11_4[3][10], "Last pixel");

            Assert.AreEqual(6.15, image2D_30_10[5][13], "Original pixel location");
            Assert.AreEqual(6.15, image2D_30_10[1][26], "New pixel location - copy");
        }

        [Test]
        public void CompactSlice_CompactCopy()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<double>(width: 30, height: 10, paddingPitch: 8, clearMemory: true);

            IImageMemory<double> image = ownedImageMemory.Image;

            Assert.AreEqual(32, image.RowPitch, "RowPitch");

            var image2D_30_10 = image.AsWritableImage2D();
            image2D_30_10[0][0] = 1.1;
            image2D_30_10[0][3] = 1.4;
            image2D_30_10[2][3] = 3.4;      // Top left pixel after slicing
            image2D_30_10[5][13] = 6.15;    // Bottom right pixel after slicing

            var image2D_11_4_slice = image2D_30_10.Slice(3, 2, 11, 4);

            // ===
            image.CompactCopy(image2D_11_4_slice);
            // ===

            var image2D_11_4 = image.AsWritableImage2D();

            Assert.AreEqual(16, image.RowPitch, "RowPitch");
            Assert.AreEqual(3.4, image2D_11_4[0][0], "First pixel");
            Assert.AreEqual(6.15, image2D_11_4[3][10], "Last pixel");

            Assert.AreEqual(6.15, image2D_30_10[5][13], "Original pixel location");
            Assert.AreEqual(6.15, image2D_30_10[1][26], "New pixel location - copy");
        }
    }
}
