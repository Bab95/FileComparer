using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    class Container : IComparable<Container>
    {
        public string currenLine { get; set; }
        public StreamReader Reader { get; set; }

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
