namespace FileApp.FileCreator.Core.Abstraction;

public interface IFileMerger
{
    public Task MergeFiles(IReadOnlyList<string> files, CancellationToken cancellationToken);
}