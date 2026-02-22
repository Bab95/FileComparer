namespace FileComparer
{
    public static class Constants
    {
        /// <summary>
        /// Chunk size for breaking file for multithreaded processing.
        /// </summary>
        public static readonly int ChunkSize = 50000;

        /// <summary>
        /// Specifies the default number of items to process in each sort operation chunk.
        /// </summary>
        /// <remarks>This value can be used to control memory usage and performance when sorting large
        /// datasets in batches. Adjusting the chunk size may affect the efficiency of sorting operations, especially
        /// for very large collections.</remarks>
	    public static readonly int SortChunkSize=100000;

        /// <summary>
        /// Max no of jobs allowed in threadpool.
        /// This is to control the memory consumption.
        /// </summary>
        public static readonly int MaxJobsInPool = 1000;

        /// <summary>
        /// Max no of threads allowed to run from ThreadPool.
        /// </summary>
        public static readonly int MaxThreadsCount = 64;

        /// <summary>
        /// Max no of differences to print on console. This is to control the console output and make it readable.
        /// </summary>
        public static readonly int MaxDifferenceToPrintOnConsole = 100;
    }
}
