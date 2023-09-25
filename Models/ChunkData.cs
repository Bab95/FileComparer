using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public class ChunkData
    {
        public List<string> Lines1 { get; }
        public List<string> Lines2 { get; }
        public int LineNumber { get; set; }

        public ChunkData(List<string> lines1, List<string> lines2, int lineNumber)
        {
            Lines1 = lines1;
            Lines2 = lines2;
            LineNumber = lineNumber;
        }
    }
}
