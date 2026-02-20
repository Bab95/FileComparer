using CommandLine;
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
                    sortingContext.Sort(file1Path, sortedFile1);
                    sortingContext.Sort(file2Path, sortedFile2);
                    file1Path = sortedFile1;
                    file2Path = sortedFile2;
                }

                comparer = new DataFileComparer(file1Path, file2Path);
                comparer.Compare(mainObject);
            }
            finally
            {
                CleanupTempFiles(tempFiles);
            }

            await Task.CompletedTask;
        }

        public async Task RunCsvCompare(CompareCsvOptions opts)
        {
            List<string> tempFiles = new List<string>();
            string file1Path = opts.File1InputPath;
            string file2Path = opts.File2InputPath;
            string header1 = null;
            string header2 = null;

            try
            {
                if (opts.Sort)
                {
                    if (opts.IgnoreHeader)
                    {
                        file1Path = StripCsvHeader(file1Path, out header1, tempFiles);
                        file2Path = StripCsvHeader(file2Path, out header2, tempFiles);
                    }

                    sortingContext = new SortingContext(new CsvFileSortingStrategy(Constants.SortChunkSize, opts.Delimiter, opts.SortColumn, opts.Normalize));
                    string sortedFile1 = Path.GetTempFileName();
                    string sortedFile2 = Path.GetTempFileName();
                    tempFiles.Add(sortedFile1);
                    tempFiles.Add(sortedFile2);
                    sortingContext.Sort(file1Path, sortedFile1);
                    sortingContext.Sort(file2Path, sortedFile2);
                    file1Path = sortedFile1;
                    file2Path = sortedFile2;

                    if (opts.IgnoreHeader)
                    {
                        file1Path = PrependCsvHeader(header1, file1Path, tempFiles);
                        file2Path = PrependCsvHeader(header2, file2Path, tempFiles);
                    }
                }

                comparer = new CsvChunkedComparer(file1Path, file2Path, opts.Delimiter, opts.IgnoreHeader, opts.Normalize);
                comparer.Compare(mainObject);
            }
            finally
            {
                CleanupTempFiles(tempFiles);
            }

            await Task.CompletedTask;
        }

        public async Task RunExcelCompare(CompareExcelOptions opts)
        {
            throw new NotImplementedException();
        }

        public async Task RunPdfCompare(ComparePdfOptions opts)
        {
            throw new NotImplementedException();
        }

        private static string NormalizeFile(string inputFilePath, List<string> tempFiles)
        {
            string normalizedPath = Path.GetTempFileName();
            tempFiles.Add(normalizedPath);

            using (var reader = new StreamReader(inputFilePath))
            using (var writer = new StreamWriter(normalizedPath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    writer.WriteLine(NormalizeValue(line));
                }
            }

            return normalizedPath;
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

        private static void CleanupTempFiles(IEnumerable<string> tempFiles)
        {
            foreach (var tempFile in tempFiles)
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                }
            }
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
