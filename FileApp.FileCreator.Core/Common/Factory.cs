using FileApp.FileCreator.Core.Abstraction;
using FileApp.FileCreator.Core.DataGenerator;
using FileApp.FileCreator.Core.Generators;
using FileApp.FileCreator.Core.Mergers;

namespace FileApp.FileCreator.Core.Common;

public class Factory
{
    private Factory() { }

    public static IFileCreator CreateFileCreator(string fileDirectory, FileCreatorType fileCreatorType)
    {
        if (fileCreatorType == FileCreatorType.ChunkerImplementation)
        {
            var dataGenerator = CreateDataGenerator();
            var fileMerger = CreateFileMerger(fileDirectory);
            var fileGenerator = CreateFileGenerator(fileDirectory, dataGenerator);
            var fileCreator = CreateFileCreator(fileGenerator, fileMerger);

            return fileCreator;
        }

        if (fileCreatorType == FileCreatorType.QueueImplementation)
        {
            return new FileCreatorQueueImpl(fileDirectory, CreateDataGenerator());
        }

        return null;
    }

    public static IFileCreator CreateFileCreator(IFileGenerator fileGenerator, IFileMerger fileMerger)
    {
        var fileCreator = new Generators.FileCreator(fileGenerator, fileMerger);

        return fileCreator;
    }

    public static IFileGenerator CreateFileGenerator(string fileDirectory, IDataGenerator dataGenerator)
    {
        var fileGenerator = new FileGenerator(fileDirectory, dataGenerator);

        return fileGenerator;
    }

    public static IFileMerger CreateFileMerger(string fileDirectory)
    {
        var merger = new ChunkFileMerger(fileDirectory);
        //any file merger implementation can be returned  which implements IFileMerger interface
        return merger;
    }

    public static IDataGenerator CreateDataGenerator()
    {
        var dataGenerator = new FakeDataGenerator();

        return dataGenerator;
    }
}