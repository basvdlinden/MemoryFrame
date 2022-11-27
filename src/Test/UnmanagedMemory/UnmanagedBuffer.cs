using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MemoryFrame.Test.UnmanagedMemory
{
    internal class UnmanagedBuffer<T> : IMemoryOwner<T> where T : unmanaged
    {
        private readonly IntPtr _pointer = IntPtr.Zero;
        private readonly Memory<T> _memory;


        public UnmanagedBuffer(int size)
        {
            _pointer = IntPtr.Zero;
            int elementSize = Marshal.SizeOf(default(T));
            _pointer = Marshal.AllocHGlobal(size * elementSize);
            unsafe
            {
                var memoryManager = new UnmanagedMemoryManager<T>((T*)_pointer, size);
                _memory = memoryManager.Memory;
            }
        }

        public Memory<T> Memory => _memory;


        public T GetValue(int index)
        {
            unsafe
            {
                var array = (T*)_pointer;
                return array[index];
            }
        }

        public bool IsDisposed { get; private set; } = false;

        public void Dispose()
        {
            IsDisposed = true;
            Marshal.FreeHGlobal(_pointer);
        }
    }
}
