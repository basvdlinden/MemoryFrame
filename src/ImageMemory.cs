using System;
using System.Collections.Generic;

namespace MemoryFrame
{
    internal class ImageMemory<T> : IImageMemory<T> where T : struct
    {
        private readonly Memory<T> _memory;
        protected readonly int _paddingPitch;

        public ImageMemory(Memory<T> memory, int width, int height, int paddingPitch, int rowPitch)
        {
            if (rowPitch < width) throw new ArgumentException($"Width {width} must be smaller than the RowPitch: {rowPitch}.");

            _memory = memory;

            Width = width;
            RowPitch = rowPitch;
            Height = height;
            _paddingPitch = paddingPitch;

            int memorySize = RowPitch * Height;
            if (memory.Length < memorySize) throw new ArgumentException($"Memory is not large enough to hold an image with hight: {Height} and row-pitch: {RowPitch}.");
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int RowPitch { get; private set; }
        public Memory<T> Memory => _memory.Slice(0, RowPitch * Height);

        public IWritableImage2D<T> AsWritableImage2D() => new WritableImage2D<T>(SliceRowsWritable(), Width, Height, _paddingPitch);
        public IReadOnlyImage2D<T> AsReadOnlyImage2D() => new ReadOnlyImage2D<T>(SliceRowsReadOnly(), Width, Height, _paddingPitch);

        private Memory<T>[] SliceRowsWritable()
        {
            Memory<T> memory = Memory;
            var rows = new Memory<T>[Height];
            int strartIndex = 0;
            for (int i = 0; i < Height; i++)
            {
                rows[i] = memory.Slice(strartIndex, RowPitch);
                strartIndex += RowPitch;
            }

            return rows;
        }

        private ReadOnlyMemory<T>[] SliceRowsReadOnly()
        {
            ReadOnlyMemory<T> memory = Memory;
            var rows = new ReadOnlyMemory<T>[Height];
            int strartIndex = 0;
            for (int i = 0; i < Height; i++)
            {
                rows[i] = memory.Slice(strartIndex, RowPitch);
                strartIndex += RowPitch;
            }

            return rows;
        }

        public void CompactSlice(int left, int top, int width, int height)
        {
            var image = AsReadOnlyImage2D();
            var slicedImage = image.Slice(left, top, width, height);
            Width = width;
            Height = height;
            RowPitch = Padding.GetRowPitch(Width, _paddingPitch);
            Copy(slicedImage);
        }

        public void CompactCopy(IImage2D<T> sourceImage)
        {
            Width = sourceImage.Width;
            Height = sourceImage.Height;
            RowPitch = Padding.GetRowPitch(Width, _paddingPitch);
            Copy(sourceImage);
        }

        public void Copy(IImage2D<T> sourceImage)
        {
            int rowPitch = RowPitch;
            var memorySpan = Memory.Span;
            int rowsToCopy = Math.Min(sourceImage.Height, Height);
            var sourceRows = sourceImage.GetPaddedRows();

            int pixelIndex = 0;
            for (int rowIndex = 0; rowIndex < rowsToCopy; rowIndex++)
            {
                var paddedRowSpan = sourceRows[rowIndex].Span;
                paddedRowSpan = paddedRowSpan.Length <= rowPitch
                    ? paddedRowSpan
                    : paddedRowSpan.Slice(0, rowPitch);

                var destination = memorySpan.Slice(pixelIndex);
                paddedRowSpan.CopyTo(destination);
                pixelIndex += rowPitch;
            }
        }

        public IEnumerable<T> PixelValues
        {
            get
            {
                var memory = _memory;
                int rowStartIndex = 0;
                for (int rowIndex = 0; rowIndex < Height; rowIndex++)
                {
                    int pixelIndex = rowStartIndex;
                    for (int columnIndex = 0; columnIndex < Width; columnIndex++)
                    {
                        yield return memory.Span[pixelIndex];
                        pixelIndex++;
                    }
                    rowStartIndex += RowPitch;
                }
            }
        }

        public IEnumerable<PixelInfo<T>> Pixels
        {
            get
            {
                var memory = _memory;
                int rowStartIndex = 0;
                for (int rowIndex = 0; rowIndex < Height; rowIndex++)
                {
                    int pixelIndex = rowStartIndex;
                    for (int columnIndex = 0; columnIndex < Width; columnIndex++)
                    {
                        yield return new PixelInfo<T>(left: rowIndex, top: pixelIndex, val: memory.Span[pixelIndex]);
                            
                        pixelIndex++;
                    }
                    rowStartIndex += RowPitch;
                }
            }
        }
    }
}
