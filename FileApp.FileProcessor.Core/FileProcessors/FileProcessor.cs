using FileApp.FileProcessor.Core.Abstraction;
using System.Diagnostics;

namespace FileApp.FileProcessor.Core.FileProcessors;
public class FileProcessor : IFileProcessor
{
    private readonly IFileSplitter _fileSplitter;
    private readonly IFileSorter _fileSorter;
    private readonly IFileMerger _fileMerger;

    public FileProcessor(
        IFileSplitter fileSplitter,
        IFileSorter fileSorter,
        IFileMerger fileMerger)
    {
        _fileSplitter = fileSplitter ?? throw new ArgumentNullException(nameof(fileSplitter));
        _fileSorter = fileSorter ?? throw new ArgumentNullException(nameof(fileSorter));
        _fileMerger = fileMerger ?? throw new ArgumentNullException(nameof(fileMerger));
    }

    public async Task Process(string fileName)
    {
        Console.WriteLine($"Creating output file... thread = {Thread.CurrentThread.ManagedThreadId}");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        CancellationToken cancellationToken = CancellationToken.None;
        var files = await _fileSplitter.SplitFile(fileName, cancellationToken);

        var sortedFiles = await _fileSorter.SortFiles(files);

        await _fileMerger.MergeFiles(sortedFiles, cancellationToken);

        stopwatch.Stop();
        Console.WriteLine($"Created sorted output file in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. thread = {Thread.CurrentThread.ManagedThreadId}");

    }
}