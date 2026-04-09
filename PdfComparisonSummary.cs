using FileComparer.Models.Comparer.PdfComparison;

namespace FileComparer
{
    /// <summary>
    /// Prints a unified summary of PDF comparison results from all strategies (text + pixel).
    /// </summary>
    public class PdfComparisonSummary
    {
        private readonly List<PdfComparisonResult> results;

        public PdfComparisonSummary(List<PdfComparisonResult> results)
        {
            this.results = results;
        }

        public void PrintSummary()
        {
            bool allIdentical = results.All(r => r.AreIdentical);
            int textDiffs = results.Where(r => r.StrategyName == "Text").Sum(r => r.PageDifferences.Count);
            int pixelDiffs = results.Where(r => r.StrategyName == "Pixel").Sum(r => r.PageDifferences.Count);

            var textResult = results.FirstOrDefault(r => r.StrategyName == "Text");
            var pixelResult = results.FirstOrDefault(r => r.StrategyName == "Pixel");

            Logger.LogInfo("\n*************************************");
            Logger.LogInfo("*** PDF Comparison Summary        ***");
            Logger.LogInfo("*************************************");

            if (textResult != null)
            {
                Logger.LogInfo($"  File1 pages: {textResult.File1PageCount}");
                Logger.LogInfo($"  File2 pages: {textResult.File2PageCount}");
            }

            if (allIdentical)
            {
                Logger.LogInfo("  Result: Files are IDENTICAL (both text and pixel comparison agree).");
            }
            else
            {
                Logger.LogInfo($"  Result: Files DIFFER");
                Logger.LogInfo($"    Text differences:  {textDiffs} page(s)");
                Logger.LogInfo($"    Pixel differences: {pixelDiffs} page(s)");
            }

            Logger.LogInfo("*************************************");

            if (!allIdentical)
            {
                PrintStrategyDetails("Text", textResult);
                PrintStrategyDetails("Pixel", pixelResult);
            }
        }

        private void PrintStrategyDetails(string strategyName, PdfComparisonResult? result)
        {
            if (result == null || result.PageDifferences.Count == 0)
                return;

            Logger.LogInfo($"\n--- {strategyName} Comparison Details ---");

            int printed = 0;
            foreach (var diff in result.PageDifferences)
            {
                if (printed >= Constants.MaxDifferenceToPrintOnConsole)
                {
                    Logger.LogWarn($"  ... and {result.PageDifferences.Count - printed} more page(s) with {strategyName.ToLower()} differences.");
                    break;
                }

                Logger.LogInfo($"  {diff.Description}");

                if (diff.WhitespaceOnly)
                {
                    Logger.LogWarn("    (difference is whitespace only)");
                }

                if (diff.File1TextDiffs != null && diff.File1TextDiffs.Count > 0)
                {
                    int linesToShow = Math.Min(diff.File1TextDiffs.Count, 5);
                    for (int i = 0; i < linesToShow; i++)
                    {
                        Logger.LogInfo($"    File1: {diff.File1TextDiffs[i]}");
                        if (diff.File2TextDiffs != null && i < diff.File2TextDiffs.Count)
                            Logger.LogInfo($"    File2: {diff.File2TextDiffs[i]}");
                    }
                    if (diff.File1TextDiffs.Count > 5)
                        Logger.LogWarn($"    ... {diff.File1TextDiffs.Count - 5} more differing line(s)");
                }

                if (diff.MismatchPercentage.HasValue)
                {
                    Logger.LogInfo($"    Mismatch: {diff.MismatchPercentage:F2}%");
                }

                if (!string.IsNullOrEmpty(diff.DiffImagePath))
                {
                    Logger.LogInfo($"    Diff image: {diff.DiffImagePath}");
                }

                printed++;
            }
        }
    }
}
