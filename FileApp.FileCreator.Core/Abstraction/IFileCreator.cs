namespace FileApp.FileCreator.Core.Abstraction;

public interface IFileCreator
{
    public Task Create(int fileSizeInKB);
}