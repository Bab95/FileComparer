using FileComparer.Models.Comparer.PdfComparison;

namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Compares two PDF files using both text extraction and pixel-level comparison strategies.
    /// Always runs both strategies internally and merges results for maximum accuracy.
    /// </summary>
    public class PdfComparer : FileComparer
    {
        private readonly PdfCompareOptions options;
        private readonly List<IPdfComparisonStrategy> strategies;

        public PdfComparisonResult? TextResult { get; private set; }
        public PdfComparisonResult? PixelResult { get; private set; }

        public PdfComparer(string file1Path, string file2Path, PdfCompareOptions options)
        {
            File1Path = file1Path;
            File2Path = file2Path;
            this.options = options;
            strategies = new List<IPdfComparisonStrategy>
            {
                new PdfTextComparisonStrategy(),
                new PdfPixelComparisonStrategy()
            };
        }

        /// <summary>
        /// Runs both text and pixel comparison strategies, merges results, and sets summary.
        /// </summary>
        public override void Compare(object obj)
        {
            if (!File.Exists(File1Path))
                throw new FileNotFoundException("File not found.", File1Path);
            if (!File.Exists(File2Path))
                throw new FileNotFoundException("File not found.", File2Path);

            var results = new List<PdfComparisonResult>();

            foreach (var strategy in strategies)
            {
                Logger.LogInfo($"Running {strategy.StrategyName} comparison...");
                var result = strategy.Compare(File1Path, File2Path, options);
                results.Add(result);
                Logger.LogInfo($"{strategy.StrategyName} comparison complete. Identical: {result.AreIdentical}");
            }

            TextResult = results.FirstOrDefault(r => r.StrategyName == "Text");
            PixelResult = results.FirstOrDefault(r => r.StrategyName == "Pixel");

            AreFilesSame = results.All(r => r.AreIdentical);

            var pdfSummary = new PdfComparisonSummary(results);
            pdfSummary.PrintSummary();

            int totalDiffs = results.Sum(r => r.PageDifferences.Count);
            summary = new Summary(totalDiffs);
        }
    }
}
