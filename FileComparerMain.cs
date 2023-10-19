using CommandLine;
using FileComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer
{
    public class FileComparerMain
    {
        private IComparer _comparer { get; set; }
        private SortingContext _sortingContext { get; set; } = new SortingContext(Constants.ChunkSize);

        // There's no use of this object. It is required just to match the signature of the methods.
        private object mainObject = new object();

        private bool IsSortingRequired(Options opts)
        {
            if (opts.sort != null && opts.sort == true)
            {
                return true;
            }

            return false;
        }
        private string GetOutPath(string inputPath)
        {
            string baseTempDirectory = AppDomain.CurrentDomain.BaseDirectory + "temp_sorted";
            if (!Directory.Exists(baseTempDirectory)) 
            {
                Directory.CreateDirectory(baseTempDirectory);
            }
            string fileName = Path.GetFileName(inputPath);
            return Path.Combine(baseTempDirectory, fileName);
        }

        public async Task GetIndexOptions(GetDifferenceIndexOption opts)
        {
            string file1InputPath = opts.File1InputPath;
            string file2InputPath = opts.File2InputPath;


            if (IsSortingRequired(opts) ) 
            {
                var _sorting_watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("Starting Sorting File1..........");
                string file1outPath = GetOutPath(file1InputPath);
                _sortingContext.SortLargeFileParallel(file1InputPath, file1outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 1 :{_sorting_watch.ElapsedMilliseconds} milliseconds");
                _sorting_watch.Reset();

                _sorting_watch.Start();
                Console.WriteLine("Starting Sorting File2..........");
                string file2outPath = GetOutPath(file2InputPath);
                _sortingContext.SortLargeFileParallel(file2InputPath, file2outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 2 :{_sorting_watch.ElapsedMilliseconds} milliseconds");

                file1InputPath = file1outPath;
                file2InputPath= file2outPath;
            }

            _comparer = new ChunkedFileComparer(file1InputPath, file2InputPath)
            {
                OutPath = opts.OutPath,
                outputKind = OutputKind.FileWriting
            };

            opts.OutPath = Utils.CheckOuputFileStatus(opts.OutPath);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ChunkedFileComparer.printIndexes = true;

            ChunkedFileComparer.countOfActiveWorker++;

            ThreadPool.SetMaxThreads(Constants.MaxThreadsCount, Constants.MaxThreadsCount);
            
            ThreadPool.QueueUserWorkItem(new WaitCallback(_comparer.Compare), mainObject);

            
            object obj = new object();

            string currentDiff = null;

            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            
            int totalDiffCount = 0;
            
            while (ChunkedFileComparer.countOfActiveWorker > 0 || ChunkedFileComparer.FileDifferences.TryDequeue(out currentDiff))
            {
                using (StreamWriter writer = new StreamWriter(opts.OutPath, true))
                {
                    if (currentDiff != null)
                    {
                        totalDiffCount++;
                        await writer.WriteLineAsync(currentDiff);
                    }
                }
            }

            (_comparer as ChunkedFileComparer).summary.noOfDifferences = totalDiffCount;

            Console.WriteLine((_comparer as ChunkedFileComparer).summary.ToString());

            watch.Stop();
            Console.WriteLine($"Total Time Taken in Milliseconds: {watch.ElapsedMilliseconds}");
        }

        public async Task GetLinesOption(GetDifferentLinesOption opts)
        {
            string file1InputPath = opts.File1InputPath;
            string file2InputPath = opts.File2InputPath;


            if (IsSortingRequired(opts))
            {
                var _sorting_watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("Starting Sorting File1..........");
                string file1outPath = GetOutPath(file1InputPath);
                _sortingContext.SortLargeFileParallel(file1InputPath, file1outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 1 :{_sorting_watch.ElapsedMilliseconds} milliseconds");
                _sorting_watch.Reset();

                _sorting_watch.Start();
                Console.WriteLine("Starting Sorting File2..........");
                string file2outPath = GetOutPath(file2InputPath);
                _sortingContext.SortLargeFileParallel(file2InputPath, file2outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 2 :{_sorting_watch.ElapsedMilliseconds} milliseconds");

                file1InputPath = file1outPath;
                file2InputPath = file2outPath;
            }

            _comparer = new ChunkedFileComparer(file1InputPath, file2InputPath)
            {
                OutPath = opts.OutPath,
                outputKind = OutputKind.FileWriting
            };

            opts.OutPath = Utils.CheckOuputFileStatus(opts.OutPath);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ChunkedFileComparer.printIndexes = false;

            ChunkedFileComparer.countOfActiveWorker++;

            ThreadPool.SetMaxThreads(Constants.MaxThreadsCount, Constants.MaxThreadsCount);

            ThreadPool.QueueUserWorkItem(new WaitCallback(_comparer.Compare), mainObject);


            object obj = new object();

            int totalDiffCount = 0;
            int currentLine = 0;
            while (ChunkedFileComparer.countOfActiveWorker > 0 || ChunkedFileComparer.LineDifferences.TryDequeue(out currentLine))
            {
                if (opts.OutPath == null)
                {
                    if (currentLine != 0)
                    {
                        totalDiffCount++;
                        Console.WriteLine(currentLine);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(opts.OutPath, true))
                    {
                        if (currentLine != 0)
                        {
                            totalDiffCount++;
                            await writer.WriteLineAsync(currentLine.ToString());
                        }
                    }
                }
            }
            (_comparer as ChunkedFileComparer).summary.noOfDifferences = totalDiffCount;
            Console.WriteLine((_comparer as ChunkedFileComparer).summary.ToString());
            watch.Stop();
            Console.WriteLine($"Total Time Taken in Milliseconds: {watch.ElapsedMilliseconds}");
        }

        public async Task GetFilesParityOption(GetFileParityOption opts)
        {
            string file1InputPath = opts.File1InputPath;
            string file2InputPath = opts.File2InputPath;


            if (IsSortingRequired(opts))
            {
                var _sorting_watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("Starting Sorting File1..........");
                string file1outPath = GetOutPath(file1InputPath);
                _sortingContext.SortLargeFileParallel(file1InputPath, file1outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 1 :{_sorting_watch.ElapsedMilliseconds} milliseconds");
                _sorting_watch.Reset();

                _sorting_watch.Start();
                Console.WriteLine("Starting Sorting File2..........");
                string file2outPath = GetOutPath(file2InputPath);
                _sortingContext.SortLargeFileParallel(file2InputPath, file2outPath, Constants.ChunkSize);
                _sorting_watch.Stop();
                Console.WriteLine($"Total Time taken in sorting file 2 :{_sorting_watch.ElapsedMilliseconds} milliseconds");

                file1InputPath = file1outPath;
                file2InputPath = file2outPath;
            }

            _comparer = new SequentialFileComparer(file1InputPath, file2InputPath)
            {
                printDiffs = opts.PrintTopDiffs
            };

            ChunkedFileComparer.countOfActiveWorker++;
            _comparer.Compare(mainObject);
        }

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


        public async Task AppMain(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<
                    GetDifferenceIndexOption,
                    GetDifferentLinesOption,
                    GetFileParityOption>(args)
                  .MapResult(
                    (GetDifferenceIndexOption opts) => GetIndexOptions(opts),
                    (GetDifferentLinesOption opts) => GetLinesOption(opts),
                    (GetFileParityOption opts) => GetFilesParityOption(opts),
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
