using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    /// <summary>
    /// Container is a class that encapsulates a StreamReader and the current line being read from a file.
    /// </summary>
    class Container : IComparable<Container>
    {
        /// <summary>
        /// The current line being read from the file.
        /// </summary>
        public string currenLine { get; set; }
        /// <summary>
        /// The StreamReader used to read the file.
        /// </summary>
        public StreamReader Reader { get; set; }

        /// <summary>
        /// Initializes a new instance of the Container class with the specified StreamReader. The constructor sets the Reader property to the provided StreamReader, allowing the Container to read lines from the associated file. The currenLine property can be updated as lines are read from the file using the Reader. 
        /// This class is designed
        /// </summary>
        /// <param name="reader"></param>
        public Container(StreamReader reader)
        {
            this.Reader = reader;
        }

        public int CompareTo(Container? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(this.currenLine, other.currenLine);
        }
    }
}
