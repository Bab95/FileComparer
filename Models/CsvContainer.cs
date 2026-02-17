using System;
using System.IO;

namespace FileComparer.Models
{
    public class CsvContainer : IComparable<CsvContainer>
    {
        public string CurrentLine { get; set; }
        public string SortKey { get; set; }
        public StreamReader Reader { get; set; }

        public CsvContainer(StreamReader reader)
        {
            Reader = reader;
        }

        public int CompareTo(CsvContainer? other)
        {
            if (other == null)
            {
                return 1;
            }

            int compare = string.Compare(SortKey, other.SortKey, StringComparison.Ordinal);
            if (compare != 0)
            {
                return compare;
            }

            return string.Compare(CurrentLine, other.CurrentLine, StringComparison.Ordinal);
        }
    }
}
