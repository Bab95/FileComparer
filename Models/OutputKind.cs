namespace FileComparer.Models
{
    /// <summary>
    /// Specifies the available output destinations for generated content.
    /// </summary>
    /// <remarks>Use this enumeration to indicate whether content should be written to the console or to a
    /// file. The value determines the output behavior of the associated operation.</remarks>
    public enum OutputKind
    {
        /// <summary>
        /// Gets or sets a value indicating whether output should be written to the console.
        /// </summary>
        OnConsole,
        /// <summary>
        /// Provides functionality for writing data to files.
        /// </summary>
        FileWriting
    }
}
