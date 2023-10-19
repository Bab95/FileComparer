using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public class SortingContext
    {
        private int ChunkSize { get; set; }

        public SortingContext(int chunkSize)
        {
            this.ChunkSize = chunkSize;
        }

        public void SortLargeFileParallel(string inputFilePath, string outputFilePath, int chunkSize)
        {
            List<string> tempFiles = new List<string>();
            List<Task> tasks = new List<Task>();
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempchunks");
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);

            try
            {
                using (var reader = new StreamReader(inputFilePath))
                {
                    int chunkIndex = 0;
                    List<string> chunk = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        chunk.Add(reader.ReadLine());

                        if (chunk.Count >= this.ChunkSize)
                        {
                            string tempFilePath = Path.Combine(tempDirectory, $"tc_{chunkIndex}.txt");
                            tempFiles.Add(tempFilePath);
                            var chunkToSort = chunk.ToList(); // Create a copy for parallel processing
                            var sortingTask = Task.Run(() => SortAndWriteChunk(chunkToSort, tempFilePath));

                            chunk.Clear();
                            chunkIndex++;
                            tasks.Add(sortingTask);
                        }
                    }

                    if (chunk.Count > 0)
                    {
                        string tempFilePath = $"tc_{chunkIndex}.txt";
                        tempFiles.Add(tempFilePath);
                        var chunkToSort = chunk.ToList(); // Create a copy for parallel processing
                        var sortingTask = Task.Run(() => SortAndWriteChunk(chunkToSort, tempFilePath));
                        tasks.Add(sortingTask);
                    }
                }

                // Wait for all sorting tasks to complete
                Task.WhenAll(tasks).Wait();

                // Merge the sorted chunks into the output file
                MergeSortedChunks(tempFiles, outputFilePath);
            }
            finally
            {
                // Clean up temporary files
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private async Task SortAndWriteChunk(List<string> chunk, string tempFilePath)
        {
            chunk.Sort();
            await File.WriteAllLinesAsync(tempFilePath, chunk);
        }
        private void MergeSortedChunks(List<string> chunkFiles, string outputFilePath)
        {
            using (var outputWriter = new StreamWriter(outputFilePath))
            {
                var readers = chunkFiles.Select(file => new StreamReader(file)).ToList();
                var lines = new List<Container>(readers.Count);
                PriorityQueueImpl<Container> pq = new PriorityQueueImpl<Container>();

                // Initialize lines with the first line from each chunk
                for (int i = 0; i < readers.Count; i++)
                {
                    //string line = readers[i].ReadLine();
                    Container _cont = new Container(readers[i]);
                    _cont.currenLine = _cont.Reader.ReadLine();
                    if (_cont.currenLine != null)
                    {
                        pq.Enqueue(_cont);
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
    }
}
