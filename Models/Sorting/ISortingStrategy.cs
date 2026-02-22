namespace FileComparer.Models.Sorting
{
    /// <summary>
    /// Defines a contract for sorting data from an input file and writing the sorted results to an output file.
    /// </summary>
    /// <remarks>Implementations of this interface provide specific sorting algorithms or strategies. The
    /// input and output file paths must refer to accessible files. Thread safety and performance characteristics depend
    /// on the concrete implementation.</remarks>
    public interface ISortingStrategy
    {
        /// <summary>
        /// Sorts the contents of the specified input file and writes the sorted results to the specified output file.
        /// </summary>
        /// <remarks>Both input and output files must be accessible for reading and writing, respectively.
        /// The method does not modify the input file. If the output file cannot be created or written to, an exception
        /// may be thrown.</remarks>
        /// <param name="inputFilePath">The path to the file containing the data to be sorted. Must refer to an existing file.</param>
        /// <param name="outputFilePath">The path to the file where the sorted data will be written. If the file exists, it will be overwritten.</param>
        void Sort(string inputFilePath, string outputFilePath);
    }
}
