using NUnit.Framework;
using System;
using System.Linq;
using System.Numerics;

namespace MemoryFrame.Test
{
    [TestFixture]
    class SIMDTest
    {
        [Test]
        public void __Log_Process_Capbilities()
        {
            string processType = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            Console.WriteLine($"{processType} Process");
            Console.WriteLine($".NET Version: {Environment.Version}");
            Console.WriteLine($"Machine name: {Environment.MachineName}");
            Console.WriteLine($"Vector<byte>.Count:   \t{Vector<byte>.Count}");
            Console.WriteLine($"Vector<short>.Count:  \t{Vector<short>.Count}");
            Console.WriteLine($"Vector<int>.Count:    \t{Vector<int>.Count}");
            Console.WriteLine($"Vector<float>.Count:  \t{Vector<float>.Count}");
            Console.WriteLine($"Vector<double>.Count: \t{Vector<double>.Count}");
        }

        [Test]
        public void ImageMemory_AddConst_SingleLoop()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 2 + 1, height: 3);
            var image = ownedImageMemory.Image;
            image.Memory.Span.Fill(11);
            var addValue = new Vector<short>(22);

            // Single SIMD loop
            // ====
            var shortVector = image.Memory.Span.AsVector();
            for (int i = 0; i < shortVector.Length; i++)
            {
                shortVector[i] = shortVector[i] + addValue;
            }
            // ====

            Assert.IsTrue(image.PixelValues.All(p => p == 33), "All pixels have new value");
        }

        [Test]
        public void ImageMemory_AddImage_SingleLoop()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory1 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 2 + 1, height: 3);
            using var ownedImageMemory2 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 2 + 1, height: 3);
            var span1 = ownedImageMemory1.Image.Memory.Span;
            var span2 = ownedImageMemory2.Image.Memory.Span;

            span1.Fill(11);
            span2.Fill(22);
                 
            span1[0] = 1;
            span2[0] = 32;
            span1[vectorCount] = 1;
            span2[vectorCount] = 32;

            // Single SIMD loop
            // ====
            var shortVector1 = span1.AsVector();
            var shortVector2 = span2.AsVector();
            for (int i = 0; i < shortVector1.Length; i++)
            {
                shortVector1[i] = shortVector1[i] + shortVector2[i];
            }
            // ====

            Assert.IsTrue(ownedImageMemory1.Image.PixelValues.All(p => p == 33), "All pixels have new value");
        }

        [Test]
        public void Image2D_AddConst_LoopPerRow()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 2 + 1, height: 3);
            ownedImageMemory.Image.Memory.Span.Fill(11);
            var image = ownedImageMemory.Image.AsWritableImage2D();
            var addValue = new Vector<short>(22);

            // SIMD loop per row
            // ====
            foreach (var row in image.PaddedRows)
            {
                var shortVector = row.Span.AsVector();
                for (int i = 0; i < shortVector.Length; i++)
                {
                    shortVector[i] = shortVector[i] + addValue;
                }
            }
            // ====

            Assert.IsTrue(image.PixelValues.All(p => p == 33), "All pixels have new value");
        }

        [Test]
        public void Image2D_AddImage_Sliced_LoopPerRow()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }
            int width = vectorCount * 2 + 1;
            int height = 3;
            using var ownedImageMemory1 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 4 + 1, height: 10);
            using var ownedImageMemory2 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount * 4 + 1, height: 10);
            ownedImageMemory1.Image.Memory.Span.Fill(11);
            ownedImageMemory2.Image.Memory.Span.Fill(22);
            var image1 = ownedImageMemory1.Image.AsWritableImage2D().Slice(3, 1, width, height);
            var image2 = ownedImageMemory2.Image.AsWritableImage2D().Slice(5, 3, width, height);

            // SIMD loop per row
            // ====
            for (int rowIndex = 0; rowIndex < image1.PaddedRows.Count; rowIndex++)
            {
                var shortVectorImage1 = image1.PaddedRows[rowIndex].Span.AsVector();
                var shortVectorImage2 = image2.PaddedRows[rowIndex].Span.AsVector();
                for (int i = 0; i < shortVectorImage1.Length; i++)
                {
                    shortVectorImage1[i] = shortVectorImage1[i] + shortVectorImage2[i];
                }
            }
            // ====

            Assert.IsTrue(image1.PixelValues.All(p => p == 33), "All pixels have new value");
        }


        [Test]
        public void ImageMemory_Proof_WrongPadding_Pixels_Skipped()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<short>(width: vectorCount + 1, height: 3, paddingPitch: 1);
            ownedImageMemory.Image.Memory.Span.Fill(111);
            var image = ownedImageMemory.Image.AsWritableImage2D();
            var addValue = new Vector<short>(222);

            // SIMD loop for one row
            // ====
            var row = image.PaddedRows[0];
            var intVectors = row.Span.AsVector();

            for (int i = 0; i < intVectors.Length; i++)
            {
                intVectors[i] = intVectors[i] + addValue;
            }
            // ====

            Assert.AreEqual(333, image[0][0], "First pixels is included");
            Assert.AreEqual(111, image[0][vectorCount], "Last pixel is skipped");
        }
    }
}
