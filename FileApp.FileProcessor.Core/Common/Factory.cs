using FileApp.FileProcessor.Core.Abstraction;
using FileApp.FileProcessor.Core.Comparers;
using FileApp.FileProcessor.Core.Mergers;
using FileApp.FileProcessor.Core.Settings;
using FileApp.FileProcessor.Core.Sorters;
using FileApp.FileProcessor.Core.Splitters;

namespace FileApp.FileProcessor.Core.Common;

public class Factory
{
    private Factory() { }

    public static IFileProcessor CreateFileProcessor(FileProcessorSettings settings)
    {
        IFileSplitter fileSplitter = CreateFileSplitter(settings);
        IFileSorter fileSorter = CreateFileSorter(settings);
        IFileMerger fileMerger = CreateFileMerger(settings);
        //any file processor implementation can be returned  which implements IFileProcessor interface
        var processor = CreateFileProcessor(fileSplitter, fileSorter, fileMerger);
        return processor;
    }

    public static IFileProcessor CreateFileProcessor(IFileSplitter fileSplitter, IFileSorter fileSorter, IFileMerger fileMerger)
    {
        var processor = new FileProcessors.FileProcessor(fileSplitter, fileSorter, fileMerger);
        //any file processor implementation can be returned  which implements IFileProcessor interface
        return processor;
    }

    public static IFileSplitter CreateFileSplitter(FileProcessorSettings fileProcessorSettings)
    {
        var splitter = new FileSplitter(fileProcessorSettings.Split, fileProcessorSettings.FileDirectory);
        //any file splitter implementation can be returned  which implements IFileSplitter interface
        return splitter;
    }

    public static IFileSorter CreateFileSorter(FileProcessorSettings fileProcessorSettings)
    {
        var sorter = new FileSorter(fileProcessorSettings.Sort, fileProcessorSettings.FileDirectory);
        //any file sorter implementation can be returned  which implements IFileSorter interface
        return sorter;
    }

    public static IFileMerger CreateFileMerger(FileProcessorSettings fileProcessorSettings)
    {
        var merger = new FileMerger(fileProcessorSettings.Merge, fileProcessorSettings.FileDirectory, fileProcessorSettings.Sort.Comparer);
        //any file merger implementation can be returned  which implements IFileMerger interface
        return merger;
    }

    public static IComparer<string> CreateComparer(string type = "")
    {
        var comparer = new FirstWordNumberComparer(2);
        if (string.Equals(type, nameof(FirstWordNumberComparer), StringComparison.OrdinalIgnoreCase))
        {
            return comparer;
        }
        return Comparer<string>.Default;
    }
}