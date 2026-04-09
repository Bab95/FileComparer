using System;

namespace FileComparer.Models.Comparer
{
    /// <summary>
    /// Provides functionality for comparing Excel files by extending the base file comparison operations.
    /// </summary>
    /// <remarks>Use this class to implement custom comparison logic for Excel documents. Inherit from
    /// ExcelComparer to define how Excel files should be compared within your application. This type is intended for
    /// scenarios where specialized handling of Excel file formats is required.</remarks>
    public class ExcelComparer : FileComparer
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
