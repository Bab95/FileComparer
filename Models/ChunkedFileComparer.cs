using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    /// <summary>
    /// Compares File in chunks and prints Indexes.
    /// Printing indexes is a heavy process should be used accordingly.
    /// </summary>
    public class ChunkedFileComparer : FileComparer
    {
        public static int countOfActiveWorker = 0;

        public static object activeWorkerLock = new object();

        public static bool printIndexes { get; set; } = false;

        public OutputKind outputKind { get; set; }

        public static ConcurrentQueue<string> FileDifferences { get; set; } = new ConcurrentQueue<string>();

        public static ConcurrentQueue<int> LineDifferences { get; set; } = new ConcurrentQueue<int>();

        public ChunkedFileComparer(string file1Path, string file2Path)
        {
            this.File1Path = file1Path;
            this.File2Path = file2Path;
        }

        public ChunkedFileComparer(string file1Path, string file2Path, bool printIndexes_) 
            : this(file1Path, file2Path)
        {
            printIndexes = printIndexes_;
        }


        public override void Compare(object obj)
        {
            int chunksize = Constants.ChunkSize;

            try
            {
                CompareAllLines(chunksize);

            }catch (FileNotFoundException e)
            {
                Console.WriteLine($"File not found. Please check file path {e.FileName} ");
            }catch(Exception e)
            {
                Console.WriteLine($"Unknown Exception caught {e.Message}");
            }
            finally
            {
                lock (activeWorkerLock)
                {
                    while (countOfActiveWorker > 0)
                    {
                        Monitor.Wait(activeWorkerLock);
                    }
                }
            }
        }

        public  void CompareAllLines(object obj)
        {
            int chunkSize = (int)obj;
            List<string> linesChunk1 = new List<string>();
            List<string> linesChunk2 = new List<string>();
            using (StreamReader reader1 = new StreamReader(this.File1Path))
            using (StreamReader reader2 = new StreamReader(this.File2Path))
            {
                int lineNumber = 0;
                string line1 = null;
                string line2 = null;
                while ((line1 = reader1.ReadLine()) != null &&
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

                        while (countOfActiveWorker >= Constants.MaxJobsInPool)
                        {
                            // wait before queuing new tasks
                            // we don't want to overwhelm the memory.........

                            // if needs some interactions uncomment this line.
                            //Console.WriteLine($"Lines processed {lineNumber}");
                        }

                        lock (activeWorkerLock)
                        {
                            ++countOfActiveWorker;
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

                if (reader1.ReadLine() != null
                    && reader2.ReadLine() == null) //There are still some lines left in File1
                {
                    summary = new Summary(FileName.File2, lineNumber);
                }
                else if (reader2.ReadLine() != null
                    && reader1.ReadLine() == null) // There are still lines left in File2
                {
                    summary = new Summary(FileName.File1, lineNumber);
                }
            }

            lock (activeWorkerLock)
            {
                // This is where we complete the Compare function worker thread task.
                countOfActiveWorker--;
                Monitor.PulseAll(activeWorkerLock);
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
                    
                    if (printIndexes == false)
                    {
                        LineDifferences.Enqueue(chunkData.LineNumber + index + 1 );
                    }
                    else if (printIndexes == true)
                    {
                        string currentDiff = string.Empty;
                        var strDiff = Enumerable.Range(0, Math.Max(_line1.Length, _line2.Length))
                                        .Where(i => i >= _line1.Length || i >= _line2.Length || _line1[i] != _line2[i])
                                        .Select(i => new
                                        {
                                            Index = i + 1,
                                            Char1 = i < _line1.Length ? _line1[i] : ' ',
                                            Char2 = i < _line2.Length ? _line2[i] : ' '
                                        })
                                        .ToList();

                        currentDiff = $"Line number : {chunkData.LineNumber + index + 1} ";
                        bool isFirstDiff = true;
                        int diff_index = 0;
                        foreach (var _diff in strDiff)
                        {
                            diff_index++;
                            if (!isFirstDiff)
                            {
                                currentDiff += " , ";

                            }

                            isFirstDiff = false;

                            currentDiff += $" At:{diff_index}" + 
                                            " (" + 
                                            _diff.Char1.ToString() +
                                            " | " +
                                            _diff.Char2.ToString()
                                            +
                                            ")";
                        }
                        FileDifferences.Enqueue(currentDiff);
                    }
                }
            }

            // Finally job is complete now reduce the active job count.
            lock (activeWorkerLock)
            {
                --countOfActiveWorker;
                Monitor.PulseAll(activeWorkerLock);
            }
        }
    }
}
