namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Provides functionality for comparing PDF files by extending the base file comparison operations.
    /// </summary>
    /// <remarks>Use this class to implement custom logic for comparing PDF documents. Inherit from
    /// PdfComparer to override comparison behavior specific to PDF files. This class is intended for scenarios where
    /// specialized PDF comparison is required beyond generic file comparison.</remarks>
    public class PdfComparer : FileComparer
    {
        /// <summary>
        /// Compares the current instance with the specified object to determine their relative order.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <exception cref="NotImplementedException">Thrown to indicate that the method is not implemented.</exception>
        public override void Compare(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
