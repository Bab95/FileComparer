using FileComparer;
using FileComparer.Models;
using FileComparer.Models.Comparer;
using FileComparer.Models.Sorting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// CsvFileComparer is a class that extends the ChunkedFileComparer to provide functionality for comparing CSV files. 
/// It includes options for sorting and normalizing the data before comparison, as well as handling custom delimiters. 
/// The class is designed to compare CSV files efficiently by processing them in chunks, which allows it to handle 
/// large files that may not fit entirely into memory. 
/// The comparison logic is intended to be implemented in the Compare method, which will read, parse, normalize, and compare the CSV records chunk by chunk. The class also includes logging for key operations to facilitate debugging and performance monitoring.
/// </summary>
public class CsvFileComparer : ChunkedFileComparer
{
    /// <summary>
    /// count of active threads processing the chunks, 
    /// this is used to control the flow of data and ensure that we do not overwhelm the system with too many concurrent operations.
    /// </summary>
    public static int countofActiveWorkers = 0;

    /// <summary>
    /// object used for locking access to the countofActiveWorkers variable, ensuring thread safety when multiple threads are updating the count.
    /// </summary>
    public static object activeWorkerLock = new object();

    /// <summary>
    /// Queue to store the differences found between the two CSV files during the comparison process. Each entry in the queue is a pair of CsvRecord objects, 
    /// representing a record from each file that was identified as different. This allows for thread-safe storage and retrieval of differences as they are discovered by worker threads processing the chunks of data.
    /// </summary>
    public static ConcurrentQueue<Pair<CsvRecord, CsvRecord>> recordDifferences { get; set; } = new ConcurrentQueue<Pair<CsvRecord, CsvRecord>>();

    /// <summary>
    /// First header record from the first CSV file. 
    /// This is used to store the header information if the ignoreHeader flag is set to false, 
    /// allowing for proper handling of the header row during comparison and sorting operations. 
    /// If ignoreHeader is true, this variable will not be used and the first row of each CSV file will be treated as data rather than a header.
    /// </summary>
    public CsvRecord headerRecord1 { get; set; }

    /// <summary>
    /// Second header record from the second CSV file. 
    /// Similar to headerRecord1, this variable is used to store the header information from the second CSV file
    /// when the ignoreHeader flag is set to false. 
    /// It allows for consistent handling of the header row during comparison and sorting operations, ensuring that the header does not affect the comparison results if it is present in both files. If ignoreHeader is true, this variable will not be used and the first row of each CSV file will be treated as data rather than a header.
    /// </summary>
    public CsvRecord headerRecord2 { get; set; }

    /// <summary>
    /// Chunk size for processing the CSV files. This determines how many records are read and processed in memory at a time during sorting and comparison operations.
    /// </summary>
    private readonly int chunkSize = Constants.ChunkSize;

    /// <summary>
    /// In case of differences, this variable limits the number of differences to report. 
    /// This is useful for performance reasons and to avoid overwhelming the user with too many differences, especially when comparing large CSV files. The value can be adjusted based on the specific requirements of the comparison task.
    /// </summary>
    private readonly int noOfDifferencesToReport = 10;

    /// <summary>
    /// Delimiter used to separate fields in the CSV records. This is essential for correctly parsing the records and performing comparisons based on the individual fields. The delimiter can be customized through the constructor, allowing for flexibility in handling different CSV formats.
    /// </summary>
    private string delimiter { get; set; }

    /// <summary>
    /// Normalization flag indicating whether to normalize the CSV records before comparison. Normalization can include operations such as trimming whitespace, converting to lowercase, or other transformations that ensure consistent comparison results regardless of variations in the input data. This flag can be set through the constructor to control whether normalization is applied during the sorting and comparison processes.
    /// </summary>
    private bool shouldNormalize { get; set; }

