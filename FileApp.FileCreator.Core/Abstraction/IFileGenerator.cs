namespace FileApp.FileCreator.Core.Abstraction;

public interface IFileGenerator
{
    public Task<IReadOnlyCollection<string>> GenerateChunkFiles(int fileSizeInKB);
}