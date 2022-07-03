using NUnit.Framework;
using System;
using System.Buffers;
using System.Linq;

namespace MemoryFrame.Test.Messy
{
    [TestFixture]
    public class ImageFactoryTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DisposeTest()
        {
            var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(11, 3);

            Assert.NotNull(ownedImageMemory.Image);

            ownedImageMemory.Dispose();

            Assert.Throws<ObjectDisposedException>(() => { var dummy = ownedImageMemory.Image; });
        }

        [Test]
        public void CreatePooled_Size()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(width: 11, height: 3, paddingPitch: 8);

            Assert.AreEqual(11, ownedImageMemory.Image.Width, "Image width.");
            Assert.AreEqual(3, ownedImageMemory.Image.Height, "Image height.");
        }

        [Test]
        public void OwnPooled_Size()
        {
            var memoryOwned = MemoryPool<float>.Shared.Rent(100);
            using var ownedImageMemory = ImageFactory.Padded.OwnPooled(memoryOwned, width: 11, height: 3, paddingPitch: 8);

            Assert.AreEqual(11, ownedImageMemory.Image.Width, "Image width.");
            Assert.AreEqual(3, ownedImageMemory.Image.Height, "Image height.");
        }

        [Test]
        public void OwnPooled_SizeMemory_TooSmall()
        {
            var memoryOwned = MemoryPool<float>.Shared.Rent(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                using var ownedImageMemory = ImageFactory.Padded.OwnPooled(memoryOwned, width: 11, height: 3, paddingPitch: 8);
            });
        }

        [Test]
        public void PixelCount()
        {
            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<float>(width: 11, height: 3, paddingPitch: 8);

            Assert.AreEqual(33, ownedImageMemory.Image.PixelValues.Count(), "Pixel count");
        }



    }
}
