using System.Globalization;
using System.Text.RegularExpressions;

namespace FileComparer.Models
{
    /// <summary>
    /// Represents a single record from a CSV (comma-separated values or other delimitr) file, providing access to its fields and
    /// associated metadata.
    /// </summary>
    /// <remarks>The CsvRecord class parses a CSV record string into individual fields using the specified
    /// delimiter. It exposes the parsed fields as a read-only list and provides access to the record's unique
    /// identifier. The delimiter can be set after construction to control how fields are separated. This class does not
    /// perform advanced CSV parsing such as handling quoted fields or escaped delimiters.</remarks>
    public class CsvRecord
    {
        /// <summary>
        /// RecordId is a unique identifier for the CSV record, 
        /// typically representing its line number in the source file. 
        /// It allows for easy reference and tracking of records during 
        /// processing and comparison operations.
        /// </summary>
        public long RecordId { get; }

        /// <summary>
        /// delimiter is the character or string used to separate fields in the CSV record.
        /// </summary>
        public string delimiter { get; set; }

        /// <summary>
        /// Actual Raw fields after parsing the record.
        /// </summary>
        public List<string> Fields { get; }

        /// <summary>
        /// List of fields parsed from the CSV record. Each field is a string representing a value from the record,
        /// split based on the specified delimiter.
        /// </summary>
        public List<string> NormalizedField { get; }

        /// <summary>
        /// Constructs a new instance of the CsvRecord class with the specified record ID, record string, and normalization option.
        /// </summary>
        /// <param name="recordId">The unique identifier for the CSV record.</param>
        /// <param name="record">The CSV record string.</param>
        /// <param name="normalize">Indicates whether to normalize the fields.</param>
        public CsvRecord(long recordId, 
            string record,
            string delimiter,
            bool normalize)
        {
            this.delimiter = delimiter;
            RecordId = recordId;
            NormalizedField = CsvRecord.ParseRecord(record, delimiter, normalize);
            Fields = CsvRecord.ParseRecord(record, delimiter, false);
        }

        /// <summary>
        /// Parse the CSV record string into individual fields based on the specified delimiter and normalization option.
        /// </summary>
        /// <param name="record">The CSV record string to parse.</param>
        /// <param name="delimiter">The delimiter used to separate fields in the record.</param>
        /// <param name="normalize">Indicates whether to normalize the fields.</param>
        /// <returns>A list of parsed fields from the CSV record.</returns>
        private static List<string> ParseRecord(string record, string delimiter, bool normalize)
        {
            var fields = new List<string>();
            var rawFields = record.Split(new string[] { delimiter }, StringSplitOptions.None);
            foreach (var field in rawFields)
            {
                fields.Add(normalize ? NormalizeField(field) : field);
            }

            return fields;
        }

