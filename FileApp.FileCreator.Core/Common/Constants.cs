namespace FileApp.FileCreator.Core.Common;

public static class Constants
{
    public const string GeneratedFileName = "UnortedFakeDataFile_{0}.txt";
    public const string UnsortedFileExtension = ".unsorted";
    public const string TempFileExtension = ".tmp";
    public const int BufferSize = 65536;
    public const int FilesPerRun = 10;
    public const int ChunkFileSize = 10 * 1024;
}