using FileComparer.Models;
using System.Collections.Concurrent;

namespace FileComparer
{
    /// <summary>
    /// Represents a summary of differences found during a CSV file comparison, including the output format and the
    /// collection of differing records.
    /// </summary>
    /// <remarks>Use this class to access and display the results of a CSV comparison operation. The summary
    /// includes the total number of differences and supports output to the console or to a file, depending on the
    /// specified output kind. This class is intended for scenarios where a concise overview of comparison results is
    /// needed.</remarks>
    public class CsvComparisonSummary
    {
        /// <summary>
        /// Gets the output kind used by the current instance.
        /// </summary>
        private OutputKind outputKind { get; }
        
        /// <summary>
        /// Gets or sets the output file path used by the operation.
        /// </summary>
        private string outpath { get; set; } = string.Empty;

        /// <summary>
        /// Represents a thread-safe queue containing pairs of CSV records that differ from each other.
        /// </summary>
        private ConcurrentQueue<Pair<CsvRecord, CsvRecord>> recordDifferences;
        
        /// <summary>
        /// Initializes a new instance of the CsvComparisonSummary class with the specified record differences and
        /// output kind.
        /// </summary>
        /// <param name="recordDifferences">A thread-safe queue containing pairs of CsvRecord objects that represent differences found during CSV
        /// comparison. Must not be null.</param>
        /// <param name="outputKind">The output format to use when presenting the comparison summary.</param>
        /// <param name="outputPath">the output path to where summary should be written.
        public CsvComparisonSummary(ConcurrentQueue<Pair<CsvRecord, CsvRecord>> recordDifferences, OutputKind outputKind, string outputPath) 
        {
            this.recordDifferences = recordDifferences;
            this.outputKind = outputKind;
            this.outpath = outputPath;
        }

        /// <summary>
        /// Prints a summary of the comparison results, including the total number of differences and the output
        /// destination.
        /// </summary>
        /// <remarks>If the output kind is set to console and the number of differences exceeds the
        /// maximum allowed for console display, only the first few differences are shown and a warning is logged.
        /// Otherwise, the differences are written to the specified output path. This method is intended for
        /// informational purposes and does not return any values.</remarks>
        public void PrintSummary()
        {
            int d = recordDifferences.Count;

            Logger.LogInfo  ("\n*************************\n" +
                             "***Comparison Summary: **\n" +
                             $"***Total Differences: {d}**\n" +
                             $"**Output Kind:{outputKind}**\n" +
                             "*************************");

            if (outputKind == OutputKind.OnConsole) {
                if (recordDifferences.Count > Constants.MaxDifferenceToPrintOnConsole)
                {
                    Logger.LogWarn("Too many differences to display on console. Only showing the first few differences.");
                }

                int count = 0;
                while (count < Constants.MaxDifferenceToPrintOnConsole && recordDifferences.TryDequeue(out var difference))
                {
                    count++;
                    var firstRecord = difference.First;
                    var secondRecord = difference.Second;
                    firstRecord.PrintWithDifferences(secondRecord, FileName.File1);
                    secondRecord.PrintWithDifferences(firstRecord, FileName.File2);
                    Logger.LogInfo("========================================================================================");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(outpath))
                {
                    Logger.LogError($"Output path is not set. Differences were not written to file.");
                    return;
                }

                using var writer = new StreamWriter(outpath, false);
                while (recordDifferences.TryDequeue(out var difference))
                {
                    var firstRecord = difference.First;
                    var secondRecord = difference.Second;
                    writer.WriteLine($"File1: {firstRecord.ToString()}");
                    writer.WriteLine( $"File2: {secondRecord.ToString()}");
                    writer.WriteLine("==================================================");
                }

                Logger.LogInfo($"Differences were written to {outpath}");
            }
        }
    }
}
