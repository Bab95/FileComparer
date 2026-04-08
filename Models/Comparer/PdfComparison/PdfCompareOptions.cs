namespace FileComparer.Models.Comparer.PdfComparison
{
    /// <summary>
    /// Internal options for PDF comparison strategies.
    /// </summary>
    public class PdfCompareOptions
    {
        /// <summary>
        /// Optional output path to write diff images or detailed reports.
        /// </summary>
        public string? OutputPath { get; set; }
    }
}
