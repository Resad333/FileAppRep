namespace FileApp.FileProcessor.Core.Comparers;
public class FirstWordNumberComparer : IComparer<string>
{
    private readonly int _column;
    private readonly char _firstSeparator;
    private readonly char _secondSeparator;

    public FirstWordNumberComparer(int column, char firstSeparator = '.', char secondSeparator = ' ')
    {
        _column = column;
        _firstSeparator = firstSeparator;
        _secondSeparator = secondSeparator;
    }

    public int Compare(string? x, string? y)
    {
        if (x == null && y != null)
        {
            return -1;
        }

        if (y == null && x != null)
        {
            return 1;
        }

        if (x == null || y == null)
        {
            return 0;
        }

        var xColumn = GetFirstWordFromString(x);
        var yColumn = GetFirstWordFromString(y);
        var result = xColumn.CompareTo(yColumn, StringComparison.OrdinalIgnoreCase);

        if (result == 0)
        {
            var xVal = long.Parse(GetNumber(x));
            var yVal = long.Parse(GetNumber(y));
            if (xVal != yVal)
            {
                result = xVal > yVal ? 1 : -1;
            }
        }

        return result;
    }

    private ReadOnlySpan<char> GetFirstWordFromString(string value)
    {
        var span = value.AsSpan();
        var columnCounter = 1;
        var columnStartIndex = 0;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Equals(_firstSeparator))
            {
                columnCounter++;
                continue;
            }

            if (columnCounter != _column)
            {
                continue;
            }

            columnStartIndex = i + 1;
            break;
        }

        var columnLength = 0;
        var slice = span[columnStartIndex..];
        for (var i = 0; i < slice.Length; i++)
        {
            if (slice[i] != _secondSeparator)
            {
                columnLength++;
            }
            else
            {
                break;
            }
        }

        return span.Slice(columnStartIndex, columnLength);
    }

    private ReadOnlySpan<char> GetNumber(string value)
    {
        var span = value.AsSpan();
        var columnCounter = 1;
        var columnStartIndex = 0;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Equals(_firstSeparator))
            {
                columnCounter++;
                continue;
            }

            if (columnCounter != _column)
            {
                continue;
            }

            columnStartIndex = i;
            break;
        }

        return span.Slice(0, columnStartIndex - 1);
    }
}