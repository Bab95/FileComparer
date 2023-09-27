using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer
{
    public static class Utils
    {
        public static string CheckOuputFileStatus(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                Console.WriteLine("Output path already exits! File will be overwritten.....\nPress y to continue...");
                if (Console.ReadKey().KeyChar == 'Y')
                {
                    File.Delete(FilePath);
                }
                else
                {
                    Guid guid = Guid.NewGuid();
                    string oldFileName = FilePath.Split('\\').Last();
                    FilePath = FilePath.Replace(oldFileName, guid.ToString()) + ".txt";
                    Console.WriteLine($"\nOutput will be generated at {FilePath}");
                }
            }
            return FilePath;
        }
    }
}
