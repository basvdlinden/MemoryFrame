using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryFrame
{
    public sealed class ImageMemoryPool<T> : MemoryPool<T>
    {
        private ArrayPool<T> _arrayPool;

        public static new MemoryPool<T> Shared = new ImageMemoryPool<T>();

        // Default limit of MemoryPool and ArrayPool is 1024 * 1024: https://adamsitnik.com/Array-Pool/#how-to-use-it
        public ImageMemoryPool(int maxArrayLength = 1024 * 1204 * 8, int maxArraysPerBucket = 50)
        {
            _arrayPool = ArrayPool<T>.Create(maxArrayLength, maxArraysPerBucket);  
        }

        private sealed class ImageMemoryPoolBuffer : IMemoryOwner<T>, IDisposable
        {
            private T[] _array;
            private ArrayPool<T> _arrayPool;

            public Memory<T> Memory
            {
                get
                {
                    T[] array = _array;
                    if (array == null)
                    {
                        throw new ArgumentNullException("Buffered ImageMemory is disposed");
                    }
                    return new Memory<T>(array);
                }
            }

            public ImageMemoryPoolBuffer(ArrayPool<T> arrayPool, int size)
            {
                _arrayPool = arrayPool;
                _array = _arrayPool.Rent(size);
            }

            public void Dispose()
            {
                T[] array = _array;
                if (array != null)
                {
                    _array = null;
                    _arrayPool.Return(array);
                }
            }
        }

        public sealed override int MaxBufferSize => int.MaxValue;

        public sealed override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
        {
            if (minimumBufferSize == -1)
            {
                minimumBufferSize = 1 + 4095 / Unsafe.SizeOf<T>();
            }
            else if ((uint)minimumBufferSize > 2147483647u)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumBufferSize));
            }
            return new ImageMemoryPoolBuffer(_arrayPool, minimumBufferSize);
        }

        protected sealed override void Dispose(bool disposing)
        {
        }
    }
}
