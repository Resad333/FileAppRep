using FileApp.FileCreator.Core.Abstraction;
using System.Diagnostics;
using static FileApp.FileCreator.Core.Common.Constants;

namespace FileApp.FileCreator.Core.Generators;

public class FileGenerator : IFileGenerator
{
    private readonly string _fileDirectory;
    private readonly IDataGenerator _dataGenerator;
    public FileGenerator(string fileDirectory, IDataGenerator dataGenerator)
    {
        _fileDirectory = fileDirectory;
        _dataGenerator = dataGenerator;
    }


    public async Task<IReadOnlyCollection<string>> GenerateChunkFiles(int fileSizeInKB)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var fileSize = ChunkFileSize;
        var filenames = new List<string>();
        var totalFiles = Math.Ceiling(fileSizeInKB / (double)fileSize);
        var currentFile = 0L;
        while (filenames.Count < totalFiles)
        {
            var filename = $"{++currentFile}.unsorted";
            filenames.Add(filename);
        }

        int i = 0;
        int processedFileCount = 0;
        int fileCount = 10;
        var tasks = new List<Task>();
        while (processedFileCount < filenames.Count)
        {
            var res = filenames.Skip(i * fileCount).Take(fileCount).ToList();

            var task = CreateChunkFile(res);
            tasks.Add(task);

            processedFileCount += res.Count;
            i++;
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("HANDLED_EXCEPTION: " + ex.Flatten().Message);
        }

        stopwatch.Stop();
        Console.WriteLine($"CreateChunkFiles Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. thread = {Thread.CurrentThread.ManagedThreadId}");

        return filenames;
    }

    private async Task CreateChunkFile(List<string> filenames)
    {
        var stopwatch = new Stopwatch();

        var fileSize = ChunkFileSize;
        foreach (var filename in filenames)
        {
            stopwatch.Start();

            await using var unsortedFile = File.OpenWrite(Path.Combine(_fileDirectory, filename));
            await using var outputWriter = new StreamWriter(unsortedFile, bufferSize: BufferSize);

            while (unsortedFile.Length / 1024 < fileSize)
            {
                var data = _dataGenerator.GenerateData();
                var value = $"{data.Number}. {data.Content} \n";//"1. Apple xxx kkk \n";
                await outputWriter.WriteAsync(value.AsMemory(), CancellationToken.None);
            }

            stopwatch.Stop();
            Console.WriteLine($"CreateFile Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. file = {filename},  thread = {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}