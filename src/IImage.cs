using System.Collections.Generic;

namespace MemoryFrame
{
    public interface IImage<T> where T : struct
    {
        /// <summary>
        /// Number of rows that make up the image.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Number of usable pixels in each row.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Enumerate all pixel values of the image. Rows from top to down. Pixels from left to right.
        /// Padded pixels are excluded. Note that enumerating the pixels in this way has a performance penalty.
        /// </summary>
        IEnumerable<T> PixelValues { get; }

        /// <summary>
        /// Enumerate all pixels of the image. Rows from top to down. Pixels from left to right.
        /// The <see cref="PixelInfo&lt;T&gt;"/> entry contains both the value and the index in the image.
        /// Padded pixels are excluded. Note that enumerating the pixels in this way has a performance penalty.
        /// </summary>
        IEnumerable<PixelInfo<T>> Pixels { get; }
    }

    public readonly struct PixelInfo<T> where T : struct
    {
        public PixelInfo(int left, int top, T val)
        {
            Left = left;
            Top = top;
            Val = val;
        }

        public readonly int Left;
        public readonly int Top;
        public readonly T Val;
    }
}
