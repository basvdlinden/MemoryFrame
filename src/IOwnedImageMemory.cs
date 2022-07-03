using System;

namespace MemoryFrame
{
    /// <summary>
    /// The owner of a <see cref="IImageMemory&lt;T&gt;"/> instance. There must always be one owner of the image. When the ownership is not passed 
    /// and the owner does not need the image anymore, the underlaying memory must be released or returned to the pool by calling the Dispose method.
    /// </summary>
    public interface IOwnedImageMemory<T> : IDisposable where T : struct
    {
        IImageMemory<T> Image { get; }
    }

    /// <summary>
    /// Represents an image where the pixels are arranged as a continuous block of memory. The rows can be padded with additional pixels, in order to 
    /// align the rows for vector operations (SIMD).
    /// </summary>
    public interface IImageMemory<T> : IImage<T> where T : struct
    {
        /// <summary>
        /// Number of pixels in each row, when including the padded pixels. 
        /// This row pitch determines where rows start in the underlaying continuous block of memory.
        /// </summary>
        int RowPitch { get; }

        /// <summary>
        /// Returns all the rows, including the padded pixels, as a continuous block of memory. The Memory is sliced so it contains all the 
        /// padded rows, and not more. The underlaying block of memory can be larger.
        /// </summary>
        Memory<T> Memory { get; }

        /// <summary>
        /// Get a writable 2D view on the image data in the underlaying block of memory.
        /// It allows the image to be sliced, without changing the underlaying block of memory.
        /// </summary>
        /// <returns><see cref="IWritableImage2D&lt;T&gt;"/></returns>
        IWritableImage2D<T> AsWritableImage2D();

        /// <summary>
        /// Get a read-only 2D view on the image data in the underlaying block of memory.
        /// It allows the image to be sliced, without changing the underlaying block of memory.
        /// </summary>
        /// <returns><see cref="IReadOnlyImage2D&lt;T&gt;"/></returns>
        IReadOnlyImage2D<T> AsReadOnlyImage2D();

        /// <summary>
        /// Change image size and move pixel values in underlaying block of memory. 
        /// The pixels are padded and aligned in a continuous block of memory. 
        /// Compacting can increase performance for algorithms that process the image as a single array of pixels, 
        /// by using the <see cref="Memory"/> property.
        /// During the creation of this object, the padding pitch was provided (SIMD vector size by default). This padding pitch is used.
        /// Note that existing 2D images representations, created with <see cref="AsWritableImage2D"/> and <see cref="AsReadOnlyImage2D"/>, will
        /// be corrupted, because the row and pixel indexing will not match the pixel alignment in the changed underlaying block of memory any more.
        /// </summary>
        /// <param name="left">Number of pixels from the left to remove</param>
        /// <param name="top">Number of pixels from the top to remove</param>
        /// <param name="width">Number of pixels to preserve in the width</param>
        /// <param name="height">Number of pixels to preserve in the height</param>
        void CompactSlice(int left, int top, int width, int height);

        /// <summary>
        /// Adopt image size from source image and copy pixel values from source image to underlaying block of memory. 
        /// The pixels are padded and aligned in a continuous block of memory.
        /// Compacting can increase performance for algorithms that process the image as a single array of pixels, 
        /// by using the <see cref="Memory"/> property.
        /// During the creation of this object, the padding pitch was provided (SIMD vector size by default). This padding pitch is used.
        /// Note that existing 2D images representations, created with <see cref="AsWritableImage2D"/> and <see cref="AsReadOnlyImage2D"/>, will
        /// be corrupted, because the row and pixel indexing will not match the pixel alignment in the changed underlaying block of memory any more.
        /// </summary>
        /// <param name="sourceImage">Image to </param>
        void CompactCopy(IImage2D<T> sourceImage);

        /// <summary>
        /// Copy pixel values from another 2D Image. The source image does not have to match in size.
        /// </summary>
        /// <param name="sourceImage"></param>
        void Copy(IImage2D<T> sourceImage);
    }
}