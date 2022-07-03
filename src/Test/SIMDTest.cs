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
        public void ImageMemory_AddConst()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory = ImageFactory.Padded.CreatePooled<short>(width: vectorCount + 1, height: 3);
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
        public void ImageMemory_Add()
        {
            int vectorCount = Vector<short>.Count;
            if (vectorCount == 1)
            {
                Assert.Pass("No SIMD support. Test not possible.");
            }

            using var ownedImageMemory1 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount + 1, height: 3);
            using var ownedImageMemory2 = ImageFactory.Padded.CreatePooled<short>(width: vectorCount + 1, height: 3);
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
        public void Image2D_AddConst()
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
        public void ImageMemory_Proof_NoPadding_Pixels_Skipped()
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
            Assert.AreEqual(111, image[0][vectorCount], "Last pixels is skipped");
        }
    }
}
