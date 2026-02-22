namespace FileComparer.Models
{
    /// <summary>
    /// Contains the data for a chunk of CSV records from two files being compared.
    /// </summary>
    public class CsvChunkData
    {
        /// <summary>
        /// The list of CSV records from the first file for the current chunk.
        /// </summary>
        public List<CsvRecord> Records1 { get; }

        /// <summary>
        /// The list of CSV records from the second file for the current chunk.
        /// </summary>
        public List<CsvRecord> Records2 { get; }

        /// <summary>
        /// The starting line number of the chunk in the original files.
        /// </summary>
        public long LineNumber { get; set; }

        /// <summary>
        /// The unique identifier for the chunk of CSV records.
        /// </summary>
        public long RecordId { get; set; }

        /// <summary>
        /// Initializes a new instance of the CsvChunkData class with the specified CSV records and starting line number.
        /// </summary>
        /// <param name="records1">The CSV records from the first file for the current chunk.</param>
        /// <param name="records2">The CSV records from the second file for the current chunk.</param>
        /// <param name="lineNumber">The starting line number of the chunk in the original files.</param>
        public CsvChunkData(List<CsvRecord> records1, List<CsvRecord> records2, long lineNumber)
        {
            Records1 = records1;
            Records2 = records2;
            LineNumber = lineNumber;
            RecordId = lineNumber;
        }
    }
}
