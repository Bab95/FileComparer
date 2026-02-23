namespace FileComparer.Models.Sorting
{
    /// <summary>
    /// Provides a context for performing sorting operations using a specified sorting strategy.
    /// </summary>
    /// <remarks>The sorting strategy is supplied via the constructor and determines how sorting is performed.
    /// Use this class to delegate sorting tasks to different strategies without modifying client code. This class is
    /// not thread-safe.</remarks>
    public class SortingContext
    {
        /// <summary>
        /// Sorting strategy 
        /// </summary>
        private readonly ISortingStrategy sortingStrategy;

        /// <summary>
        /// Initializes a new instance of the SortingContext class using the specified sorting strategy.
        /// </summary>
        /// <remarks>The provided sorting strategy determines how items will be sorted within the context.
        /// To change the sorting behavior, supply a different implementation of ISortingStrategy.</remarks>
        /// <param name="sortingStrategy">The sorting strategy to be used for sorting operations. Cannot be null.</param>
        public SortingContext(ISortingStrategy sortingStrategy)
        {
            this.sortingStrategy = sortingStrategy;
        }

        /// <summary>
        /// Sorts the contents of the specified input file and writes the sorted results to the specified output file.
        /// </summary>
        /// <remarks>The sorting behavior depends on the configured sorting strategy. The method does not
        /// modify the input file; it creates or overwrites the output file with sorted data. Ensure that the output
        /// file path is writable and that the input file exists and is accessible.</remarks>
        /// <param name="inputFilePath">The path to the file containing the data to be sorted. Cannot be null or empty.</param>
        /// <param name="outputFilePath">The path to the file where the sorted data will be written. Cannot be null or empty.</param>
        /// <param name="ignoreHeader">A flag indicating whether to ignore the first line of the input file, typically used to skip headers during sorting.</param>
        public void Sort(string inputFilePath, string outputFilePath, bool ignoreHeader = true)
        {
            sortingStrategy.Sort(inputFilePath, outputFilePath, ignoreHeader);
        }
    }
}
