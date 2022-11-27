using System.Buffers;
using System.Numerics;

namespace MemoryFrame
{
    /// <summary>
    /// This factory is responsible for allocating (or take ownership of) memory for an image with pixels of type T. The memory is padded to align 
    /// each row with the size of a <see cref="Vector&lt;T&gt;"/>. This allows for simple code that perform vectorized calculation (SIMD), 
    /// by using one single loop per image or row.
    /// <br/><br/>
    /// <b>Pooled memory</b>: One big array with all pixels in one continuous block of memory. Most likely allocated in LOH. By reusing the 
    /// arrays it is possible to prevent expensive LOH GC overhead. This means that the array must be returned to a pool. There must always be one 
    /// owner of the image. When the ownership is not passed and the owner does not need the image anymore, the memory must be returned to the pool 
    /// by calling the Dispose method. The ownership of the memory is represented by a <see cref="IOwnedImageMemory&lt;T&gt;"/>. 
    /// The continuous block of memory is represented by a <see cref="IImageMemory&lt;T&gt;"/>, which supports slicing the memory into separate rows, 
    /// represented by a <see cref="IWritableImage2D&lt;T&gt;"/>.
    /// <br/><br/>
    /// <b>Transient memory</b>: A jagged array with a separate array for each row. This allows allocating images which don't end up in the LOH. 
    /// The GC can reclaim the memory relatively cheap. The lifetime doesn't need to be managed. There is <b>no</b> need for passing ownership and 
    /// returning the memory to a pool, via the Dispose method. The data is represented by a <see cref="IWritableImage2D&lt;T&gt;"/>.
    /// </summary>
    public interface IPaddedImageMemoryFactory
    {
        /// <summary>
        /// Create a new <see cref="IWritableImage2D&lt;T&gt;"/> instance by allocating separate arrays for each row (jagged array). 
        /// The memory can be relatively easily reclaimed by GC. (If the number of bytes in one rows is within the limit of the LOH threshold)
        /// </summary>
        /// <typeparam name="T">Image pixel tipe</typeparam>
        /// <param name="width">The width of each row.</param>
        /// <param name="height">The number of rows.</param>
        /// <param name="paddingPitch">The row-size in memory is padded to the provided paddingPitch. If this value is -1, the pitch will be equal 
        /// to the SIMD vector size (<see cref="Vector&lt;T&gt;"/>.Count).</param>
        /// <returns><see cref="IWritableImage2D&lt;T&gt;"/></returns>
        IWritableImage2D<T> CreateTransient<T>(int width, int height, int paddingPitch = -1) where T : struct;

        /// <summary>
        /// Create a new <see cref="IOwnedImageMemory&lt;T&gt;"/> instance by renting memory from a pool and taking ownership of the pooled memory.
        /// </summary>
        /// <typeparam name="T">Image pixel tipe</typeparam>
        /// <param name="width">The width of each row.</param>
        /// <param name="height">The number of rows.</param>
        /// <param name="paddingPitch">The row-size in memory is padded to the provided paddingPitch. If this value is -1, the pitch will be equal 
        /// to the SIMD vector size (<see cref="Vector&lt;T&gt;"/>.Count).</param>
        /// <param name="clearMemory">Whether to clear the memory to the default value of &lt;T&gt;</param>
        /// <returns><see cref="IOwnedImageMemory&lt;T&gt;"/></returns>
        IOwnedImageMemory<T> CreatePooled<T>(int width, int height, int paddingPitch = -1, bool clearMemory = false) where T : struct;

        /// <summary>
        /// Create a new IOwnedImageMemory&lt;T&gt; instance by renting memory from a pool and taking ownership of the pooled memory.
        /// The source image provides the width, height, and pixel values to copy.
        /// </summary>
        /// <typeparam name="T">Image pixel tipe</typeparam>
        /// <param name="sourceImage">The source image to copy.</param>
        /// <param name="paddingPitch">The row-size in memory is padded to the provided paddingPitch. If this value is -1, the pitch will be equal 
        /// to the SIMD vector size (<see cref="Vector&lt;T&gt;"/>.Count).</param>
        /// <returns><see cref="IOwnedImageMemory&lt;T&gt;"/></returns>
        IOwnedImageMemory<T> CreatePooledCopy<T>(IImage2D<T> sourceImage, int paddingPitch = -1) where T : struct;

        /// <summary>
        /// Create a new <see cref="IOwnedImageMemory&lt;T&gt;"/> instance by taking ownership of the provided <see cref="IMemoryOwner&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">Image pixel tipe</typeparam>
        /// <param name="memoryOwned">The <see cref="IMemoryOwner&lt;T&gt;"/> that will be used and owned by the new <see cref="IOwnedImageMemory&lt;T&gt;"/> instance.</param>
        /// <param name="width">The width of each row.</param>
        /// <param name="height">The number of rows.</param>
        /// <param name="paddingPitch">The row-size in memory is padded to the provided paddingPitch. If this value is -1, the pitch will be equal 
        /// to the SIMD vector size (<see cref="Vector&lt;T&gt;"/>.Count).</param>
        /// <param name="clearMemory">Whether to clear the memory to the default value of &lt;T&gt;</param>
        /// <returns><see cref="IOwnedImageMemory&lt;T&gt;"/></returns>
        IOwnedImageMemory<T> OwnPooled<T>(IMemoryOwner<T> memoryOwned, int width, int height, int paddingPitch = -1, bool clearMemory = false) where T : struct;

        /// <summary>
        /// Get the buffer size required for all the padded rows.
        /// In case you want to calculate the buffer size in bytes, then multiply the returned size with Marshal.SizeOf(default(T)).
        /// </summary>
        /// <param name="width">The width of each row.</param>
        /// <param name="height">The number of rows.</param>
        /// <param name="paddingPitch">The row-size in memory is padded to the provided paddingPitch. If this value is -1, the pitch will be equal 
        /// to the SIMD vector size (<see cref="Vector&lt;T&gt;"/>.Count).</param>
        /// <returns>Number of T entries</returns>
        int GetBufferSize<T>(int width, int height, int paddingPitch = -1) where T : struct;
    }
}