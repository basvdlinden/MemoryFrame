namespace MemoryFrame
{
    public static class ImageFactory
    {
        ///<inheritdoc cref="IPaddedImageMemoryFactory"/>
        public static IPaddedImageMemoryFactory Padded { get; } = new PaddedImageMemoryFactory();
    }
}
