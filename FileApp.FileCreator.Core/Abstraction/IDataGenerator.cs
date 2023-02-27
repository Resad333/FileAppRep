using FileApp.FileCreator.Core.Models;

namespace FileApp.FileCreator.Core.Abstraction;

public interface IDataGenerator
{
    public FileLine GenerateData();
}