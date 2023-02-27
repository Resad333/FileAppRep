using FileApp.FileCreator.Core.Abstraction;
using static FileApp.FileCreator.Core.Common.Constants;

namespace FileApp.FileCreator.Core.Mergers;

public class ChunkFileMerger : IFileMerger
{
    private readonly string _fileDirectory;

    public ChunkFileMerger(string fileDirectory)
    {
        _fileDirectory = fileDirectory;
    }

    public async Task MergeFiles(IReadOnlyList<string> files, CancellationToken cancellationToken)
    {
        var guid = Guid.NewGuid().ToString().Replace("-", "");
        var outputFile = File.OpenWrite(Path.Combine(_fileDirectory, string.Format(GeneratedFileName, guid)));
        var done = false;
        while (!done)
        {
            var runSize = FilesPerRun;
            var finalRun = files.Count <= runSize;

            if (finalRun)
            {
                await Merge(files, outputFile, cancellationToken);
                return;
            }

            var runs = files.Chunk(runSize);
            var chunkCounter = 0;
            foreach (var chunkFiles in runs)
            {
                var outputFilename = $"{++chunkCounter}{UnsortedFileExtension}{TempFileExtension}";
                if (chunkFiles.Length == 1)
                {
                    OverwriteTempFile(chunkFiles.First(), outputFilename);
                    continue;
                }

                var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                await Merge(chunkFiles, outputStream, cancellationToken);
                OverwriteTempFile(outputFilename, outputFilename);
            }

            files = Directory.GetFiles(_fileDirectory, $"*{UnsortedFileExtension}")
                .OrderBy(x =>
                {
                    var filename = Path.GetFileNameWithoutExtension(x);
                    return int.Parse(filename);
                })
                .ToArray();

            if (files.Count > 1)
            {
                continue;
            }

            done = true;
        }
    }

    private async Task Merge(IReadOnlyList<string> filesToMerge, Stream outputStream, CancellationToken cancellationToken)
    {
        var streamReaders = await InitializeStreamReaders(filesToMerge);
        await using var outputWriter = new StreamWriter(outputStream, bufferSize: BufferSize);
        foreach (var streamReader in streamReaders)
        {
            while (!streamReader.EndOfStream)
            {
                var value = await streamReader.ReadLineAsync();
                await outputWriter.WriteLineAsync(value.AsMemory(), cancellationToken);
            }
        }

        Cleanup(streamReaders, filesToMerge);
    }

    private async Task<StreamReader[]> InitializeStreamReaders(IReadOnlyList<string> files)
    {
        var streamReaders = new StreamReader[files.Count];
        for (var i = 0; i < files.Count; i++)
        {
            var sortedFilePath = GetFullPath(files[i]);
            var sortedFileStream = File.OpenRead(sortedFilePath);
            streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: BufferSize);
        }

        return streamReaders;
    }

    private void Cleanup(StreamReader[] streamReaders, IReadOnlyList<string> files)
    {
        for (var i = 0; i < streamReaders.Length; i++)
        {
            streamReaders[i].Dispose();
            var temporaryFilename = GetFullPath($"{files[i]}.removal");
            File.Move(GetFullPath(files[i]), temporaryFilename);
            File.Delete(temporaryFilename);
        }
    }
    private void OverwriteTempFile(string from, string to)
    {
        File.Move(GetFullPath(from), GetFullPath(to.Replace(TempFileExtension, string.Empty)), true);
    }

    private string GetFullPath(string filename)
    {
        return Path.Combine(_fileDirectory, Path.GetFileName(filename));
    }
}