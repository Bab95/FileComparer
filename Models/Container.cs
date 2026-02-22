namespace FileComparer.Models
{
    /// <summary>
    /// Container is a class that encapsulates a StreamReader and the current line being read from a file.
    /// </summary>
    class Container : IComparable<Container>
    {
        /// <summary>
        /// The current line being read from the file.
        /// </summary>
        public string currenLine { get; set; }
        /// <summary>
        /// The StreamReader used to read the file.
        /// </summary>
        public StreamReader Reader { get; set; }

        /// <summary>
        /// Initializes a new instance of the Container class with the specified StreamReader. The constructor sets the Reader property to the provided StreamReader, allowing the Container to read lines from the associated file. The currenLine property can be updated as lines are read from the file using the Reader. 
        /// This class is designed
        /// </summary>
        /// <param name="reader"></param>
        public Container(StreamReader reader)
        {
            this.Reader = reader;
        }

        /// <summary>
        /// Compares the current Container instance to another Container and returns a value indicating their relative
        /// order based on the currenLine property.
        /// </summary>
        /// <remarks>Comparison is performed using a string comparison of the currenLine property. This
        /// method can be used for sorting or ordering Container instances.</remarks>
        /// <param name="other">The Container instance to compare with the current instance. Can be null, in which case the current instance
        /// is considered greater.</param>
        /// <returns>A value less than zero if the current instance precedes other; zero if they are equal; or greater than zero
        /// if the current instance follows other.</returns>
        public int CompareTo(Container? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(this.currenLine, other.currenLine);
        }
    }
}
