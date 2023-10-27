using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public class SortingContext
    {
        public IComparer fileComparer { get; set; }
        private int ChunkSize { get; set; }

        Type comparerType { get; set; }

        public SortingContext(int chunkSize)
        {
            this.ChunkSize = chunkSize;
        }

        public SortingContext(int chunkSize, IComparer fileComparer, long bufferSize)
        {
            this.ChunkSize = chunkSize;
            this.fileComparer = fileComparer;
            this.comparerType = typeof(IComparer);
        }

        public void CompareSorted(string file1InputPath, string file2InputPath)
        {
            /*
            ConstructorInfo parameterizedConstructor = comparerType.GetConstructor(new[] { typeof(string), typeof(string) });
            if (parameterizedConstructor != null)
            {
                object[] parameters = { };
                IComparer comparer = 
            }*/
            Console.WriteLine("Using FileParity Comparer");
            SortAndCompareLargeFileParallel(file1InputPath, file2InputPath, this.ChunkSize);

        }

        public void SortAndCompareLargeFileParallel(string inputFilePath1, string inputFilePath2, int chunkSize)
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
                using (var reader = new StreamReader(inputFilePath1))
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
                        string tempFilePath = Path.Combine(tempDirectory, $"tc_{chunkIndex}.txt");
                        tempFiles.Add(tempFilePath);
                        var chunkToSort = chunk.ToList(); // Create a copy for parallel processing
                        var sortingTask = Task.Run(() => SortAndWriteChunk(chunkToSort, tempFilePath));
                        tasks.Add(sortingTask);
                    }
                }

                // Wait for all sorting tasks to complete
                Task.WhenAll(tasks).Wait();
                List<string> outFilePaths = new List<string>();
                // Merge the sorted chunks into the output file
                CompareWhileSortingChunks(tempFiles, outFilePaths);
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

        private void CompareWhileSortingChunks(List<string> tempFiles1, List<string> tempFiles2)
        {
            var readers1 = tempFiles1.Select(file => new StreamReader(file)).ToList();
            var readers2 = tempFiles2.Select(file => new StreamReader(file)).ToList();

            var lines1 = new List<Container>(readers1.Count);
            
            PriorityQueueImpl<Container> pq1 = new PriorityQueueImpl<Container>();
            PriorityQueueImpl<Container> pq2 = new PriorityQueueImpl<Container>();

            // Initialize lines with the first line from each chunk
            for (int i = 0; i < readers1.Count; i++)
            {
                //string line = readers[i].ReadLine();
                Container _cont = new Container(readers1[i]);
                _cont.currenLine = _cont.Reader.ReadLine();
                if (_cont.currenLine != null)
                {
                    pq1.Enqueue(_cont);
                }
            }

            for (int i = 0; i < readers2.Count; i++)
            {
                //string line = readers[i].ReadLine();
                Container _cont = new Container(readers2[i]);
                _cont.currenLine = _cont.Reader.ReadLine();
                if (_cont.currenLine != null)
                {
                    pq2.Enqueue(_cont);
                }
            }
            
            List<string> differences = new List<string>();

            while (!pq1.IsEmpty() && !pq2.IsEmpty())
            {
                var current1 = pq1.Dequeue();
                var current2 = pq2.Dequeue();
                
                if (!current1.currenLine.Equals(current2.currenLine))
                {
                    differences.Add($"File1::{current1.currenLine}\nFile2::{current2.currenLine}");
                }

                current1.currenLine = current1.Reader.ReadLine();
                current2.currenLine = current2.Reader.ReadLine();

                if (current1.currenLine == null)
                {
                    current1.Reader.Dispose();
                }
                else
                {
                    pq1.Enqueue(current1);
                }
                if (current1.currenLine == null)
                {
                    current1.Reader.Dispose();
                }
                else
                {
                    pq1.Enqueue(current1);
                }


            }


            for(int i = 0; i < differences.Count; i++)
            {
                Console.WriteLine(differences[i]);
            }

            if (!pq1.IsEmpty())
            {
                Console.WriteLine("There are more lines in File1");
            }

            if (!pq2.IsEmpty())
            {
                Console.WriteLine("There are more lines in File2");
            }

            for (int i = 0; i < readers1.Count; i++)
            {
                readers1[i].Close();
            }

            for(int i=0;i < readers2.Count; i++)
            {
                readers2[i].Close();
            }
        }

        private void MergeSortedChunks(List<string> chunkFiles, List<string> outputFilePath, int bufferSize, int fileNumber)
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
                int count = 0;
                int chunkIndex = 0;
                string currentOutFile = $"sr_{chunkIndex}_{fileNumber}.txt";
                using (StreamWriter outputWriter = new StreamWriter(currentOutFile))
                {
                    while (count < bufferSize)
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
                        count++;
                    }
                }

                chunkIndex++;
                outputFilePath.Add(currentOutFile);
            }

            for (int i = 0; i < readers.Count; i++)
            {
                readers[i].Close();
            }
            
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
                        string tempFilePath = Path.Combine(tempDirectory, $"tc_{chunkIndex}.txt");
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
