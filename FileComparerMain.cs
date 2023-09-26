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

        // There's no use of this object. It is required just to match the signature of the methods.
        private object mainObject = new object();

        public async Task GetIndexOptions(GetDifferenceIndexOption opts)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _comparer = new ChunkedFileComparer(opts.File1InputPath, opts.File2InputPath)
            {
                OutPath = opts.OutPath,
                outputKind = OutputKind.FileWriting
            };

            if (File.Exists(opts.OutPath))
            {
                Console.WriteLine("Output path already exits! File will be overwritten.....");
            }

            ChunkedFileComparer.printIndexes = true;

            ChunkedFileComparer.countOfActiveWorker++;

            ThreadPool.SetMaxThreads(Constants.MaxThreadsCount, Constants.MaxThreadsCount);
            
            ThreadPool.QueueUserWorkItem(new WaitCallback(_comparer.Compare), mainObject);

            
            object obj = new object();

            object[] currentDiff;

            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            
            int totalDiffCount = 0;
            
            while (ChunkedFileComparer.countOfActiveWorker > 0)
            {
                totalDiffCount++;
                using (StreamWriter writer = new StreamWriter(opts.OutPath, false))
                {
                    ChunkedFileComparer.FileDifferences.TryDequeue(out currentDiff);
                    await writer.WriteLineAsync(currentDiff.ToString());
                }
            }

            (_comparer as ChunkedFileComparer).summary.noOfDifferences = totalDiffCount;

            Console.WriteLine((_comparer as ChunkedFileComparer).summary.ToString());

            watch.Stop();
        }

        public async Task GetLinesOption(GetDifferentLinesOption opts)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _comparer = new ChunkedFileComparer(opts.File1InputPath, opts.File2InputPath)
            {
                outputKind = opts.OutPath == null ? OutputKind.OnConsole : OutputKind.FileWriting,
                OutPath = opts.OutPath
            };

            ChunkedFileComparer.printIndexes = false;

            ChunkedFileComparer.countOfActiveWorker++;

            ThreadPool.SetMaxThreads(Constants.MaxThreadsCount, Constants.MaxThreadsCount);

            ThreadPool.QueueUserWorkItem(new WaitCallback(_comparer.Compare), mainObject);


            object obj = new object();

            //object[] currentDiff;

            //Thread.CurrentThread.Priority = ThreadPriority.Lowest;

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
            Console.WriteLine($"Total Time Taken: {watch.ElapsedMilliseconds}");
        }

        public async Task GetFilesParityOption(GetFileParityOption opts)
        {
            _comparer = new SequentialFileComparer(opts.File1InputPath, opts.File2InputPath)
            {
                printDiffs = opts.PrintTopDiffs
            };

            ChunkedFileComparer.countOfActiveWorker++;
            _comparer.Compare(mainObject);
        }

        public static async Task ReportCommandArgumentErrors(IEnumerable<Error> err)
        {
            foreach (var error in err)
            {
                Console.WriteLine(err);
            }
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
            }
        }
    }
}
