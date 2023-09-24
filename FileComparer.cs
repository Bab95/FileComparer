using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileComparer
{
    public class FileComparer_
    {
        public static void Write(params object[] oo)
        {
            foreach (var o in oo)
                if (o == null)
                    Console.ResetColor();
                else if (o is ConsoleColor)
                    Console.ForegroundColor = (ConsoleColor)o;
                else
                    Console.Write(o.ToString());
            Console.ResetColor();
            Console.WriteLine();
        }

        private string file1Path { get; set; }
        private string file2Path { get; set; }

        public static int countOfActiveWorker = 0;

        public static object activeWorkerLock = new object();

        public static bool areIdentical = true;

        public ExtraData extraData = null;

        public static ConcurrentQueue<object[]> differences { get; set; } = new ConcurrentQueue<object[]>();

        public static ConcurrentQueue<long> _diff2 = new ConcurrentQueue<long> ();

        public static bool flag = false;

        public FileComparer_(string filepath1, string filepath2)
        {
            this.file1Path = filepath1;
            this.file2Path = filepath2;
        }

        private void CompareAllLines(int chunkSize)
        {
            List<string> linesChunk1 = new List<string>();
            List<string> linesChunk2 = new List<string>();
            using (StreamReader reader = new StreamReader(this.file1Path))
            using (StreamReader reader2 = new StreamReader(this.file2Path))
            {
                int lineNumber = 0;
                string line1 = null;
                string line2 = null;
                while ((line1 = reader.ReadLine()) != null &&
                    (line2 = reader2.ReadLine()) != null)
                {
   
                    linesChunk1.Add(line1);
                    linesChunk2.Add(line2);

                    lineNumber++;
                    if (linesChunk1.Count == chunkSize || linesChunk2.Count == chunkSize)
                    {
                        List<string> chunk1ToProcess = new List<string>(linesChunk1);
                        List<string> chunk2ToProcess = new List<string>(linesChunk2);

                        ChunkData chunkData = new ChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber - chunkSize);

                        lock (activeWorkerLock)
                        {
                            ++countOfActiveWorker;
                        }
                        
                        while (countOfActiveWorker >= 500) 
                        {
                            // wait before queuing new tasks.........
                        }

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessChunk), chunkData);

                        linesChunk1.Clear();
                        linesChunk2.Clear();
                    }
                }

                // Edge We still have some lines left in either file 1 or file2.
                // But we are sure that chunk size would be same as we are reading files parallely.
                if (linesChunk1.Count > 0 || linesChunk2.Count > 0)
                {
                    lock (activeWorkerLock)
                    {
                        List<string> chunk1ToProcess = new List<string>(linesChunk1);
                        List<string> chunk2ToProcess = new List<string>(linesChunk2);

                        ChunkData lastChunk = new ChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber - linesChunk1.Count);

                        ++countOfActiveWorker;
                        
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessChunk), lastChunk);
                        linesChunk1.Clear();
                        linesChunk2.Clear();
                    }
                }

                if (reader.ReadLine() != null
                    && reader2.ReadLine() == null) //There are still some lines left in File1
                {
                    this.extraData = new ExtraData();
                    extraData.file = ExtraData.File.File1;
                    extraData.LineNumber = lineNumber;
                    extraData.extraMessage = new object[] { "There are no more lines in ", ConsoleColor.DarkMagenta, "File2 ", ConsoleColor.White, "beyond line ", ConsoleColor.DarkRed, $"{lineNumber + 1}" };
                }
                else if (reader2.ReadLine() != null
                    && reader.ReadLine() == null) // There are still lines left in File2
                {
                    this.extraData = new ExtraData();
                    extraData.file = ExtraData.File.File2;
                    extraData.LineNumber = lineNumber;
                    extraData.extraMessage = new object[] { "There are no more lines in ", ConsoleColor.DarkMagenta, "File2 ", ConsoleColor.White, "beyond line ", ConsoleColor.DarkRed, $"{lineNumber + 1}" };
                }
            }

            lock (activeWorkerLock)
            {
                countOfActiveWorker--;
                Monitor.PulseAll(activeWorkerLock);

                while (countOfActiveWorker > 0)
                {
                    Monitor.Wait(activeWorkerLock);
                }
            }
        }
        public void Compare(object obj)
        {
            int chunkSize = 50000;

            try
            {
                CompareAllLines(chunkSize);
            }
            catch (FileNotFoundException e)
            {
                areIdentical = false;
                Write(ConsoleColor.Red, $"File Not found Exception occurred. Please check the file input parmetres {e.FileName}");
            }
            catch (Exception e)
            {
                areIdentical = false;
                Console.WriteLine("UNKNOWN EXCEPTION Occurred !!!!" + e.StackTrace);
            }
            finally {
                lock (activeWorkerLock)
                {
                    while (countOfActiveWorker > 0)
                    {
                        Monitor.Wait(activeWorkerLock);
                    }
                }
            }
        }

        public static void ProcessChunk(object data)
        {
            ChunkData chunkData = (ChunkData)data;

            for (int index = 0; index < chunkData.Lines1.Count; index++)
            {
                string _line1 = chunkData.Lines1[index];
                string _line2 = chunkData.Lines2[index];
                if (!_line1.Equals(_line2, StringComparison.Ordinal))
                {
                    if (flag)
                    {
                        var strDiff = Enumerable.Range(0, Math.Max(_line1.Length, _line2.Length))
                                        .Where(i => i >= _line1.Length || i >= _line2.Length || _line1[i] != _line2[i])
                                        .Select(i => new
                                        {
                                            Index = i + 1,
                                            Char1 = i < _line1.Length ? _line1[i] : ' ',
                                            Char2 = i < _line2.Length ? _line2[i] : ' '
                                        })
                                        .ToList();

                        List<object> diff_list = new List<object>();
                        diff_list.AddRange(new object[]{ ConsoleColor.Green,
                        $"Line number : {chunkData.LineNumber + index + 1} -> " });
                        bool isFirstDiff = true;
                        foreach (var _diff in strDiff)
                        {
                            if (!isFirstDiff)
                            {
                                diff_list.Add(" ,");

                            }

                            isFirstDiff = false;

                            object[] diff = {   " At index:",
                                            ConsoleColor.DarkRed,
                                            _diff.Index,
                                            ConsoleColor.White,
                                            " (",
                                            ConsoleColor.Cyan,
                                            _diff.Char1.ToString(),
                                            ConsoleColor.White,
                                            " vs ",
                                            ConsoleColor.White,
                                            _diff.Char2.ToString(),
                                            ConsoleColor.White,
                                            ")"

                                        };
                            diff_list.AddRange(diff);
                        }

                        differences.Enqueue(diff_list.ToArray());
                    }
                    else
                    {
                        _diff2.Enqueue(chunkData.LineNumber + index);
                    }
                }
            }

            lock (activeWorkerLock)
            {
                --countOfActiveWorker;
                Monitor.PulseAll(activeWorkerLock);
            }
        }

        // Data structure to store a chunk of lines and the starting line number
        private class ChunkData
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

        public class ExtraData
        {
            public enum File
            {
                File1,
                File2
            }
            public File file { get; set; }
            public long LineNumber { get; set; }

            public object[] extraMessage { get; set; }
        }
    }
}
