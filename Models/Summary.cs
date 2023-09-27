using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public class Summary
    {
        public FileName fileName { get; set; }
       
        public long lineNumber { get; set; }

        public long noOfDifferences { get; set; }


        public Summary(FileName fileName, long lineNumber, long noOfDifferences) : this(fileName, lineNumber)
        {
            this.noOfDifferences = noOfDifferences;
        }

        public Summary(FileName fileName, long lineNumber)
        {
            this.fileName = fileName;
            this.lineNumber = lineNumber;
        }

        public Summary(long noOfDifferences) 
        {
            this.noOfDifferences = noOfDifferences;
            this.fileName = FileName.NoFile;
        }

        public Summary(FileName fileName)
        {
            this.fileName = fileName;
        }

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
