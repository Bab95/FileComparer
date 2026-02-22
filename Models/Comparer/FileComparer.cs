namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Provides an abstract base class for comparing two files and determining their similarity or differences.
    /// </summary>
    /// <remarks>FileComparer defines the contract for file comparison operations, including properties for
    /// specifying input and output file paths, and a flag indicating whether the files are considered the same. Derived
    /// classes should implement the Compare method to perform the actual comparison logic. This class is intended for
    /// scenarios where file comparison and result output are required, such as validation, synchronization, or
    /// reporting.</remarks>
    public abstract class FileComparer : IComparer
    {
        /// <summary>
        /// Gets or sets the file system path for the first input file.
        /// </summary>
        public string File1Path { get; set; }

        /// <summary>
        /// Gets or sets the file system path for the second file used in the operation.
        /// </summary>
        public string File2Path { get; set; }

        /// <summary>
        /// Gets or sets the output directory path where generated files will be saved.
        /// </summary>
        /// <remarks>The specified path should be a valid directory. If the directory does not exist, it
        /// may need to be created before saving files. Relative paths are resolved based on the application's working
        /// directory.</remarks>
        public string OutPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the compared files have identical content.
        /// </summary>
        public bool AreFilesSame { get; internal set; }

        /// <summary>
        /// Gets or sets the summary information associated with the current object.
        /// </summary>
        public Summary summary { get; set; }

        /// <summary>
        /// Compares the current instance with the specified object to determine their relative values or states.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. Cannot be null.</param>
        public abstract void Compare(object obj);
    }
}
