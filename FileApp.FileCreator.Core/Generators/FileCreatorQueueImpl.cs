using FileApp.FileCreator.Core.Abstraction;
using System.Collections.Concurrent;
using System.Diagnostics;
using static FileApp.FileCreator.Core.Common.Constants;

namespace FileApp.FileCreator.Core.Generators;

public class FileCreatorQueueImpl : IFileCreator
{
    private readonly int _numberOfThreads = 5;
    private readonly string _fileDirectory;
    private readonly IDataGenerator _dataGenerator;
    private readonly ConcurrentQueue<string> _lineQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    public FileCreatorQueueImpl(string fileDirectory, IDataGenerator dataGenerator)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _lineQueue = new ConcurrentQueue<string>();
        _fileDirectory = fileDirectory;
        _dataGenerator = dataGenerator;
    }

    public async Task Create(int fileSizeInKB)
    {
        Console.WriteLine($"Creating file... fileSize = {fileSizeInKB} KB, thread = {Thread.CurrentThread.ManagedThreadId}");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var tasks = AddDataToQueue();
        var fileCreatorTask = CreateFileFromQueue(fileSizeInKB);
        tasks.Add(fileCreatorTask);

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("HANDLED_EXCEPTION: " + ex.Flatten().Message);
        }

        stopwatch.Stop();
        Console.WriteLine($"Create Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. thread = {Thread.CurrentThread.ManagedThreadId}");
    }

    private async Task CreateFileFromQueue(int fileSizeInKB)
    {
        var guid = Guid.NewGuid().ToString().Replace("-", "");
        var outputFile = File.OpenWrite(Path.Combine(_fileDirectory, string.Format(GeneratedFileName, guid)));
        await using var outputWriter = new StreamWriter(outputFile, bufferSize: BufferSize);
        await Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_lineQueue.TryDequeue(out var line))
                {
                    await outputWriter.WriteLineAsync(line.AsMemory(), _cancellationTokenSource.Token);
                    if (outputFile.Length / 1024 >= fileSizeInKB)
                    {
                        _cancellationTokenSource.Cancel();
                        _lineQueue.Clear();
                        break;
                    }
                }
            }
        });

    }

    private List<Task> AddDataToQueue()
    {
        var count = 0;
        var tasks = new List<Task>();
        while (count < _numberOfThreads)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var data = _dataGenerator.GenerateData();
                    var value = $"{data.Number}. {data.Content}";

                    _lineQueue.Enqueue(value);
                }

                stopwatch.Stop();
                Console.WriteLine($"AddDataToQueue Done in {stopwatch.ElapsedMilliseconds / 1000.0} Seconds. thread = {Thread.CurrentThread.ManagedThreadId}");
            });
            tasks.Add(task);
            count++;
        }

        return tasks;
    }
}