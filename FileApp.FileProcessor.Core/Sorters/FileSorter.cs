using FileApp.FileProcessor.Core.Abstraction;
using FileApp.FileProcessor.Core.Settings;
using static FileApp.FileProcessor.Core.Common.Constants;

namespace FileApp.FileProcessor.Core.Sorters;

public class FileSorter : IFileSorter
{
    private int _sortedFilesCount = 0;
    private readonly int _fileCountToProcessedForTask = 20;
    private readonly SortSettings _settings;
    private readonly string _fileDirectory;
    public FileSorter(SortSettings settings, string fileDirectory)
    {
        _settings = settings;
        _fileDirectory = fileDirectory;
    }

    public async Task<IReadOnlyList<string>> SortFiles(IReadOnlyCollection<string> unsortedFiles)
    {
        var sortedFiles = new List<string>(unsortedFiles.Count);
        double totalFiles = unsortedFiles.Count;

        int i = 0;
        int processedFileCount = 0;
        var tasks = new List<Task>();
        while (processedFileCount < unsortedFiles.Count)
        {
            var res = unsortedFiles.Skip(i * _fileCountToProcessedForTask).Take(_fileCountToProcessedForTask).ToList();

            var task = SortFile(res, totalFiles);
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

        _sortedFilesCount = 0;

        foreach (var unsortedFile in unsortedFiles)
        {
            var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
            sortedFiles.Add(sortedFilename);

            var unsortedFilePath = Path.Combine(_fileDirectory, unsortedFile);
            File.Delete(unsortedFilePath);
        }

        return sortedFiles;
    }

    private async Task SortFile(List<string> files, double totalFiles)
    {
        foreach (var unsortedFile in files)
        {
            var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
            var unsortedFilePath = Path.Combine(_fileDirectory, unsortedFile);
            var sortedFilePath = Path.Combine(_fileDirectory, sortedFilename);
            var source = File.OpenRead(unsortedFilePath);
            var target = File.OpenWrite(sortedFilePath);

            var unsortedLines = new List<string>();
            var bufferSize = unsortedFile.Length < _settings.InputBufferSize ? (int)unsortedFile.Length : _settings.InputBufferSize;
            using var streamReader = new StreamReader(source, bufferSize: bufferSize);
            while (!streamReader.EndOfStream)
            {
                unsortedLines.Add((await streamReader.ReadLineAsync())!);
            }
            unsortedLines.Sort(_settings.Comparer);
            await using var streamWriter = new StreamWriter(target, bufferSize: _settings.OutputBufferSize);
            foreach (var line in unsortedLines.Where(x => x is not null))
            {
                await streamWriter.WriteLineAsync(line);
            }
            
            unsortedLines.Clear();
            Interlocked.Increment(ref _sortedFilesCount);
            _settings.ProgressHandler?.Report(_sortedFilesCount / totalFiles);
        }
    }
}