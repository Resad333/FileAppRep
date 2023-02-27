namespace FileApp.FileProcessor.Core.Abstraction;

public interface IFileMerger
{
    public Task MergeFiles(IReadOnlyList<string> sortedFiles, CancellationToken cancellationToken);
}