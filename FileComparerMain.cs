using CommandLine;
using FileComparer.Models;
using FileComparer.Models.Comparer;
using FileComparer.Models.Sorting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileComparer
{
    public class FileComparerMain
    {
        private IComparer comparer { get; set; }

        private SortingContext sortingContext { get; set; } // = new SortingContext(Constants.ChunkSize);

        // There's no use of this object. It is required just to match the signature of the methods.
        private object mainObject = new object();

        public static Task ReportCommandArgumentErrors(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
            {
                if (error.GetType() != typeof(HelpRequestedError) &&
                    error.GetType() != typeof(HelpVerbRequestedError))
                {
                    Console.WriteLine(error);
                }
            }

            return Task.CompletedTask;
        }

        public async Task RunDataFileCompare(CompareDataFileOptions opts)
        {
            List<string> tempFiles = new List<string>();
            string file1Path = opts.File1InputPath;
            string file2Path = opts.File2InputPath;

            try
            {
                if (opts.Sort)
                {
                    sortingContext = new SortingContext(new DataFileSortingStrategy(Constants.SortChunkSize));
                    string sortedFile1 = Path.GetTempFileName();
                    string sortedFile2 = Path.GetTempFileName();
                    tempFiles.Add(sortedFile1);
                    tempFiles.Add(sortedFile2);
                    sortingContext.Sort(file1Path, sortedFile1, true);
                    sortingContext.Sort(file2Path, sortedFile2, true);
                    file1Path = sortedFile1;
                    file2Path = sortedFile2;
                }

                comparer = new DataFileComparer(file1Path, file2Path);
                comparer.Compare(mainObject);
            }
            catch(Exception e)
            {
                Logger.LogError($"Exception occured!! {e.Message}");
                Logger.LogError(e.StackTrace == null ? string.Empty : e.StackTrace);
            }

            await Task.CompletedTask;
        }

        public async Task RunCsvCompare(CompareCsvOptions opts)
        {
            string file1Path = opts.File1InputPath;
            string file2Path = opts.File2InputPath;
            
            try
            {
                comparer = new CsvFileComparer(file1Path, file2Path, opts.Delimiter, opts.Sort, opts.Normalize)
                {
                    outputKind = opts.OutPath != null ? OutputKind.FileWriting : OutputKind.OnConsole,
                    OutputPath = opts.OutPath == null ? string.Empty : opts.OutPath,
                    ignoreHeader = opts.IgnoreHeader
                };

                comparer.Compare(mainObject);
            }
            catch(Exception e)
            {
                Logger.LogError("Exception occurred while comparing csv files: " + e.Message);
                Logger.LogError(e.StackTrace == null ? string.Empty : e.StackTrace);
            }

            await Task.CompletedTask;
        }

        public async Task RunExcelCompare(CompareExcelOptions opts)
        {
            throw new NotImplementedException();
        }

        public async Task RunPdfCompare(ComparePdfOptions opts)
        {
            try
            {
                var options = new FileComparer.Models.Comparer.PdfComparison.PdfCompareOptions
                {
                    OutputPath = opts.OutPath
                };

                comparer = new FileComparer.Models.Comparer.PdfComparer(
                    opts.File1InputPath, opts.File2InputPath, options);
                comparer.Compare(mainObject);
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError($"File not found: {e.FileName}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Exception occurred while comparing PDF files: {e.Message}");
                Logger.LogError(e.StackTrace ?? string.Empty);
            }

            await Task.CompletedTask;
        }

        private static string StripCsvHeader(string inputFilePath, out string header, List<string> tempFiles)
        {
            string contentPath = Path.GetTempFileName();
            tempFiles.Add(contentPath);

            using (var reader = new StreamReader(inputFilePath))
            using (var writer = new StreamWriter(contentPath))
            {
                header = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    writer.WriteLine(reader.ReadLine());
                }
            }

            return contentPath;
        }

        private static string PrependCsvHeader(string header, string sortedContentPath, List<string> tempFiles)
        {
            string outputPath = Path.GetTempFileName();
            tempFiles.Add(outputPath);

            using (var writer = new StreamWriter(outputPath))
            {
                if (!string.IsNullOrEmpty(header))
                {
                    writer.WriteLine(header);
                }

                using (var reader = new StreamReader(sortedContentPath))
                {
                    while (!reader.EndOfStream)
                    {
                        writer.WriteLine(reader.ReadLine());
                    }
                }
            }

            return outputPath;
        }

        private static string NormalizeValue(string line)
        {
            return (line ?? string.Empty).Trim();
        }

        public async Task AppMain(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<
                        CompareDataFileOptions,
                        CompareCsvOptions,
                        CompareExcelOptions,
                        ComparePdfOptions
                        >(args).MapResult(
                            (CompareDataFileOptions opts) => RunDataFileCompare(opts),
                            (CompareCsvOptions opts) => RunCsvCompare(opts),
                            (CompareExcelOptions opts) => RunExcelCompare(opts),
                            (ComparePdfOptions opts) => RunPdfCompare(opts),
                        errs => ReportCommandArgumentErrors(errs));
            }
            catch (Exception e)
            {
                Console.WriteLine("Some unknown exception occurred " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
