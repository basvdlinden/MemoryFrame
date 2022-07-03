using System;

namespace MemoryFrame
{
    public static class ImageExtensions
    {
        public static void FlipHorizonal<T>(this IWritableImage2D<T> image) where T : struct
        {
            for (int rowIndex = 0; rowIndex < image.Height; rowIndex++)
            {
                image[rowIndex].Reverse();
            }
        }

        public static void FlipVertical<T>(this IWritableImage2D<T> image) where T : struct
        {
            image.PaddedRowsMemory.Span.Reverse();
        }
    }
}
