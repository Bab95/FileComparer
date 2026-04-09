namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Defines a method for comparing objects to determine their ordering or equality.
    /// </summary>
    /// <remarks>Implementations of this interface provide custom comparison logic for objects, which can be
    /// used in sorting, searching, or other operations that require object comparison. The specific comparison criteria
    /// depend on the implementation.</remarks>
    public interface IComparer
    {
        /// <summary>
        /// Compares the current instance with the specified object to determine their relative values or states.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. Cannot be null.</param>
        void Compare(object obj);
    }
}
