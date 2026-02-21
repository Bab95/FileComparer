using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileComparer.Models.Sorting
{
    public class DataFileSortingStrategy : ISortingStrategy
    {
        private readonly int chunkSize;

        public DataFileSortingStrategy(int chunkSize)
        {
            this.chunkSize = chunkSize;
        }

        #region Sorting Strategy Begins
        public void Sort(string inputFilePath, string outputFilePath)
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

        private async Task SortAndWriteChunk(List<string> chunk, string tempFilePath)
        {
            chunk.Sort();
            await File.WriteAllLinesAsync(tempFilePath, chunk);
        }
    }
}
