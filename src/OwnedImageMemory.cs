using System;
using System.Buffers;
using System.Numerics;

namespace MemoryFrame
{
    internal class OwnedImageMemory<T> : IOwnedImageMemory<T> where T : struct
    {
        private readonly IMemoryOwner<T> _memoryOwner;
        private IImageMemory<T> _imageMemory;

        public OwnedImageMemory(IMemoryOwner<T> memoryOwned, int width, int height)
            : this(memoryOwned, width, height, Vector<T>.Count)
        { }

        public OwnedImageMemory(IMemoryOwner<T> memoryOwned, int width, int height, int paddingPitch)
            : this(memoryOwned, width, height, paddingPitch, Padding.GetRowPitch(width, paddingPitch))
        { }

        public OwnedImageMemory(IMemoryOwner<T> memoryOwned, int width, int height, int paddingPitch, int rowPitch)
        {
            _memoryOwner = memoryOwned;
            _imageMemory = new ImageMemory<T>(memoryOwned.Memory, width, height, paddingPitch, rowPitch);
        }

        public IImageMemory<T> Image { 
            get { 
                if(_imageMemory is null)
                {
                    throw new ObjectDisposedException(nameof(Image), "Memory has been returned to the pool");
                }
                return _imageMemory; 
            } 
        }

        public void Dispose()
        {
            _imageMemory = null;
            _memoryOwner.Dispose();
        }
    }
}
