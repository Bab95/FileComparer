namespace FileComparer.Models
{
    /// <summary>
    /// Specifies the available file options for operations that require a file selection.
    /// </summary>
    /// <remarks>The FileName enumeration provides named constants to represent specific files or the absence
    /// of a file. Use this enum to indicate which file to process or to signal that no file is selected.</remarks>
    public enum FileName
    {
        /// <summary>
        /// Represents the first file in a collection or sequence.
        /// </summary>
        File1,
        /// <summary>
        /// Represents a file and provides access to its properties and operations.
        /// </summary>
        File2,
        /// <summary>
        /// Represents the absence of a file in the current context.
        /// </summary>
        NoFile
    }
}
