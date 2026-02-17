using System.Collections.Generic;

namespace FileComparer.Models
{
    public class CsvChunkData
    {
        public List<CsvRecord> Records1 { get; }
        public List<CsvRecord> Records2 { get; }
        public long LineNumber { get; set; }

        public CsvChunkData(List<CsvRecord> records1, List<CsvRecord> records2, long lineNumber)
        {
            Records1 = records1;
            Records2 = records2;
            LineNumber = lineNumber;
        }
    }
}
