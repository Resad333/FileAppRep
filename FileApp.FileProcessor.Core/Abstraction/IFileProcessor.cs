namespace FileApp.FileProcessor.Core.Abstraction;

public interface IFileProcessor
{
    public Task Process(string fileName);
}