using FileApp.FileProcessor.Core.Abstraction;
using FileApp.FileProcessor.Core.Models;
using FileApp.FileProcessor.Core.Settings;
using static FileApp.FileProcessor.Core.Common.Constants;

namespace FileApp.FileProcessor.Core.Mergers;

public class FileMerger : IFileMerger
{
    private readonly IComparer<string> _comparer;
    private readonly MergeSettings _settings;
    private readonly string _fileDirectory;
    private double _totalFilesToMerge = 0;
    private int _mergeFilesProcessed = 0;
    public FileMerger(MergeSettings settings, string fileDirectory, IComparer<string> comparer)
    {
        _settings = settings;
        _fileDirectory = fileDirectory;
        _comparer = comparer;
    }

    public async Task MergeFiles(IReadOnlyList<string> sortedFiles, CancellationToken cancellationToken)
    {
        InitializeProgressbarParameters(sortedFiles.Count);
        var guid = Guid.NewGuid().ToString().Replace("-", "");
        var outputFile = File.OpenWrite(Path.Combine(_fileDirectory, string.Format(SortedOutputFile, guid)));

        while (sortedFiles.Count > 0)
        {
            int i = 0;
            int processedFileCount = 0;
            var outputFileNames = new List<string>();
            var tasks = new List<Task>();

            while (processedFileCount < sortedFiles.Count)
            {
                var res = sortedFiles.Skip(i * _settings.FilesPerRun).Take(_settings.FilesPerRun).ToList();
                var outputFilename = $"{++i}{SortedFileExtension}{TempFileExtension}";
                var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                var task = Merge(res, outputFilename, outputStream, cancellationToken);
                tasks.Add(task);
                outputFileNames.Add(outputFilename);
                processedFileCount += res.Count;
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("HANDLED_EXCEPTION: " + ex.Flatten().Message);
            }

            foreach (var outputFilename in outputFileNames)
            {
                File.Move(GetFullPath(outputFilename), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)), true);
            }

            sortedFiles = Directory.GetFiles(_fileDirectory, $"*{SortedFileExtension}")
                .OrderBy(x =>
                {
                    var filename = Path.GetFileNameWithoutExtension(x);
                    return int.Parse(filename);
                })
                .ToArray();

            if (sortedFiles.Count > 1)
            {
                continue;
            }

            await Merge(sortedFiles, string.Empty, outputFile, cancellationToken);

            break;
        }
    }

    private async Task Merge(IReadOnlyList<string> filesToMerge, string outputFilename, Stream outputStream, CancellationToken cancellationToken)
    {
        var (streamReaders, lines) = await InitializeStreamReaders(filesToMerge);
        var finishedStreamReaders = new List<int>(streamReaders.Length);
        var done = false;
        await using var outputWriter = new StreamWriter(outputStream, bufferSize: _settings.OutputBufferSize);

        while (!done)
        {
            lines.Sort((line1, line2) => _comparer.Compare(line1.Value, line2.Value));
            var valueToWrite = lines[0].Value;
            var streamReaderIndex = lines[0].StreamReaderIndex;
            await outputWriter.WriteLineAsync(valueToWrite.AsMemory(), cancellationToken);

            if (streamReaders[streamReaderIndex].EndOfStream)
            {
                var indexToRemove = lines.FindIndex(x => x.StreamReaderIndex == streamReaderIndex);
                lines.RemoveAt(indexToRemove);
                finishedStreamReaders.Add(streamReaderIndex);
                done = finishedStreamReaders.Count == streamReaders.Length;
                Interlocked.Increment(ref _mergeFilesProcessed);
                _settings.ProgressHandler?.Report(_mergeFilesProcessed / _totalFilesToMerge);
                continue;
            }

            var value = await streamReaders[streamReaderIndex].ReadLineAsync();
            lines[0] = new Line { Value = value!, StreamReaderIndex = streamReaderIndex };
        }

        Cleanup(streamReaders, filesToMerge);
    }

    private async Task<(StreamReader[] StreamReaders, List<Line> rows)> InitializeStreamReaders(IReadOnlyList<string> sortedFiles)
    {
        var streamReaders = new StreamReader[sortedFiles.Count];
        var lines = new List<Line>(sortedFiles.Count);
        for (var i = 0; i < sortedFiles.Count; i++)
        {
            var sortedFilePath = GetFullPath(sortedFiles[i]);
            var sortedFileStream = File.OpenRead(sortedFilePath);
            streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _settings.InputBufferSize);
            var value = await streamReaders[i].ReadLineAsync();
            var line = new Line
            {
                Value = value!,
                StreamReaderIndex = i
            };
            lines.Add(line);
        }

        return (streamReaders, lines);
    }

    private void InitializeProgressbarParameters(int sortedFilesCount)
    {
        var done = false;
        var size = _settings.FilesPerRun;
        _totalFilesToMerge = sortedFilesCount;
        var result = sortedFilesCount / size;

        while (!done)
        {
            if (result <= 0)
            {
                done = true;
            }
            _totalFilesToMerge += result;
            result /= size;
        }
    }

    private void Cleanup(StreamReader[] streamReaders, IReadOnlyList<string> filesToMerge)
    {
        for (var i = 0; i < streamReaders.Length; i++)
        {
            streamReaders[i].Dispose();
            var temporaryFilename = GetFullPath($"{filesToMerge[i]}.removal");
            File.Move(GetFullPath(filesToMerge[i]), temporaryFilename);
            File.Delete(temporaryFilename);
        }
    }

    private string GetFullPath(string filename)
    {
        return Path.Combine(_fileDirectory, Path.GetFileName(filename));
    }
}