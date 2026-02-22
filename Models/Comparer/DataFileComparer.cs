using System.Collections.Concurrent;

namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Provides functionality for comparing two data files line by line, supporting chunked processing and concurrent
    /// difference tracking.
    /// </summary>
    /// <remarks>DataFileComparer extends ChunkedFileComparer to enable efficient comparison of large files by
    /// processing them in chunks and utilizing worker threads. Differences between files are tracked in thread-safe
    /// queues, allowing for concurrent access and aggregation of results. The class supports optional index printing
    /// for detailed difference reporting. It is designed for scenarios where performance and scalability are important,
    /// such as processing large datasets or log files. Thread safety is managed internally for worker coordination and
    /// result collection.</remarks>
    public class DataFileComparer : ChunkedFileComparer
    {
        /// <summary>
        /// Represents the current number of active worker instances.
        /// </summary>
        public static int countOfActiveWorker = 0;

        /// <summary>
        /// Provides a synchronization object used to coordinate access to shared resources among active worker threads.
        /// </summary>
        /// <remarks>This object can be used with locking constructs, such as the lock statement, to
        /// ensure thread-safe operations. It is intended for scenarios where multiple threads may attempt to access or
        /// modify shared state concurrently.</remarks>
        public static object activeWorkerLock = new object();

        /// <summary>
        /// Gets or sets a value indicating whether index information should be printed during output operations.
        /// </summary>
        public static bool printIndexes { get; set; } = false;

        /// <summary>
        /// concurrent queue to hold file differences.
        /// </summary>
        public static ConcurrentQueue<string> FileDifferences { get; set; } = new ConcurrentQueue<string>();

        /// <summary>
        /// concurrent queue to hold line differences.
        /// </summary>
        public static ConcurrentQueue<int> LineDifferences { get; set; } = new ConcurrentQueue<int>();

        /// <summary>
        /// Initializes a new instance of the DataFileComparer class using the specified file paths for comparison.
        /// </summary>
        /// <param name="file1Path">The path to the first data file to be compared. Cannot be null or empty.</param>
        /// <param name="file2Path">The path to the second data file to be compared. Cannot be null or empty.</param>
        public DataFileComparer(string file1Path, string file2Path) : base(file1Path, file2Path)
        {
            File1Path = file1Path;
            File2Path = file2Path;
        }

        /// <summary>
        /// Initializes a new instance of the DataFileComparer class with the specified file paths and a flag indicating
        /// whether to print index information during comparison.
        /// </summary>
        /// <param name="file1Path">The path to the first data file to be compared. Cannot be null or empty.</param>
        /// <param name="file2Path">The path to the second data file to be compared. Cannot be null or empty.</param>
        /// <param name="printIndexes_">A value indicating whether index information should be printed during the comparison. Specify <see
        /// langword="true"/> to print indexes; otherwise, <see langword="false"/>.</param>
        public DataFileComparer(string file1Path, string file2Path, bool printIndexes_)
            : this(file1Path, file2Path)
        {
            printIndexes = printIndexes_;
        }

        /// <summary>
        /// Compares the contents of the current instance with the specified object and processes all lines in chunks.
        /// </summary>
        /// <remarks>This method processes lines in chunks and waits for all active worker threads to
        /// complete before returning. If a file is not found during processing, an error message is written to the
        /// console. The method is thread-safe and blocks until all worker threads have finished.</remarks>
        /// <param name="obj">The object to compare with the current instance. The comparison logic depends on the implementation of the
        /// derived class.</param>
        public override void Compare(object obj)
        {
            int chunksize = Constants.ChunkSize;

            try
            {
                CompareAllLines(chunksize);

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"File not found. Please check file path {e.FileName} ");
            }
            catch (Exception e)
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

        /// <summary>
        /// Compares all lines between two files in parallel, processing them in chunks of the specified size.
        /// </summary>
        /// <remarks>This method reads both files simultaneously and processes their lines in chunks to
        /// optimize memory usage and performance. The comparison is performed using worker threads from the thread
        /// pool. If the files differ in length, the method records which file has extra lines. Thread safety is
        /// maintained when updating worker counts. The method should be called when both file paths are properly
        /// initialized and accessible.</remarks>
        /// <param name="obj">An object representing the chunk size to use when dividing lines for comparison. Must be an integer greater
        /// than zero.</param>
        public void CompareAllLines(object obj)
        {
            int chunkSize = (int)obj;
            List<string> linesChunk1 = new List<string>();
            List<string> linesChunk2 = new List<string>();
            using (StreamReader reader1 = new StreamReader(File1Path))
            using (StreamReader reader2 = new StreamReader(File2Path))
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
                else
                {
                    // There are equal lines and we still haven't counted diffs. So initialize with NoFile.
                    summary = new Summary(FileName.NoFile);
                }
            }

            lock (activeWorkerLock)
            {
                // This is where we complete the Compare function worker thread task.
                countOfActiveWorker--;
                Monitor.PulseAll(activeWorkerLock);
            }
        }

        /// <summary>
        /// Compares two sets of lines within a chunk and records differences based on the current configuration.
        /// </summary>
        /// <remarks>This method enqueues line numbers or detailed difference information depending on the
        /// value of the printIndexes setting. It also updates the active worker count upon completion. Thread safety is
        /// ensured when modifying the worker count.</remarks>
        /// <param name="data">An object containing chunk data to be processed. Must be of type ChunkData; otherwise, an exception may
        /// occur.</param>
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
                        LineDifferences.Enqueue(chunkData.LineNumber + index + 1);
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

                        foreach (var _diff in strDiff)
                        {

                            if (!isFirstDiff)
                            {
                                currentDiff += " , ";

                            }

                            isFirstDiff = false;

                            currentDiff += _diff.Index;
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
