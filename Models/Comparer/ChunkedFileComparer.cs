using FileComparer.Models.Sorting;

namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Compares File in chunks and prints Indexes.
    /// Printing indexes is a heavy process should be used accordingly.
    /// </summary>
    public abstract class ChunkedFileComparer : FileComparer
    {
        /// <summary>
        /// Initializes a new instance of the ChunkedFileComparer class with the specified file paths to compare.
        /// </summary>
        /// <param name="file1Path">The path to the first file to be compared. Cannot be null or empty.</param>
        /// <param name="file2Path">The path to the second file to be compared. Cannot be null or empty.</param>
        public ChunkedFileComparer(string file1Path, string file2Path)
        {
            File1Path = file1Path;
            File2Path = file2Path;
        }

        /// <summary>
        /// Gets or sets the sorting context used to determine the order of items.
        /// </summary>
        /// <remarks>Assign a value to specify custom sorting behavior. If not set, the default sorting
        /// logic will be applied. The property can be null, indicating no explicit sorting context is
        /// provided.</remarks>
        public SortingContext? SortingContext { get; set; }

        /// <summary>
        /// output kind to record output
        /// </summary>
        public OutputKind outputKind { get; set; }

        /// <summary>
        /// Gets or sets the file system path where output files are written.
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;
    }
}
