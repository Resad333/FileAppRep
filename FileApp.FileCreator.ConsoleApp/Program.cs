using FileApp.FileCreator.Core.Common;

//Create any directory which will contain created file on local machine and set it here
var fileDirectory = @"C:\temp\files";
//Set size of file here . ex: 1 GB = 2 * 1024 MB = 2 * 1024 * 1024 KB
var fileSizeInKB = 1 * 1024 * 1024;

//FileCreatorTypeQueueImplementation will be used and works faster.
var fileCreator = Factory.CreateFileCreator(fileDirectory, FileCreatorType.QueueImplementation);

/*  var fileCreator = Factory.CreateFileCreator(fileDirectory, FileCreatorType.ChunkerImplementation);
 *  FileCreator chunker based implementation can be useful if file will be updated/changed
 */

await fileCreator.Create(fileSizeInKB);

Console.ReadLine();