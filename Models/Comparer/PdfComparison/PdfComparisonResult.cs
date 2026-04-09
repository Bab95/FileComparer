namespace FileComparer.Models.Comparer.PdfComparison
{
    /// <summary>
    /// Represents the result of a PDF comparison strategy for a single run.
    /// </summary>
    public class PdfComparisonResult
    {
        /// <summary>
        /// The name of the strategy that produced this result.
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;

        /// <summary>
        /// Whether both files are considered identical by this strategy.
        /// </summary>
        public bool AreIdentical { get; set; } = true;

        /// <summary>
        /// Number of pages in the first PDF.
        /// </summary>
        public int File1PageCount { get; set; }

        /// <summary>
        /// Number of pages in the second PDF.
        /// </summary>
        public int File2PageCount { get; set; }

        /// <summary>
        /// Per-page differences found by this strategy.
        /// </summary>
        public List<PdfPageDifference> PageDifferences { get; set; } = new();
    }

    /// <summary>
    /// Describes a difference found on a specific page of the PDF.
    /// </summary>
    public class PdfPageDifference
    {
        /// <summary>
        /// The 1-based page number where the difference was found.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The type of difference: "Text" or "Pixel".
        /// </summary>
        public string DifferenceType { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable description of the difference.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// For text comparison: whether the difference is only in whitespace.
        /// </summary>
        public bool WhitespaceOnly { get; set; }

        /// <summary>
        /// For pixel comparison: the percentage of pixels that differ (0.0 to 100.0).
        /// </summary>
        public double? MismatchPercentage { get; set; }

        /// <summary>
        /// For pixel comparison: path to the diff image file, if generated.
        /// </summary>
        public string? DiffImagePath { get; set; }

        /// <summary>
        /// For text comparison: the text lines that differ in file 1.
        /// </summary>
        public List<string>? File1TextDiffs { get; set; }

        /// <summary>
        /// For text comparison: the text lines that differ in file 2.
        /// </summary>
        public List<string>? File2TextDiffs { get; set; }
    }
}
