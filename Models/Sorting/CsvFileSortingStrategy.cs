using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileComparer.Models.Sorting
{
    public class CsvFileSortingStrategy : ISortingStrategy
    {
        private readonly int chunkSize;
        private readonly char delimiter;
        private readonly int sortColumnIndex;
        private readonly bool normalize;

        public CsvFileSortingStrategy(int chunkSize, char delimiter, int sortColumnIndex, bool normalize)
        {
            this.chunkSize = chunkSize;
            this.delimiter = delimiter;
            this.sortColumnIndex = sortColumnIndex;
            this.normalize = normalize;
        }

        public void Sort(string inputFilePath, string outputFilePath)
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

                MergeSortedChunks(tempFiles, outputFilePath);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private async Task SortAndWriteChunk(List<string> chunk, string tempFilePath)
        {
            var sorted = chunk
                .Select(line => new CsvSortItem(line, GetSortKey(line)))
                .ToList();

            sorted.Sort((left, right) =>
            {
                int compare = string.Compare(left.SortKey, right.SortKey, StringComparison.Ordinal);
                if (compare != 0)
                {
                    return compare;
                }

                return string.Compare(left.Line, right.Line, StringComparison.Ordinal);
            });

            await File.WriteAllLinesAsync(tempFilePath, sorted.Select(item => item.Line));
        }

        private void MergeSortedChunks(List<string> chunkFiles, string outputFilePath)
        {
            using (var outputWriter = new StreamWriter(outputFilePath))
            {
                var readers = chunkFiles.Select(file => new StreamReader(file)).ToList();
                PriorityQueueImpl<CsvContainer> pq = new PriorityQueueImpl<CsvContainer>();

                for (int i = 0; i < readers.Count; i++)
                {
                    CsvContainer container = new CsvContainer(readers[i]);
                    container.CurrentLine = container.Reader.ReadLine();
                    if (container.CurrentLine != null)
                    {
                        container.SortKey = GetSortKey(container.CurrentLine);
                        pq.Enqueue(container);
                    }
                }

                while (!pq.IsEmpty())
                {
                    var current = pq.Dequeue();

                    outputWriter.WriteLine(current.CurrentLine);

                    current.CurrentLine = current.Reader.ReadLine();

                    if (current.CurrentLine == null)
                    {
                        current.Reader.Dispose();
                    }
                    else
                    {
                        current.SortKey = GetSortKey(current.CurrentLine);
                        pq.Enqueue(current);
                    }
                }

                for (int i = 0; i < readers.Count; i++)
                {
                    readers[i].Close();
                }
            }
        }

        private string GetSortKey(string line)
        {
            var fields = (line ?? string.Empty).Split(delimiter);
            int index = Math.Max(0, sortColumnIndex - 1);
            string key = index < fields.Length ? fields[index] : string.Empty;

            if (normalize)
            {
                return key.Trim();
            }

            return key;
        }

        private class CsvSortItem
        {
            public CsvSortItem(string line, string sortKey)
            {
                Line = line;
                SortKey = sortKey;
            }

            public string Line { get; }
            public string SortKey { get; }
        }
    }
}