    /// <summary>
    /// Flag indicating whether to ignore the header row in the CSV files during comparison. When set to true, the first row of each CSV file is treated as a header and is excluded from the comparison process. This is useful when the CSV files contain column names that should not be considered as part of the data comparison.
    /// if ignore header is true, then the sorting strategy will also be configured to ignore the header row when sorting the files, ensuring that the header does not affect the sorting order or comparison results.
    /// if ignore header is true, then first row is data.
    /// if ignore header is false, then first row is header and will be ignored for comparison and sorting.
    /// </summary>
    public bool ignoreHeader { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the CsvFileComparer class with the specified file paths.
    /// </summary>
    /// <param name="filePath1">The path to the first CSV file.</param>
    /// <param name="filePath2">The path to the second CSV file.</param>
    public CsvFileComparer(string filePath1, 
        string filePath2) : base(filePath1, filePath2)
    {
        this.delimiter = ",";
        Logger.LogInfo("CSV comparer initialized.");
    }

    /// <summary>
    /// Initializes a new instance of the CsvFileComparer class with the specified file paths and a custom delimiter for parsing the CSV records. This constructor allows for flexibility in handling different CSV formats that may use delimiters other than the default comma.
    /// </summary>
    /// <param name="filePath1">The path to the first CSV file.</param>
    /// <param name="filePath2">The path to the second CSV file.</param>
    /// <param name="delimiter">The delimiter used to separate fields in the CSV records.</param>
    public CsvFileComparer(string filePath1, string filePath2, string delimiter) : base(filePath1, filePath2)
    {
        this.delimiter = delimiter;
        Logger.LogInfo("CSV comparer initialized with custom delimiter.");
    }

    /// <summary>
    /// Initializes a new instance of the CsvFileComparer class with the specified file paths and a flag indicating whether to sort the CSV files before comparison. If sorting is requested, the constructor configures the sorting context with a CsvFileSortingStrategy, which will be used to sort the files based on the specified chunk size and delimiter. This allows for efficient comparison of large CSV files by ensuring that they are sorted before the comparison process begins.
    /// </summary>
    /// <param name="filePath1">The path to the first CSV file.</param>
    /// <param name="filePath2">The path to the second CSV file.</param>
    /// <param name="sort">A flag indicating whether to sort the CSV files before comparison.</param>
    public CsvFileComparer(string filePath1, string filePath2, bool sort) : this(filePath1, filePath2)
    {
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, delimiter, 0, false));
        Logger.LogInfo("CSV sorting strategy configured.");
    }

    /// <summary>
    /// Initializes a new instance of the CsvFileComparer class with the specified file paths, a custom delimiter, and a flag indicating whether to sort the CSV files before comparison. This constructor allows for both customization of the CSV parsing through the delimiter and control over whether sorting is performed, providing flexibility in how the comparison is conducted based on the specific requirements of the CSV files being compared.
    /// </summary>
    /// <param name="filePath1">The path to the first CSV file.</param>
    /// <param name="filePath2">The path to the second CSV file.</param>
    /// <param name="delimiter">The delimiter used to separate fields in the CSV records.</param>
    /// <param name="shouldSort">A flag indicating whether to sort the CSV files before comparison.</param>
    public CsvFileComparer(string filePath1, string filePath2, string delimiter, bool shouldSort) : this(filePath1, filePath2, shouldSort)
    {
        this.delimiter = delimiter;
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, delimiter, 0, false));
        Logger.LogInfo("CSV sorting strategy configured with custom delimiter.");
    }

    /// <summary>
    /// Initializes a new instance of the CsvFileComparer class with the specified file paths, a custom delimiter, and flags indicating whether to sort and normalize the CSV files before comparison. This constructor provides the most comprehensive configuration options, allowing for customization of both the parsing and sorting behavior. The sorting strategy is configured to sort based on the first column (index 0) and includes an option for normalization, which can help ensure consistent sorting and comparison results by standardizing the data before processing.
    /// </summary>
    /// <param name="filePath1">The path to the first CSV file.</param>
    /// <param name="filePath2">The path to the second CSV file.</param>
    /// <param name="delimiter">The delimiter used to separate fields in the CSV records.</param>
    /// <param name="shouldSort">A flag indicating whether to sort the CSV files before comparison.</param>
    /// <param name="shouldNormalize">A flag indicating whether to normalize the CSV files before comparison.</param>
    public CsvFileComparer(string filePath1, 
        string filePath2, 
        string delimiter,
        bool shouldSort,
        bool shouldNormalize) : base(filePath1, filePath2)
    {
        this.delimiter = delimiter;
        this.shouldNormalize = shouldNormalize;
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, 
                                                    delimiter,
                                                    0, // for simplicity we are sorting based on first column, this can be extended to take sorting column number as input. 
                                                    shouldNormalize));

        Logger.LogInfo("CSV sorting strategy configured with normalization.");
    }

    /// <summary>
    /// Sort the CSV files using the configured sorting context. If a sorting context is defined, it will sort both CSV files in place, meaning that the original files will be overwritten with the sorted versions. The method includes logging to indicate when the sorting process starts and completes, as well as the time taken for the sorting operation. If no sorting context is defined, a warning is logged to indicate that sorting was not requested.
    /// </summary>
    private void Sort()
    {
        if (this.SortingContext != null)
        {
            // NOTE: outpath change for sorted files.
            Logger.LogInfo("Sorting CSV files started.");
            var stopwatch = Stopwatch.StartNew();
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempsorted");
            if (Directory.Exists(tempDirectory))
            {
                Logger.LogWarn("Existing Temp directory found. Deleting...");
                Directory.Delete(tempDirectory, true);
            }

            string sortedFile1Path = $"{Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(File1Path))}_sorted{Path.GetExtension(File1Path)}";
            this.SortingContext.Sort(File1Path, sortedFile1Path, ignoreHeader: !this.ignoreHeader);
            Logger.LogInfo($"Temporary sorted File Path: {sortedFile1Path}");
            File1Path = sortedFile1Path;
            
            string sortedFile2Path = $"{Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(File2Path))}_sorted{Path.GetExtension(File2Path)}";
            this.SortingContext.Sort(File2Path, sortedFile2Path, ignoreHeader: !this.ignoreHeader);
            File2Path = sortedFile2Path;

            Logger.LogInfo($"Temporary sorted File Path2: {sortedFile2Path}");
            stopwatch.Stop();
            Logger.LogInfo($"Sorting CSV files completed in {stopwatch.ElapsedMilliseconds} ms.");
        }
        else
        {
            Logger.LogWarn("Sorting was not requested. Comparison will proceed without sorting.");
        }
    }

    /// <summary>
    /// Compare the two CSV files. This method is responsible for orchestrating the comparison process, which includes sorting the files if a sorting context is defined, and then reading, parsing, normalizing, and comparing the records chunk by chunk. The actual comparison logic is not implemented yet, but the method includes logging to indicate the start of the comparison process and will throw a NotImplementedException to indicate that the functionality is still pending development.
    /// </summary>
    /// <param name="obj">An optional parameter that can be used to pass additional information required for the comparison.</param>
    /// <exception cref="NotImplementedException">Thrown to indicate that the CSV comparison functionality is not yet implemented.</exception>
    public override void Compare(object obj)
    {

        Logger.LogInfo($"Parameter details - File1: {this.File1Path}, File2: {this.File2Path}, Delimiter: {this.delimiter}, Normalize: {this.shouldNormalize}, IgnoreHeader: {this.ignoreHeader}");
        // handle headers i.e. read and store the header record if ignoreHeader is false, and configure the sorting strategy to ignore the header row when sorting the files. This ensures that the header does not affect the comparison results if it is present in both files. If ignoreHeader is true, then the first row of each CSV file will be treated as data rather than a header, and no special handling will be applied to it during sorting or comparison.
        if (this.ignoreHeader == false)
        {
            using (StreamReader reader1 = new StreamReader(this.File1Path))
            {
                string header1 = reader1.ReadLine();
                this.headerRecord1 = new CsvRecord(0, header1, delimiter, this.shouldNormalize);
            }

            using (StreamReader reader2 = new StreamReader(this.File2Path))
            {
                string header2 = reader2.ReadLine();
                this.headerRecord2 = new CsvRecord(0, header2, delimiter, this.shouldNormalize);
            }
        }

        this.Sort();
        try
        {
            if (!this.ignoreHeader && !headerRecord1.Equals(headerRecord2))
            {
                Logger.LogError("Header records are different. This may impact the comparison results.");
                headerRecord1.PrintWithDifferences(headerRecord2, FileName.File1);
                headerRecord2.PrintWithDifferences(headerRecord1, FileName.File2);
            }

            var start = Stopwatch.StartNew();
            CompareAllLines(Constants.ChunkSize);
            start.Stop();
            Logger.LogInfo($"CSV comparison completed in {start.ElapsedMilliseconds} ms.");
        }
        catch (FileNotFoundException e)
        {
            Logger.LogError($"File not found. Please check file path {e.FileName} ");
        }
        catch (Exception e)
        {
            Logger.LogError($"Unknown Exception caught {e.Message}");
        }
        finally
        {
            lock (activeWorkerLock)
            {
                while (countofActiveWorkers > 0)
                {
                    Monitor.Wait(activeWorkerLock);
                }
            }

        }
        
        PrintSummary();
    }

    /// <summary>
    /// Prints a summary of the CSV comparison results to the configured output destination.
    /// </summary>
    /// <remarks>This method generates and displays a summary based on the current record differences and
    /// output settings. It is intended for internal use and does not return any value or modify external state beyond
    /// printing the summary.</remarks>
    private void PrintSummary()
    {
        CsvComparisonSummary summary = new CsvComparisonSummary(recordDifferences, outputKind, this.OutputPath);
        
        summary.PrintSummary();
    }

    /// <summary>
    /// Compares lines from two files in parallel by processing them in chunks of the specified size.
    /// </summary>
    /// <remarks>This method reads lines from both files simultaneously and processes them in chunks using
    /// worker threads. The chunk size determines how many lines are compared at a time. The method is intended for use
    /// in scenarios where large files need to be compared efficiently. Thread safety is managed internally, but callers
    /// should ensure that the method is not invoked concurrently on the same instance.</remarks>
    /// <param name="obj">An object representing the chunk size. Must be an integer greater than zero, indicating the number of lines to
    /// process per chunk.</param>
    public void CompareAllLines(object obj)
    {
        int chunkSize = (int)obj;
        List<string> linesChunk1 = new List<string>();
        List<string> linesChunk2 = new List<string>();
        using (StreamReader reader1 = new StreamReader(this.File1Path))
        using (StreamReader reader2 = new StreamReader(this.File2Path))
        {
            int lineNumber = 0;
            string? line1 = null;
            string? line2 = null;
            while ((line1 = reader1.ReadLine()) != null &&
                (line2 = reader2.ReadLine()) != null)
            {

                linesChunk1.Add(line1);
                linesChunk2.Add(line2);

                lineNumber++;
                if (linesChunk1.Count == chunkSize || linesChunk2.Count == chunkSize)
                {
                    List<CsvRecord> chunk1ToProcess = linesChunk1.Select(line => new CsvRecord(lineNumber, line, delimiter, shouldNormalize)).ToList();
                    List<CsvRecord> chunk2ToProcess = linesChunk2.Select(line => new CsvRecord(lineNumber, line, delimiter, shouldNormalize)).ToList();
                    CsvChunkData csvChunkData = new CsvChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber);
                    while (countofActiveWorkers >= Constants.MaxThreadsCount)
                    {
                        // Wait for an active worker to finish before starting a new one
                    }

                    lock (activeWorkerLock)
                    {
                        ++countofActiveWorkers;
                    }

                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessCsvChunk), csvChunkData);
                    linesChunk1.Clear();
                    linesChunk2.Clear();
                }
            }

            if (linesChunk1.Count > 0 || linesChunk2.Count > 0)
            {
                List<CsvRecord> chunk1ToProcess = linesChunk1.Select(line => new CsvRecord(lineNumber, line, delimiter, shouldNormalize)).ToList();
                List<CsvRecord> chunk2ToProcess = linesChunk2.Select(line => new CsvRecord(lineNumber, line, delimiter, shouldNormalize)).ToList();
                CsvChunkData csvChunkData = new CsvChunkData(chunk1ToProcess, chunk2ToProcess, lineNumber);

                lock (activeWorkerLock)
                {
                    ++countofActiveWorkers;
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessCsvChunk), csvChunkData);
                linesChunk1.Clear();
                linesChunk2.Clear();
            }

            if (reader1.ReadLine() != null
                && reader2.ReadLine() == null) //There are still some lines left in File1
            {
                Logger.LogError("File2 is missing some records!!!");
            }
            else if (reader2.ReadLine() != null
                && reader1.ReadLine() == null) // There are still lines left in File2
            {
                Logger.LogError("File1 is missing some records!!!");
            }
        }
    }

    /// <summary>
    /// Processes a chunk of CSV records by comparing corresponding entries and enqueues any differences for further
    /// analysis.
    /// </summary>
    /// <remarks>This method is intended for use in multi-threaded scenarios where CSV data is processed in
    /// parallel. It decrements the active worker count and signals waiting threads when processing is complete. The
    /// differences between records are enqueued for subsequent handling.</remarks>
    /// <param name="obj">An object containing the data required to process the CSV chunk. Must be of type CsvChunkData.</param>
    public static void ProcessCsvChunk(object obj)
    {
        CsvChunkData csvChunkData = (CsvChunkData)obj;
        // Process the chunk of CSV records and compare them
        // For each pair of records that are different, enqueue the difference in recordDifferences
        // ...
        for (int index=0;index<csvChunkData.Records1.Count; index++)
        {
            CsvRecord record1 = csvChunkData.Records1[index];
            CsvRecord record2 = csvChunkData.Records2[index];
            if (!record1.Equals(record2))
            {
                recordDifferences.Enqueue(new Pair<CsvRecord, CsvRecord>(record1, record2));
            }
        }

        lock (activeWorkerLock)
        {
            --countofActiveWorkers;
            Monitor.Pulse(activeWorkerLock); // Notify waiting threads that a worker has finished
        }
    }
}