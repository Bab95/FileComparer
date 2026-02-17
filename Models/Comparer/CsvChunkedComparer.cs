using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileComparer.Models.Comparer
{
    public class CsvChunkedComparer : FileComparer
    {
        public static int countOfActiveWorker = 0;

        public static object activeWorkerLock = new object();

        public static ConcurrentQueue<long> LineDifferences { get; set; } = new ConcurrentQueue<long>();

        private readonly char delimiter;
        private readonly bool ignoreHeader;
        private readonly bool normalize;
        private readonly Dictionary<string, string> normalizedCache = new Dictionary<string, string>(StringComparer.Ordinal);

        public CsvChunkedComparer(string file1Path, string file2Path, char delimiter, bool ignoreHeader, bool normalize)
        {
            File1Path = file1Path;
            File2Path = file2Path;
            this.delimiter = delimiter;
            this.ignoreHeader = ignoreHeader;
            this.normalize = normalize;
        }

        public override void Compare(object obj)
        {
            int chunkSize = Constants.ChunkSize;

            try
            {
                CompareAllLines(chunkSize);
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

        public void CompareAllLines(int chunkSize)
        {
            List<CsvRecord> recordsChunk1 = new List<CsvRecord>();
            List<CsvRecord> recordsChunk2 = new List<CsvRecord>();
            using (StreamReader reader1 = new StreamReader(File1Path))
            using (StreamReader reader2 = new StreamReader(File2Path))
            {
                if (ignoreHeader)
                {
                    reader1.ReadLine();
                    reader2.ReadLine();
                }

                long lineNumber = 0;
                string line1 = null;
                string line2 = null;
                while ((line1 = reader1.ReadLine()) != null &&
                    (line2 = reader2.ReadLine()) != null)
                {
                    lineNumber++;
                    recordsChunk1.Add(ParseRecord(line1, lineNumber));
                    recordsChunk2.Add(ParseRecord(line2, lineNumber));

                    if (recordsChunk1.Count == chunkSize || recordsChunk2.Count == chunkSize)
                    {
                        List<CsvRecord> chunk1ToProcess = new List<CsvRecord>(recordsChunk1);
                        List<CsvRecord> chunk2ToProcess = new List<CsvRecord>(recordsChunk2);

                        CsvChunkData chunkData = new CsvChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber - chunkSize);

                        while (countOfActiveWorker >= Constants.MaxJobsInPool)
                        {
                        }

                        lock (activeWorkerLock)
                        {
                            ++countOfActiveWorker;
                        }

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessChunk), chunkData);

                        recordsChunk1.Clear();
                        recordsChunk2.Clear();
                    }
                }

                if (recordsChunk1.Count > 0 || recordsChunk2.Count > 0)
                {
                    lock (activeWorkerLock)
                    {
                        List<CsvRecord> chunk1ToProcess = new List<CsvRecord>(recordsChunk1);
                        List<CsvRecord> chunk2ToProcess = new List<CsvRecord>(recordsChunk2);

                        CsvChunkData lastChunk = new CsvChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber - recordsChunk1.Count);

                        ++countOfActiveWorker;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessChunk), lastChunk);
                        recordsChunk1.Clear();
                        recordsChunk2.Clear();
                    }
                }

                if (reader1.ReadLine() != null
                    && reader2.ReadLine() == null)
                {
                    summary = new Summary(FileName.File2, lineNumber);
                }
                else if (reader2.ReadLine() != null
                    && reader1.ReadLine() == null)
                {
                    summary = new Summary(FileName.File1, lineNumber);
                }
                else
                {
                    summary = new Summary(FileName.NoFile);
                }
            }

            lock (activeWorkerLock)
            {
                countOfActiveWorker--;
                Monitor.PulseAll(activeWorkerLock);
            }
        }

        private CsvRecord ParseRecord(string line, long recordId)
        {
            var fields = (line ?? string.Empty).Split(delimiter);
            return new CsvRecord(recordId, fields, normalize, normalizedCache);
        }

        public static void ProcessChunk(object data)
        {
            CsvChunkData chunkData = (CsvChunkData)data;

            for (int index = 0; index < chunkData.Records1.Count; index++)
            {
                CsvRecord record1 = chunkData.Records1[index];
                CsvRecord record2 = chunkData.Records2[index];
                if (!AreRecordsEqual(record1, record2))
                {
                    LineDifferences.Enqueue(chunkData.LineNumber + index + 1);
                }
            }

            lock (activeWorkerLock)
            {
                --countOfActiveWorker;
                Monitor.PulseAll(activeWorkerLock);
            }
        }

        private static bool AreRecordsEqual(CsvRecord record1, CsvRecord record2)
        {
            if (record1.Fields.Count != record2.Fields.Count)
            {
                return false;
            }

            for (int index = 0; index < record1.Fields.Count; index++)
            {
                if (!record1.Fields[index].Equals(record2.Fields[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
