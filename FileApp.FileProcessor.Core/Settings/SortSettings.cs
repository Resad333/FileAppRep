namespace FileApp.FileProcessor.Core.Settings;

public class SortSettings
{
    public IComparer<string> Comparer { get; init; } 
    public int InputBufferSize { get; init; } = 65536;
    public int OutputBufferSize { get; init; } = 65536;
    public IProgress<double> ProgressHandler { get; init; } = null!;
}