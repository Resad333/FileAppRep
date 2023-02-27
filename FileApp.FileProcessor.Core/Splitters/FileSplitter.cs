using FileApp.FileProcessor.Core.Abstraction;
using FileApp.FileProcessor.Core.Settings;

namespace FileApp.FileProcessor.Core.Splitters;

public class FileSplitter : IFileSplitter
{
    private readonly SplitSettings _settings;
    private readonly string _fileDirectory;

    public FileSplitter(SplitSettings settings, string fileDirectory)
    {
        _settings = settings;
        _fileDirectory = fileDirectory;
    }

    public async Task<IReadOnlyCollection<string>> SplitFile(string fileName, CancellationToken cancellationToken)
    {
        var sourceStream = File.OpenRead(Path.Combine(_fileDirectory, fileName));
        var fileSize = _settings.FileSize;
        var buffer = new byte[fileSize];
        var extraBuffer = new List<byte>();
        var filenames = new List<string>();
        var totalFiles = Math.Ceiling(sourceStream.Length / (double)fileSize);

        await using (sourceStream)
        {
            var currentFile = 0L;
            while (sourceStream.Position < sourceStream.Length)
            {
                var totalRows = 0;
                var runBytesRead = 0;
                while (runBytesRead < fileSize)
                {
                    var value = sourceStream.ReadByte();
                    if (value == -1)
                    {
                        break;
                    }

                    var byteValue = (byte)value;
                    buffer[runBytesRead] = byteValue;
                    runBytesRead++;
                    if (byteValue == _settings.NewLineSeparator)
                    {
                        totalRows++;
                    }
                }

                var extraByte = buffer[fileSize - 1];

                while (extraByte != _settings.NewLineSeparator)
                {
                    var flag = sourceStream.ReadByte();
                    if (flag == -1)
                    {
                        break;
                    }
                    extraByte = (byte)flag;
                    extraBuffer.Add(extraByte);
                }

                var filename = $"{++currentFile}.unsorted";
                await using var unsortedFile = File.Create(Path.Combine(_fileDirectory, filename));
                await unsortedFile.WriteAsync(buffer.AsMemory(0, runBytesRead), cancellationToken);
                if (extraBuffer.Count > 0)
                {
                    totalRows++;
                    await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count, cancellationToken);
                }

                _settings.ProgressHandler?.Report(currentFile / totalFiles);
                filenames.Add(filename);
                extraBuffer.Clear();
            }

            return filenames;
        }
    }
}