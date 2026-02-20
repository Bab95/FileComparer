using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    /// <summary>
    /// ChunkData is a class that encapsulates a chunk of data from two files being compared. 
    /// It holds the lines of text from both files for a specific chunk, along with the line number where the chunk starts. This class is used to facilitate the comparison process by allowing the program to work with manageable pieces of data, especially when dealing with large files that cannot be loaded entirely into memory. The Lines1 and Lines2 properties store the lines from the first and second files, respectively, while the LineNumber property indicates the starting line number of the chunk in the original files.
    /// </summary>
    public class ChunkData
    {
        /// <summary>
        /// Lines1 is a list of strings representing the lines of text from the first file for the current chunk. 
        /// Each string in the list corresponds to a line of text, and the collection allows for easy access and manipulation of the data during the comparison process.
        /// </summary>
        public List<string> Lines1 { get; }
        
        /// <summary>
        /// Lines2 is a list of strings representing the lines of text from the second file for the current chunk. 
        /// Each string in the list corresponds to a line of text, and the collection allows for easy access and manipulation of the data during the comparison process.
        /// </summary>
        public List<string> Lines2 { get; }

        /// <summary>
        /// LineNumber indicates the starting line number of the chunk in the original files.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChunkData class with the specified lines from both files and the starting line number.
        /// </summary>
        /// <param name="lines1">The lines from the first file for the current chunk.</param>
        /// <param name="lines2">The lines from the second file for the current chunk.</param>
        /// <param name="lineNumber">The starting line number of the chunk in the original files.</param>
        public ChunkData(List<string> lines1, List<string> lines2, int lineNumber)
        {
            Lines1 = lines1;
            Lines2 = lines2;
            LineNumber = lineNumber;
        }
    }
}
