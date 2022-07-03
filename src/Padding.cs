using System;

namespace MemoryFrame
{
    internal class Padding
    {
        public static int GetRowPitch(int width, int paddingPitch)
        {
            if (!IsPowerOfTwo(paddingPitch)) throw new ArgumentException("Vector size must be a power of 2.");

            int mask = paddingPitch - 1;

            if ((width & mask) == 0)
            {
                return width;
            }

            return (width | mask) + 1;
        }

        internal static bool IsPowerOfTwo(int n)
        {
            return (n & (n - 1)) == 0;
        }
    }
}