        /// <summary>
        /// Prints the current CSV record to the console, highlighting fields that differ from the specified record.
        /// </summary>
        /// <remarks>Fields that are different from the corresponding fields in the specified record are
        /// displayed in red. The output includes a timestamp and thread identifier for context. This method is intended
        /// for diagnostic or informational purposes and writes directly to the console.</remarks>
        /// <param name="other">The CSV record to compare against. Fields that differ from this record will be visually highlighted in the
        /// output.</param>
        public void PrintWithDifferences(CsvRecord other, FileName fileName)
        {
            var differenceConsoleColor = ConsoleColor.Green;

            if (fileName == FileName.File2)
            {
                differenceConsoleColor = ConsoleColor.Red;
            }

            Console.Write($"[Info] {fileName}:");

            for (int i = 0; i < NormalizedField.Count; i++)
            {
                string fieldValue = GetNormalizedField(i);
                string otherFieldValue = other.GetNormalizedField(i);
                if (string.Equals(fieldValue, otherFieldValue, StringComparison.Ordinal))
                {
                    Console.Write($"{fieldValue}{delimiter}");
                }
                else
                {
                    Console.ForegroundColor = differenceConsoleColor;
                    Console.Write($"{fieldValue}");
                    Console.ResetColor();
                    Console.Write($"{delimiter}");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current CsvRecord instance based on the normalized
        /// field values.
        /// </summary>
        /// <remarks>Equality is determined by comparing the count and values of normalized fields using
        /// ordinal string comparison. This method is intended for value-based comparison of CsvRecord
        /// instances.</remarks>
        /// <param name="obj">The object to compare with the current CsvRecord. Can be null or any object type.</param>
        /// <returns>true if the specified object is a CsvRecord and its normalized fields are equal to those of the current
        /// instance; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is CsvRecord other)
            {
                if (NormalizedField.Count != other.NormalizedField.Count)
                {
                    return false;
                }

                for (int i = 0; i < NormalizedField.Count; i++)
                {
                    if (!string.Equals(NormalizedField[i], other.NormalizedField[i], StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a string representation of the record by concatenating all normalized fields, separated by the
        /// specified delimiter.
        /// </summary>
        /// <remarks>This method is useful for serializing the record into a delimited format, such as
        /// CSV. The order of fields in the output matches their order in the normalized field collection.</remarks>
        /// <returns>A string containing all normalized fields joined by the delimiter. If there are no fields, returns an empty
        /// string.</returns>
        public override string ToString()
        {
            string record = string.Empty;

            for (int i = 0; i < Fields.Count; i++)
            {
                record += GetField(i);
                if (i < Fields.Count - 1)
                {
                    record += delimiter;
                }
            }

            return record;
        }

        /// <summary>
        /// Normalizes a field value by trimming whitespace and converting it to a consistent case (e.g., lowercase).
        /// </summary>
        /// <param name="field">The field value to normalize.</param>
        /// <returns>The normalized field value.</returns>
        private static string NormalizeField(string field)
        {
            if (field is null)
            {
                return string.Empty;
            }

            var trimmed = field.Trim();
            if ((trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) ||
                (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();
            }

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            var lower = trimmed.ToLowerInvariant();
            if (lower == "null" || lower == "na" || lower == "n/a")
            {
                return string.Empty;
            }

            if (TryNormalizeDate(trimmed, out var normalizedDate))
            {
                return normalizedDate;
            }

            if (TryNormalizePercentage(trimmed, out var normalizedPercentage))
            {
                return normalizedPercentage;
            }

            if (TryNormalizeMoney(trimmed, out var normalizedMoney))
            {
                return normalizedMoney;
            }

            return lower.Trim();
        }

        /// <summary>
        /// Attempts to parse the specified date string and, if successful, outputs the date in normalized ISO format
        /// (yyyy-MM-dd).
        /// </summary>
        /// <remarks>Parsing is performed using both the invariant and current culture settings. The
        /// normalized output uses the invariant culture format. This method does not throw exceptions for invalid
        /// input.</remarks>
        /// <param name="value">The date string to parse. Can be in a format recognized by either the invariant or current culture.</param>
        /// <param name="normalized">When this method returns, contains the normalized date string in yyyy-MM-dd format if parsing succeeds;
        /// otherwise, contains an empty string.</param>
        /// <returns>true if the date string was successfully parsed and normalized; otherwise, false.</returns>
        private static bool TryNormalizeDate(string value, out string normalized)
        {
            normalized = string.Empty;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed) ||
                DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
            {
                normalized = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to normalize a percentage string to a standard format.
        /// </summary>
        /// <remarks>The normalized output will always include the numeric value followed by a percent
        /// sign, with any extraneous whitespace or percent signs removed. This method does not validate the numeric
        /// range of the percentage.</remarks>
        /// <param name="value">The input string representing a percentage value. Must contain a percent sign ('%') and a valid numeric
        /// value.</param>
        /// <param name="normalized">When this method returns <see langword="true"/>, contains the normalized percentage string in the format
        /// "number%"; otherwise, contains an empty string.</param>
        /// <returns><see langword="true"/> if the input string was successfully normalized; otherwise, <see langword="false"/>.</returns>
        private static bool TryNormalizePercentage(string value, out string normalized)
        {
            normalized = string.Empty;
            if (!value.Contains('%'))
            {
                return false;
            }

            var match = Regex.Match(value, @"^\s*%?\s*(?<number>[+-]?(?:\d+(?:\.\d+)?|\.\d+))\s*%?\s*$");
            if (!match.Success)
            {
                return false;
            }

            normalized = $"{match.Groups["number"].Value}%";
            return true;
        }

        /// <summary>
        /// Attempts to parse and normalize a monetary value from the specified string.
        /// </summary>
        /// <remarks>The method recognizes currency symbols from Unicode and supports numbers with
        /// optional signs and decimal points. The normalized result always places the currency symbol before the number
        /// and converts the output to lowercase for consistency.</remarks>
        /// <param name="value">The input string containing a monetary value, which may include a currency symbol and a numeric amount.
        /// Leading and trailing whitespace are ignored.</param>
        /// <param name="normalized">When this method returns <see langword="true"/>, contains the normalized monetary value in the format
        /// "currency symbol" followed by the number, in lowercase. Otherwise, contains an empty string.</param>
        /// <returns><see langword="true"/> if the input string was successfully parsed and normalized; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool TryNormalizeMoney(string value, out string normalized)
        {
            normalized = string.Empty;
            var match = Regex.Match(value, @"^\s*(?<currency>[\p{Sc}])\s*(?<number>[+-]?(?:\d+(?:\.\d+)?|\.\d+))\s*$|^\s*(?<number>[+-]?(?:\d+(?:\.\d+)?|\.\d+))\s*(?<currency>[\p{Sc}])\s*$");
            if (!match.Success)
            {
                return false;
            }

            var currency = match.Groups["currency"].Value;
            var number = match.Groups["number"].Value;
            normalized = $"{currency}{number}".ToLowerInvariant();
            return true;
        }

        /// <summary>
        /// gets the field value at the specified index. If the index is out of range, 
        /// it returns an empty string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetField(int index)
        {
            if (index < 0 || index >= Fields.Count)
            {
                return string.Empty;
            }

            return Fields[index];
        }

        /// <summary>
        /// gets the normalized field value at the specified index. If the index is out of range, 
        /// it returns an empty string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetNormalizedField(int index)
        {
            if (index < 0 || index >= NormalizedField.Count)
            {
                return string.Empty;
            }

            return NormalizedField[index];
        }
    }
}
