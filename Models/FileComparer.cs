using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public abstract class FileComparer : IComparer
    {
        public string File1Path { get; set; }

        public string File2Path { get; set; }

        public bool AreFilesSame { get; internal set; }

        public Summary summary { get; set; }

        public abstract void Compare();
    }
}
