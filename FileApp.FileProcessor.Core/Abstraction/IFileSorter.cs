namespace FileApp.FileProcessor.Core.Abstraction;

public interface IFileSorter
{
    public Task<IReadOnlyList<string>> SortFiles(IReadOnlyCollection<string> unsortedFiles);
}