namespace FileApp.FileProcessor.Core.Settings;

public class SplitSettings
{
    /// <summary>
    /// Size of unsorted file (chunk) (in bytes)
    /// </summary>
    public int FileSize { get; init; } = 500 * 1024;
    public char NewLineSeparator { get; init; } = '\n';
    public IProgress<double> ProgressHandler { get; init; } = null!;
}