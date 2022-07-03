using System;
using System.Collections.Generic;

namespace MemoryFrame
{
    internal class ReadOnlyImage2D<T> : IReadOnlyImage2D<T> where T : struct
    {
        private readonly ReadOnlyMemory<T>[] _paddedRows;
        private readonly int _paddingPitch;

        public ReadOnlyImage2D(ReadOnlyMemory<T>[] paddedRows, int width, int height, int paddingPitch)
        {
            _paddedRows = paddedRows;
            Width = width;
            Height = height;
            _paddingPitch = paddingPitch;
        }

        public int Width { get; }
        public int Height { get; }

        public ReadOnlySpan<T> this[int rowIndex] => _paddedRows[rowIndex].Span.Slice(0,Width);   // netstandard2.1: Span[..Width];

        public IReadOnlyList<ReadOnlyMemory<T>> PaddedRows => _paddedRows;

        public ReadOnlyMemory<ReadOnlyMemory<T>> PaddedRowsMemory => _paddedRows;

        public IReadOnlyImage2D<T> Slice(int left, int top, int width, int height)
        {
            this.ValidateSlice(left, top, width, height);
            
            int rowPitch = Padding.GetRowPitch(width, _paddingPitch);

            var rows = new ReadOnlyMemory<T>[height];
            int sourceRowIndex = top;
            for (int targetRowIndex = 0; targetRowIndex < height; targetRowIndex++)
            {
                rows[targetRowIndex] = _paddedRows[sourceRowIndex].Slice(left, rowPitch);
                sourceRowIndex += 1;
            }

            return new ReadOnlyImage2D<T>(rows, width, height, _paddingPitch);
        }

        public IWritableImage2D<T> AsTransientCopy() => ImageShared.CreateTransientCopy(this, _paddingPitch);

        public IReadOnlyImage2D<T> AsReadOnly() => this;

        public IReadOnlyList<ReadOnlyMemory<T>> GetPaddedRows() => _paddedRows;

        public IEnumerable<T> PixelValues
        {
            get
            {
                for (int rowIndex = 0; rowIndex < Height; rowIndex++)
                {
                    ReadOnlyMemory<T> row = _paddedRows[rowIndex];
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
                    ReadOnlyMemory<T> row = _paddedRows[rowIndex];
                    for (int columnIndex = 0; columnIndex < Width; columnIndex++)
                    {
                        yield return new PixelInfo<T>(left: columnIndex, top: rowIndex, row.Span[columnIndex]);
                    }
                }
            }
        }
    }
}
