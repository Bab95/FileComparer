using FileComparer.Models.Sorting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Compares File in chunks and prints Indexes.
    /// Printing indexes is a heavy process should be used accordingly.
    /// </summary>
    public abstract class ChunkedFileComparer : FileComparer
    {
        public ChunkedFileComparer(string file1Path, string file2Path)
        {
            File1Path = file1Path;
            File2Path = file2Path;
        }

        public SortingContext? SortingContext { get; set; }

        public OutputKind outputKind { get; set; }

        public string OutputPath { get; set; } = string.Empty;
    }
}
