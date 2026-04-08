using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FileComparer.Models.Comparer.PdfComparison
{
    /// <summary>
    /// Compares two PDFs by rendering each page to a bitmap at 300 DPI and performing
    /// pixel-by-pixel comparison. Uses Docnet.Core (MuPDF) for high-fidelity rendering.
    /// </summary>
    public class PdfPixelComparisonStrategy : IPdfComparisonStrategy
    {
        private const int RenderDpi = 300;

        public string StrategyName => "Pixel";

        public PdfComparisonResult Compare(string file1Path, string file2Path, PdfCompareOptions options)
        {
            var result = new PdfComparisonResult { StrategyName = StrategyName };

            using var library = DocLib.Instance;
            using var doc1 = library.GetDocReader(file1Path, new PageDimensions(RenderDpi));
            using var doc2 = library.GetDocReader(file2Path, new PageDimensions(RenderDpi));

            result.File1PageCount = doc1.GetPageCount();
            result.File2PageCount = doc2.GetPageCount();

            int minPages = Math.Min(result.File1PageCount, result.File2PageCount);
            int maxPages = Math.Max(result.File1PageCount, result.File2PageCount);

            if (result.File1PageCount != result.File2PageCount)
            {
                result.AreIdentical = false;
            }

            for (int i = 0; i < minPages; i++)
            {
                var diff = ComparePages(doc1, doc2, i, options);
                if (diff != null)
                {
                    result.AreIdentical = false;
                    result.PageDifferences.Add(diff);
                }
            }

            // Report extra pages
            for (int i = minPages; i < maxPages; i++)
            {
                string extraInFile = result.File1PageCount > result.File2PageCount ? "File1" : "File2";
                result.PageDifferences.Add(new PdfPageDifference
                {
                    PageNumber = i + 1,
                    DifferenceType = "Pixel",
                    Description = $"Page {i + 1} exists only in {extraInFile}.",
                    MismatchPercentage = 100.0
                });
            }

            return result;
        }

        private static PdfPageDifference? ComparePages(IDocReader doc1, IDocReader doc2, int pageIndex, PdfCompareOptions options)
        {
            using var page1 = doc1.GetPageReader(pageIndex);
            using var page2 = doc2.GetPageReader(pageIndex);

            int w1 = page1.GetPageWidth();
            int h1 = page1.GetPageHeight();
            int w2 = page2.GetPageWidth();
            int h2 = page2.GetPageHeight();

            byte[] pixels1 = page1.GetImage();
            byte[] pixels2 = page2.GetImage();

            if (w1 != w2 || h1 != h2)
            {
                string? diffImagePath = GenerateDiffImage(pixels1, w1, h1, pixels2, w2, h2, pageIndex, options);
                return new PdfPageDifference
                {
                    PageNumber = pageIndex + 1,
                    DifferenceType = "Pixel",
                    Description = $"Page {pageIndex + 1}: dimensions differ ({w1}x{h1} vs {w2}x{h2}).",
                    MismatchPercentage = 100.0,
                    DiffImagePath = diffImagePath
                };
            }

            // BGRA format: 4 bytes per pixel
            int totalPixels = w1 * h1;
            int mismatchCount = 0;

            for (int p = 0; p < pixels1.Length; p += 4)
            {
                if (p + 3 >= pixels1.Length || p + 3 >= pixels2.Length)
                    break;

                // Compare BGR channels (ignore alpha)
                if (pixels1[p] != pixels2[p] ||
                    pixels1[p + 1] != pixels2[p + 1] ||
                    pixels1[p + 2] != pixels2[p + 2])
                {
                    mismatchCount++;
                }
            }

            double mismatchPercent = (double)mismatchCount / totalPixels * 100.0;

            if (mismatchCount == 0)
                return null;

            string? imagePath = GenerateDiffImage(pixels1, w1, h1, pixels2, w2, h2, pageIndex, options);
            return new PdfPageDifference
            {
                PageNumber = pageIndex + 1,
                DifferenceType = "Pixel",
                Description = $"Page {pageIndex + 1}: {mismatchPercent:F2}% pixels differ ({mismatchCount}/{totalPixels}).",
                MismatchPercentage = mismatchPercent,
                DiffImagePath = imagePath
            };
        }

        /// <summary>
        /// Generates a diff image highlighting pixel differences. Changed pixels are shown in red.
        /// Only generates if an output path is configured.
        /// </summary>
        private static string? GenerateDiffImage(byte[] pixels1, int w1, int h1, byte[] pixels2, int w2, int h2, int pageIndex, PdfCompareOptions options)
        {
            if (string.IsNullOrEmpty(options.OutputPath))
                return null;

            int width = Math.Min(w1, w2);
            int height = Math.Min(h1, h2);

            using var diffBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var bmpData = diffBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            byte[] diffPixels = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx1 = (y * w1 + x) * 4;
                    int idx2 = (y * w2 + x) * 4;
                    int outIdx = (y * width + x) * 4;

                    bool same = idx1 + 3 < pixels1.Length && idx2 + 3 < pixels2.Length &&
                                pixels1[idx1] == pixels2[idx2] &&
                                pixels1[idx1 + 1] == pixels2[idx2 + 1] &&
                                pixels1[idx1 + 2] == pixels2[idx2 + 2];

                    if (same)
                    {
                        // Dimmed original pixel
                        diffPixels[outIdx] = (byte)(pixels1[idx1] / 2);
                        diffPixels[outIdx + 1] = (byte)(pixels1[idx1 + 1] / 2);
                        diffPixels[outIdx + 2] = (byte)(pixels1[idx1 + 2] / 2);
                        diffPixels[outIdx + 3] = 255;
                    }
                    else
                    {
                        // Red highlight for differences
                        diffPixels[outIdx] = 0;       // B
                        diffPixels[outIdx + 1] = 0;   // G
                        diffPixels[outIdx + 2] = 255;  // R
                        diffPixels[outIdx + 3] = 255;  // A
                    }
                }
            }

            Marshal.Copy(diffPixels, 0, bmpData.Scan0, diffPixels.Length);
            diffBitmap.UnlockBits(bmpData);

            string outputDir = options.OutputPath!;
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string filePath = Path.Combine(outputDir, $"diff_page_{pageIndex + 1}.png");
            diffBitmap.Save(filePath, ImageFormat.Png);

            return filePath;
        }
    }
}
