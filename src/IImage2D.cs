using System;
using System.Collections.Generic;

namespace MemoryFrame
{
    /// <summary>
    /// Generic operations on two dimentional image.
    /// </summary>
    public interface IImage2D<T> : IImage<T> where T : struct
    {
        IWritableImage2D<T> AsTransientCopy();
        IReadOnlyImage2D<T> AsReadOnly();

        IReadOnlyList<ReadOnlyMemory<T>> GetPaddedRows();
    }

    /// <summary>
    /// A writable; sliced; transparrant; padded; two dimentional view on memory.
    /// Underlaying memory can be a jagged array or a continuous block of memory.
    /// Memory can be managed or unmanaged.
    /// </summary>
    public interface IWritableImage2D<T> : IImage2D<T> where T : struct
    {
        /// <summary>
        /// Get a span of pixels for the specfied row index. The lenght is equal to the Width of the image.
        /// </summary>
        /// <param name="rowIndex">The index of the row to return</param>
        /// <returns><see cref="Span&lt;T&gt;"/></returns>
        Span<T> this[int rowIndex] { get; }

        /// <summary>
        /// A list with writable and padded rows of pixels. 
        /// </summary>
        IReadOnlyList<Memory<T>> PaddedRows { get; }
        Memory<Memory<T>> PaddedRowsMemory { get; }

        IWritableImage2D<T> Slice(int left, int top, int width, int height);
    }

    /// <summary>
    /// A read-only; sliced; transparrant; padded; two dimentional view on memory.
    /// Underlaying memory can be a jagged array or a continuous block of memory.
    /// Memory can be managed or unmanaged.
    /// </summary>
    public interface IReadOnlyImage2D<T> : IImage2D<T> where T : struct
    {
        ReadOnlySpan<T> this[int rowIndex] { get; }

        IReadOnlyList<ReadOnlyMemory<T>> PaddedRows { get; }
        ReadOnlyMemory<ReadOnlyMemory<T>> PaddedRowsMemory { get; }

        IReadOnlyImage2D<T> Slice(int left, int top, int width, int height);
    }



}