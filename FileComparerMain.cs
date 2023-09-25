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

        public async Task GetIndexOptions(GetDifferenceIndexOption opts)
        {

        }

        public async Task GetLinesOption(GetDifferentLinesOption opts)
        {
            _comparer = new ChunkedFileComparer(opts.File1InputPath, opts.File2InputPath)
            {
                outputKind = opts.OutPath == null ? OutputKind.OnConsole : OutputKind.FileWriting,
                OutPath = opts.OutPath
            };

            ChunkedFileComparer.printIndexes = false;

        }

        public async Task GetFilesParityOption(GetFileParityOption opts)
        {
            _comparer = new SequentialFileComparer(opts.File1InputPath, opts.File2InputPath)
            {
                printDiffs = opts.PrintTopDiffs
            };

            _comparer.Compare();
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
