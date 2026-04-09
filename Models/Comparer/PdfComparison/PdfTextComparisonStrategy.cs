using UglyToad.PdfPig;

namespace FileComparer.Models.Comparer.PdfComparison
{
    /// <summary>
    /// Compares two PDFs by extracting text from each page and performing line-by-line text comparison.
    /// Uses PdfPig for accurate text extraction.
    /// </summary>
    public class PdfTextComparisonStrategy : IPdfComparisonStrategy
    {
        public string StrategyName => "Text";

        public PdfComparisonResult Compare(string file1Path, string file2Path, PdfCompareOptions options)
        {
            var result = new PdfComparisonResult { StrategyName = StrategyName };

            using var doc1 = PdfDocument.Open(file1Path);
            using var doc2 = PdfDocument.Open(file2Path);

            result.File1PageCount = doc1.NumberOfPages;
            result.File2PageCount = doc2.NumberOfPages;

            int minPages = Math.Min(doc1.NumberOfPages, doc2.NumberOfPages);
            int maxPages = Math.Max(doc1.NumberOfPages, doc2.NumberOfPages);

            if (doc1.NumberOfPages != doc2.NumberOfPages)
            {
                result.AreIdentical = false;
            }

            for (int i = 1; i <= minPages; i++)
            {
                var page1Text = doc1.GetPage(i).Text;
                var page2Text = doc2.GetPage(i).Text;

                if (!string.Equals(page1Text, page2Text, StringComparison.Ordinal))
                {
                    result.AreIdentical = false;
                    var diff = BuildTextDiff(page1Text, page2Text, i);
                    result.PageDifferences.Add(diff);
                }
            }

            // Report extra pages in the longer document
            for (int i = minPages + 1; i <= maxPages; i++)
            {
                string extraInFile = doc1.NumberOfPages > doc2.NumberOfPages ? "File1" : "File2";
                result.PageDifferences.Add(new PdfPageDifference
                {
                    PageNumber = i,
                    DifferenceType = "Text",
                    Description = $"Page {i} exists only in {extraInFile}."
                });
            }

            return result;
        }

        private static PdfPageDifference BuildTextDiff(string text1, string text2, int pageNumber)
        {
            // Check if the difference is whitespace-only
            bool isWhitespaceOnly = NormalizeWhitespace(text1) == NormalizeWhitespace(text2);

            var lines1 = text1.Split('\n');
            var lines2 = text2.Split('\n');

            var diffLines1 = new List<string>();
            var diffLines2 = new List<string>();

            int maxLines = Math.Max(lines1.Length, lines2.Length);
            for (int i = 0; i < maxLines; i++)
            {
                string l1 = i < lines1.Length ? lines1[i] : string.Empty;
                string l2 = i < lines2.Length ? lines2[i] : string.Empty;

                if (!string.Equals(l1, l2, StringComparison.Ordinal))
                {
                    diffLines1.Add($"L{i + 1}: {l1}");
                    diffLines2.Add($"L{i + 1}: {l2}");
                }
            }

            string description = isWhitespaceOnly
                ? $"Page {pageNumber}: {diffLines1.Count} line(s) differ (whitespace only)."
                : $"Page {pageNumber}: {diffLines1.Count} line(s) differ.";

            return new PdfPageDifference
            {
                PageNumber = pageNumber,
                DifferenceType = "Text",
                Description = description,
                WhitespaceOnly = isWhitespaceOnly,
                File1TextDiffs = diffLines1,
                File2TextDiffs = diffLines2
            };
        }

        private static string NormalizeWhitespace(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");
        }
    }
}
