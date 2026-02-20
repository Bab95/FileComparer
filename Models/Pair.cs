using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    /// <summary>
    /// Implementation of a generic pair class to hold two related objects.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first object.</typeparam>
    /// <typeparam name="TSecond">The type of the second object.</typeparam>
    public sealed class Pair <TFirst, TSecond>
    {
        /// <summary>
        /// Gets the first element of the pair.
        /// </summary>
        public TFirst First { get; }

        /// <summary>
        /// Gets the second element of the pair.
        /// </summary>
        public TSecond Second { get; }

        /// <summary>
        /// Constructs a new instance of the Pair class with the specified first and second elements.
        /// </summary>
        /// <param name="first">The first element of the pair.</param>
        /// <param name="second">The second element of the pair.</param>
        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        /// <summary>
        /// Returns a string that represents the current pair in the format "(First, Second)".
        /// </summary>
        /// <returns>A string representation of the pair, with the first and second elements separated by a comma and enclosed in
        /// parentheses. If either element is null, its string representation will be omitted.</returns>
        public override string ToString()
        {
            return "(" + First?.ToString() + ", " + Second?.ToString() + ")";
        }

        /// <summary>
        /// Returns a hash code for the current pair. The hash code is based on the hash codes of the first and second elements.
        /// </summary>
        /// <returns>A hash code for the current pair.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(First, Second);
        }
    }
}
