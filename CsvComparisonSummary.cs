using FileComparer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer
{
    public class CsvComparisonSummary
    {
        private OutputKind outputKind { get; }
        
        private string outpath { get; set; } = string.Empty;

        private ConcurrentQueue<Pair<CsvRecord, CsvRecord>> recordDifferences;
        
        public CsvComparisonSummary(ConcurrentQueue<Pair<CsvRecord, CsvRecord>> recordDifferences, OutputKind outputKind) 
        {
            this.recordDifferences = recordDifferences;
            this.outputKind = outputKind;
        }

        public void PrintSummary()
        {
            Logger.LogInfo  ("************************");
            Logger.LogInfo  ("*Comparison Summary:    *");
            Logger.LogError($"*Total Differences:   {recordDifferences.Count}*");
            Logger.LogInfo ($"*Output Kind: {outputKind} *");
            Logger.LogInfo  ("************************");
            if (outputKind == OutputKind.OnConsole) {
                if (recordDifferences.Count > Constants.MaxDifferenceToPrintOnConsole)
                {
                    Logger.LogWarn("Too many differences to display on console. Only showing the first few differences.");
                }
                int count = 0;
                while (count < Constants.MaxDifferenceToPrintOnConsole &&recordDifferences.TryDequeue(out var difference))
                {
                    count++;
                    var firstRecord = difference.First;
                    var secondRecord = difference.Second;
                    //Logger.LogInfo("========================================");
                    Logger.LogInfo($"File1: {firstRecord.ToString()}");
                    secondRecord.PrintWithDifferences(firstRecord);
                    Logger.LogInfo("========================================================================================");
                }
            }
            else
            {
                Logger.LogError($"Differences would be Written to {outpath}");
            }
        }
    }
}
