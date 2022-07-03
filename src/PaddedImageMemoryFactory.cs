using System;
using System.Buffers;
using System.Numerics;

namespace MemoryFrame
{

    internal class PaddedImageMemoryFactory : IPaddedImageMemoryFactory
    {
        public IOwnedImageMemory<T> CreatePooled<T>(int width, int height, int paddingPitch = -1, bool clearMemory = false) where T : struct
        {
            int rowPitch = GetRowPitch<T>(width, ref paddingPitch);
            int memorySize = rowPitch * height;
            var memoryOwned = ImageMemoryPool<T>.Shared.Rent(memorySize);
            if (clearMemory)
            {
                memoryOwned.Memory.Span.Fill(default);
            }
            return new OwnedImageMemory<T>(memoryOwned, width, height, paddingPitch, rowPitch);
        }

        public IOwnedImageMemory<T> CreatePooledCopy<T>(IImage2D<T> sourceImage, int paddingPitch = -1) where T : struct
        {
            var memoryOwner = CreatePooled<T>(sourceImage.Width, sourceImage.Height, paddingPitch);
            memoryOwner.Image.Copy(sourceImage);
            return memoryOwner;
        }

        public IWritableImage2D<T> CreateTransient<T>(int width, int height, int paddingPitch = -1) where T : struct
        {
            int rowPitch = GetRowPitch<T>(width, ref paddingPitch);

            Memory<T>[] rows = new Memory<T>[height];
            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                rows[rowIndex] = new T[rowPitch];
            }
            return new WritableImage2D<T>(rows, width, height, paddingPitch);
        }

        public IOwnedImageMemory<T> OwnPooled<T>(IMemoryOwner<T> memoryOwned, int width, int height, int paddingPitch = -1, bool clearMemory = false) where T : struct
        {
            int rowPitch = GetRowPitch<T>(width, ref paddingPitch);
            int memorySize = rowPitch * height;
            if (memoryOwned.Memory.Length < memorySize)
            {
                throw new ArgumentOutOfRangeException(nameof(memoryOwned), $"Provided memory is too small. Memory must have at least {memorySize} entries.");
            }
            if (clearMemory)
            {
                memoryOwned.Memory.Span.Fill(default);
            }
            return new OwnedImageMemory<T>(memoryOwned, width, height, paddingPitch, rowPitch);
        }

        private static int GetRowPitch<T>(int width, ref int paddingPitch) where T : struct
        {
            if (paddingPitch == -1)
            {
                paddingPitch = Vector<T>.Count;
            }
            return Padding.GetRowPitch(width, paddingPitch);
        }

    }
}
