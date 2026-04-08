namespace FileComparer.Models.Comparer.PdfComparison
{
    /// <summary>
    /// Defines a strategy for comparing two PDF files.
    /// Implementations provide different comparison approaches (text, pixel, etc.).
    /// </summary>
    public interface IPdfComparisonStrategy
    {
        /// <summary>
        /// Gets the human-readable name of this comparison strategy.
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Compares two PDF files and returns a detailed result.
        /// </summary>
        /// <param name="file1Path">Path to the first PDF file.</param>
        /// <param name="file2Path">Path to the second PDF file.</param>
        /// <param name="options">Comparison options (DPI, threshold, whitespace handling).</param>
        /// <returns>A result describing differences found by this strategy.</returns>
        PdfComparisonResult Compare(string file1Path, string file2Path, PdfCompareOptions options);
    }
}
