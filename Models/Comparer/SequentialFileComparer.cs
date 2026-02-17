using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models.Comparer
{
    public class SequentialFileComparer : FileComparer
    {
        public bool printDiffs { get; set; } = false;
        public int printNoOfDiffs { get; set; } = 5;
        public SequentialFileComparer(string file1Path,  string file2Path)
        {
            File1Path = file1Path;
            File2Path = file2Path;
        }
        
        public void PrintSummary()
        {
            Console.WriteLine(summary.ToString());
        }

        public override void Compare(object obj)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                long noOfDifferentLines = 0;
                using (StreamReader reader1 = new StreamReader(File1Path)) 
                    using (StreamReader reader2 = new StreamReader(File2Path))
                {
                    string line1 = null;
                    string line2 = null;
                    long lineNumber = 0;
                    int countOfPrinted = 0;
                    while ((line1 = reader1.ReadLine()) != null &&
                            (line2 = reader2.ReadLine()) != null)
                    {
                        if (!line1.Equals(line2))
                        {
                            noOfDifferentLines++;
                            if (printDiffs == true && countOfPrinted < printNoOfDiffs)
                            {
                                Console.WriteLine($"[File1::{lineNumber} :: {line1}");
                                Console.WriteLine($"[File2::{lineNumber} :: {line2}");
                                countOfPrinted++;
                            }
                        }
                        lineNumber++;
                    }

                    if (reader1.ReadLine() != null
                        && reader2.ReadLine() == null) //There are still some lines left in File1
                    {
                        AreFilesSame = false;
                        summary = new Summary(FileName.File1, lineNumber, noOfDifferentLines);
                    }
                    else if (reader2.ReadLine() != null
                        && reader1.ReadLine() == null) // There are still lines left in File2
                    {
                        AreFilesSame = false;
                        summary = new Summary(FileName.File2, lineNumber, noOfDifferentLines);
                    }
                    else
                    {
                        summary = new Summary(noOfDifferentLines);
                    }

                    PrintSummary();
                }

            }
            catch (FileNotFoundException fe)
            {
                Console.WriteLine("Unable to open the file " + fe.Source);
            }
            
            watch.Stop();

            Console.WriteLine($"TOTAL TIME TAKEN IN COMPARISON {watch.ElapsedMilliseconds}");
        }
    }
}
