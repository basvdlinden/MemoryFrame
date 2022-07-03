using System;
using System.Collections.Generic;

namespace MemoryFrame
{
    internal class WritableImage2D<T> : IWritableImage2D<T> where T : struct
    {
        private readonly Memory<T>[] _paddedRows;
        private readonly int _paddingPitch;

        private ReadOnlyMemory<T>[] _paddedReadonlyRows = null;

        public WritableImage2D(Memory<T>[] paddedRows, int width, int height, int paddingPitch)
        {
            _paddedRows = paddedRows;
            Width = width;
            Height = height;
            _paddingPitch = paddingPitch;
        }

        public int Width { get; }
        public int Height { get; }

        private ReadOnlyMemory<T>[] CreateCachedReadOnlyPaddedRows()
        {
            if (_paddedReadonlyRows == null)
            {
                var rows = new ReadOnlyMemory<T>[Height];
                for (int i = 0; i < Height; i++)
                {
                    rows[i] = _paddedRows[i];
                }
                _paddedReadonlyRows = rows;

            }
            return _paddedReadonlyRows;
        }

        public Span<T> this[int rowIndex] => _paddedRows[rowIndex].Span.Slice(0, Width);   // netstandard2.1: Span[..Width];

        public IReadOnlyList<Memory<T>> PaddedRows => _paddedRows;

        public Memory<Memory<T>> PaddedRowsMemory => _paddedRows;

        public IWritableImage2D<T> Slice(int left, int top, int width, int height)
        {
            this.ValidateSlice(left, top, width, height);

            int rowPitch = Padding.GetRowPitch(width, _paddingPitch);

            var rows = new Memory<T>[height];
            int sourceRowIndex = top;
            for (int targetRowIndex = 0; targetRowIndex < height; targetRowIndex++)
            {
                rows[targetRowIndex] = _paddedRows[sourceRowIndex].Slice(left, rowPitch);
                sourceRowIndex += 1;
            }

            return new WritableImage2D<T>(rows, width, height, _paddingPitch);
        }

        public IReadOnlyImage2D<T> AsReadOnly() => new ReadOnlyImage2D<T>(CreateCachedReadOnlyPaddedRows(), Width, Height, _paddingPitch);

        public IWritableImage2D<T> AsTransientCopy() => ImageShared.CreateTransientCopy(this, _paddingPitch);

        public IReadOnlyList<ReadOnlyMemory<T>> GetPaddedRows() => CreateCachedReadOnlyPaddedRows();

        public IEnumerable<T> PixelValues
        {
            get
            {
                for (int rowIndex = 0; rowIndex < Height; rowIndex++)
                {
                    Memory<T> row = _paddedRows[rowIndex];
                    for (int columnIndex = 0; columnIndex < Width; columnIndex++)
                    {
                        yield return row.Span[columnIndex];
                    }
                }
            }
        }

        public IEnumerable<PixelInfo<T>> Pixels
        {
            get
            {
                for (int rowIndex = 0; rowIndex < Height; rowIndex++)
                {
                    Memory<T> row = _paddedRows[rowIndex];
                    for (int columnIndex = 0; columnIndex < Width; columnIndex++)
                    {
                        yield return new PixelInfo<T>(left: columnIndex, top: rowIndex, row.Span[columnIndex]);
                    }
                }
            }
        }
    }
}
