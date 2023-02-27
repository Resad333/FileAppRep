namespace FileApp.FileProcessor.Core.Abstraction;

public interface IFileSplitter
{
    public Task<IReadOnlyCollection<string>> SplitFile(string fileName, CancellationToken cancellationToken);
}