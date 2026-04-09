namespace FileComparer.Models
{
    /// <summary>
    /// Represents a summary of file comparison results, including the file name, line number, and the number of
    /// differences found.
    /// </summary>
    /// <remarks>Use this class to encapsulate information about differences detected between files, such as
    /// the specific file involved, the line number up to which differences were found, and the total count of
    /// differences. The class provides multiple constructors to allow flexible initialization depending on the
    /// available information. The ToString method returns a formatted description of the comparison outcome, which can
    /// be used for reporting or logging purposes.</remarks>
    public class Summary
    {
        /// <summary>
        /// Gets or sets the file name associated with the current operation.
        /// </summary>
        public FileName fileName { get; set; }
       
        /// <summary>
        /// Gets or sets the line number associated with the current operation or data element.
        /// </summary>
        public long lineNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of differences detected during the comparison process.
        /// </summary>
        public long noOfDifferences { get; set; }

        /// <summary>
        /// Initializes a new instance of the Summary class with the specified file name, line number, and number of
        /// differences.
        /// </summary>
        /// <param name="fileName">The file name associated with the summary. Cannot be null.</param>
        /// <param name="lineNumber">The line number in the file to which the summary applies. Must be greater than or equal to zero.</param>
        /// <param name="noOfDifferences">The number of differences detected at the specified line. Must be greater than or equal to zero.</param>
        public Summary(FileName fileName, long lineNumber, long noOfDifferences) : this(fileName, lineNumber)
        {
            this.noOfDifferences = noOfDifferences;
        }

        /// <summary>
        /// Initializes a new instance of the Summary class with the specified file name and line number.
        /// </summary>
        /// <param name="fileName">The file name associated with the summary. Cannot be null.</param>
        /// <param name="lineNumber">The line number within the file to which the summary refers. Must be greater than or equal to zero.</param>
        public Summary(FileName fileName, long lineNumber)
        {
            this.fileName = fileName;
            this.lineNumber = lineNumber;
        }

        /// <summary>
        /// Initializes a new instance of the Summary class with the specified number of differences.
        /// </summary>
        /// <param name="noOfDifferences">The total number of differences to associate with this summary. Must be a non-negative value.</param>
        public Summary(long noOfDifferences) 
        {
            this.noOfDifferences = noOfDifferences;
            this.fileName = FileName.NoFile;
        }

        /// <summary>
        /// Initializes a new instance of the Summary class using the specified file name.
        /// </summary>
        /// <param name="fileName">The file name to associate with this instance. Cannot be null.</param>
        public Summary(FileName fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Returns a string that summarizes the comparison results between two files, including the number of
        /// differences and line information.
        /// </summary>
        /// <remarks>The returned string provides context about the comparison, including file names and
        /// line numbers when available. This method is useful for displaying a human-readable summary of file
        /// comparison results.</remarks>
        /// <returns>A descriptive string indicating whether the files are identical or the total number of differences found. If
        /// applicable, the string also includes the line number up to which the comparison was made and information
        /// about the end of the file.</returns>
        public override string ToString() 
        {
            string message1 = string.Empty;
            string message2 = string.Empty;
            string message3 = string.Empty;
            
            if (noOfDifferences > 0) 
            {
                message1 = $"Total no of differences {noOfDifferences}";
            }
            else
            {
                message1 = "File1 and File2 are Identical";
            }


            if (fileName != FileName.NoFile)
            {
                message2 = $" till line number {this.lineNumber}\n";
                message3 = "There are no more lines in " + this.fileName.ToString() + " beyond line " + this.lineNumber.ToString();
            }

            return message1 + message2 + message3;
        }
    }
}
