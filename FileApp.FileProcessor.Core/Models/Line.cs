namespace FileApp.FileProcessor.Core.Models;

readonly struct Line
{
    public string Value { get; init; }
    public int StreamReaderIndex { get; init; }
}