using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileComparer.Models.Sorting
{
    /// <summary>
    /// Provides a sorting strategy for large data files by dividing them into manageable chunks, sorting each chunk
    /// individually, and merging the sorted chunks into a final output file.
    /// </summary>
    /// <remarks>This class is designed to efficiently sort files that may not fit entirely in memory by using
    /// a chunk-based approach. It creates temporary files for each sorted chunk and merges them using a priority queue.
    /// The chunk size is configurable via the constructor, allowing optimization based on available system resources.
    /// Temporary files and directories are managed automatically and cleaned up after sorting completes. This strategy
    /// is suitable for scenarios where external sorting is required, such as processing large log files or
    /// datasets.</remarks>
    public class DataFileSortingStrategy : ISortingStrategy
    {
        /// <summary>
        /// chunk size
        /// </summary>
        private readonly int chunkSize;

        /// <summary>
        /// Initializes a new instance of the DataFileSortingStrategy class with the specified chunk size for processing
        /// data files.
        /// </summary>
        /// <remarks>The chunk size determines how data files are partitioned during sorting operations.
        /// Choosing an appropriate chunk size can affect performance and memory usage.</remarks>
        /// <param name="chunkSize">The maximum number of items to include in each chunk when sorting data files. Must be a positive integer.</param>
        public DataFileSortingStrategy(int chunkSize)
        {
            this.chunkSize = chunkSize;
        }

        #region Sorting Strategy Begins
        /// <summary>
        /// Sorts the contents of the specified input file and writes the sorted results to the specified output file.
        /// </summary>
        /// <remarks>This method is designed to handle large files by processing and sorting data in
        /// chunks, which are temporarily stored and merged. Temporary files and directories are created during the
        /// operation and are cleaned up automatically after completion. The method blocks until sorting and merging are
        /// finished. If an error occurs during processing, temporary files may be deleted as part of cleanup.</remarks>
        /// <param name="inputFilePath">The path to the file containing the unsorted data. Must refer to an existing file.</param>
        /// <param name="outputFilePath">The path to the file where the sorted data will be written. If the file exists, it will be overwritten.</param>
        /// <param name="shouldSkipFirstLine">A flag indicating whether to skip the first line of the input file, typically used to ignore headers.</param>
        public void Sort(string inputFilePath, string outputFilePath, bool shouldSkipFirstLine)
        {
            List<string> tempFiles = new List<string>();
            List<Task> tasks = new List<Task>();
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempchunks");
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
                Logger.LogWarn("Existing temporary directory found and deleted: " + tempDirectory);
            }

            Directory.CreateDirectory(tempDirectory);
            Logger.LogInfo("Temporary directory created for chunk files: " + tempDirectory);
            
            try
            {
                using (var reader = new StreamReader(inputFilePath))
                {
                    if (shouldSkipFirstLine)
                    {
                        if (!reader.EndOfStream)
                            reader.ReadLine();
                        else
                            Logger.LogWarn("Input file is empty, Even headers are not present");
                        Logger.LogInfo("Files are to be compared with headers!!");
                    }

                    int chunkIndex = 0;
                    List<string> chunk = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        chunk.Add(reader.ReadLine());

                        if (chunk.Count >= chunkSize)
                        {
                            string tempFilePath = Path.Combine(tempDirectory, $"tc_{chunkIndex}.txt");
                            tempFiles.Add(tempFilePath);
                            var chunkToSort = chunk.ToList();
                            var sortingTask = Task.Run(() => SortAndWriteChunk(chunkToSort, tempFilePath));

                            chunk.Clear();
                            chunkIndex++;
                            tasks.Add(sortingTask);
                        }
                    }

                    if (chunk.Count > 0)
                    {
                        string tempFilePath = Path.Combine(tempDirectory, $"tc_{chunkIndex}.txt");
                        tempFiles.Add(tempFilePath);
                        var chunkToSort = chunk.ToList();
                        var sortingTask = Task.Run(() => SortAndWriteChunk(chunkToSort, tempFilePath));
                        tasks.Add(sortingTask);
                    }
                }

                Task.WhenAll(tasks).Wait();
                Logger.LogInfo("All chunks sorted and written to temporary files. Starting merge process...");
                MergeSortedChunks(tempFiles, outputFilePath);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Logger.LogWarn("Clearing out Temp directory!!");
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        /// <summary>
        /// Merges multiple sorted chunk files into a single output file, preserving overall sorted order.
        /// </summary>
        /// <remarks>This method assumes that each chunk file is sorted and merges them efficiently into
        /// the specified output file. The output file's directory will be created if it does not exist. Existing files
        /// or directories at the output path will be deleted before writing. The method logs warnings and informational
        /// messages regarding file operations.</remarks>
        /// <param name="chunkFiles">A list of file paths to sorted chunk files. Each file must contain lines sorted in ascending order.</param>
        /// <param name="outputFilePath">The path to the file where the merged, sorted output will be written. If the file already exists, it will be
        /// overwritten.</param>
        private void MergeSortedChunks(List<string> chunkFiles, string outputFilePath)
        {

            if (File.Exists(outputFilePath))
            {
                Logger.LogWarn("Output file already exists and will be overwritten: " + outputFilePath);
                File.Delete(outputFilePath);
            }

            if (Directory.Exists(outputFilePath))
            {
                Logger.LogWarn("Output file already exists and will be overwritten: " + outputFilePath);
                Directory.Delete(outputFilePath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            }

            Logger.LogInfo("Merging sorted chunk files into final output file: " + outputFilePath);
            using (var outputWriter = new StreamWriter(outputFilePath))
            {
                var readers = chunkFiles.Select(file => new StreamReader(file)).ToList();
                PriorityQueueImpl<Container> pq = new PriorityQueueImpl<Container>();

                for (int i = 0; i < readers.Count; i++)
                {
                    Container container = new Container(readers[i]);
                    container.currenLine = container.Reader.ReadLine();
                    if (container.currenLine != null)
                    {
                        pq.Enqueue(container);
                    }
                }

                while (!pq.IsEmpty())
                {
                    var current = pq.Dequeue();

                    outputWriter.WriteLine(current.currenLine);

                    current.currenLine = current.Reader.ReadLine();

                    if (current.currenLine == null)
                    {
                        current.Reader.Dispose();
                    }
                    else
                    {
                        pq.Enqueue(current);
                    }
                }

                for (int i = 0; i < readers.Count; i++)
                {
                    readers[i].Close();
                }
            }
        }

        #endregion

        /// <summary>
        /// Sorts the specified list of strings in ascending order and writes the sorted contents asynchronously to a
        /// temporary file.
        /// </summary>
        /// <param name="chunk">The list of strings to be sorted and written to the file. Cannot be null.</param>
        /// <param name="tempFilePath">The path to the temporary file where the sorted strings will be written. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous sort and write operation.</returns>
        private async Task SortAndWriteChunk(List<string> chunk, string tempFilePath)
        {
            chunk.Sort();
            await File.WriteAllLinesAsync(tempFilePath, chunk);
        }
    }
}
