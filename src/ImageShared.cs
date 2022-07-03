using System;

namespace MemoryFrame
{
    internal static class ImageShared
    {
        public static IWritableImage2D<T> CreateTransientCopy<T>(this IImage2D<T> image, int paddingPitch) where T : struct
        {
            var rows = new Memory<T>[image.Height];
            var sourceRows = image.GetPaddedRows();
            for (int i = 0; i < image.Height; i++)
            {
                rows[i] = new T[sourceRows[i].Length];
                sourceRows[i].CopyTo(rows[i]);
            }
            return new WritableImage2D<T>(rows, image.Width, image.Height, paddingPitch);
        }

        public static void ValidateSlice<T>(this IImage2D<T> image, int left, int top, int width, int height) where T : struct
        {
            if (left < 0) throw new ArgumentException("Left must be a positive number.");
            if (top < 0) throw new ArgumentException("Top must be a positive number.");
            if (width < 0) throw new ArgumentException("Width must be a positive number.");
            if (height < 0) throw new ArgumentException("Height must be a positive number.");
            if (top + height > image.Height) throw new ArgumentException("Slice exceeds the boundaries of current height.");
            if (left + width > image.Width) throw new ArgumentException("Slice exceeds the boundaries of current width.");
        }
    }
}
