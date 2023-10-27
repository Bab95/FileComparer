using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer
{
    public static class Constants
    {
        /// <summary>
        /// Chunk size for breaking file for multithreaded processing.
        /// </summary>
        public static readonly int ChunkSize = 10;

        /// <summary>
        /// Max no of jobs allowed in threadpool.
        /// This is to control the memory consumption.
        /// </summary>
        public static readonly int MaxJobsInPool = 1000;

        /// <summary>
        /// Max no of threads allowed to run from ThreadPool.
        /// </summary>
        public static readonly int MaxThreadsCount = 64;

        public static readonly int mergedChunksLineNumber = 10;
    }
}
