namespace FileApp.FileProcessor.Core.Settings;

public class FileProcessorSettings
{
    public FileProcessorSettings()
    {
        Split = new SplitSettings();
        Sort = new SortSettings();
        Merge = new MergeSettings();
    }

    public string FileDirectory { get; init; } = @"C:\temp\files";
    public SplitSettings Split { get; init; }
    public SortSettings Sort { get; init; }
    public MergeSettings Merge { get; init; }
}