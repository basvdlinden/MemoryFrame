using MemoryFrame.Test.UnmanagedMemory;
using NUnit.Framework;

namespace MemoryFrame.Test
{
    [TestFixture]
    class UnmanagedMemoryTest
    {
        [Test]
        public void OwnPooled_UnmanagedMemory()
        {
            int width = 1000;
            int height = 400;
            short markerVal = 10;

            int paddedBufferSize = ImageFactory.Padded.GetBufferSize<short>(width, height);
            var buffer = new UnmanagedBuffer<short>(paddedBufferSize);

            using (var ownedMemory = ImageFactory.Padded.OwnPooled(buffer, width, height, clearMemory: true))
            {
                // Change underlaying unmanaged memory by setting one pixel equal to a marker value.
                var image = ownedMemory.Image.AsWritableImage2D();
                image[10][20] = markerVal; 

                var slicedImage = image.Slice(20, 10, 4, 4);
                Assert.AreEqual(markerVal, slicedImage[0][0], "Access unchanged underlaying unmanaged memory via sliced image.");

                image = slicedImage = null; // Instances will become invalid after CompactSlice

                // Slice and compact image, so pixel values are copied to beginning of buffer.
                ownedMemory.Image.CompactSlice(20, 10, 4, 4);

                Assert.AreEqual(markerVal, ownedMemory.Image.Memory.Span[0], "Transparent access to underlaying unmanaged memory.");

                Assert.AreEqual(markerVal, buffer.GetValue(0), "Unmanaged memory has marker value on index = 0.");
                Assert.AreEqual(0, buffer.GetValue(1), "Unmanaged memory has cleared value on index = 1.");
                
                Assert.IsFalse(buffer.IsDisposed, "Disposal of owned memory");
            }
            Assert.IsTrue(buffer.IsDisposed, "Disposal of owned memory");

        }
    }

}
