using FileApp.FileCreator.Core.Abstraction;
using System.Diagnostics;

namespace FileApp.FileCreator.Core.Generators;

public class FileCreator : IFileCreator
{
    private readonly IFileMerger _fileMerger;
    private readonly IFileGenerator _fileGenerator;
    public FileCreator(IFileGenerator fileGenerator, IFileMerger fileMerger)
    {
        _fileGenerator = fileGenerator ?? throw new ArgumentNullException(nameof(fileGenerator));
        _fileMerger = fileMerger ?? throw new ArgumentNullException(nameof(fileMerger));
    }


    public async Task Create(int fileSizeInKB)
    {
        Console.WriteLine($"Creating file... fileSize = {fileSizeInKB} KB, thread = {Thread.CurrentThread.ManagedThreadId}");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var files = await _fileGenerator.GenerateChunkFiles(fileSizeInKB);
        await _fileMerger.MergeFiles(files.ToList(), CancellationToken.None);

        stopwatch.Stop();
        Console.WriteLine($"CreateFile Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. thread = {Thread.CurrentThread.ManagedThreadId}");
    }
}