using FileApp.FileProcessor.Core.Common;
using FileApp.FileProcessor.Core.Settings;

/*
 1. Create directory on local machine. ex: @"C:\temp\files"
 2. Put any file to the directory. ex: "UnortedFakeDataFile_ced8129e040e4470974920463b164d0e.txt"
 */
var fileDirectory = @"C:\temp\files";//if needed modify this for testing your local
var fileName = "UnortedFakeDataFile_d28b8d9906ec4e549e45fbbed163730e.txt";//modify this for testing your local

var comparer = Factory.CreateComparer("FirstWordNumberComparer");
var settings = new FileProcessorSettings
{
    FileDirectory = fileDirectory,
    Split = new SplitSettings
    {
        ProgressHandler = new Progress<double>(x =>
        {
            var percentage = x * 100;
            Console.WriteLine($"File split progress: {percentage:##.##}%");
        })
    },
    Sort = new SortSettings
    {
        Comparer = comparer,
        ProgressHandler = new Progress<double>(x =>
        {
            var percentage = x * 100;
            Console.WriteLine($"File sort progress: {percentage:##.##}%");
        })
    },
    Merge = new MergeSettings
    {
        ProgressHandler = new Progress<double>(x =>
        {
            var percentage = x * 100;
            Console.WriteLine($"File merge progress: {percentage:##.##}%");
        })
    }
};

var fileProcessor = Factory.CreateFileProcessor(settings);

await fileProcessor.Process(fileName);


Console.ReadLine();