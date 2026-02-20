using System;
using System.Collections.Generic;

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
        /// List of fields parsed from the CSV record. Each field is a string representing a value from the record,
        /// split based on the specified delimiter.
        /// </summary>
        public List<string> Fields { get; }

        /// <summary>
        /// Constructs a new instance of the CsvRecord class with the specified record ID, record string, and normalization option.
        /// </summary>
        /// <param name="recordId">The unique identifier for the CSV record.</param>
        /// <param name="record">The CSV record string.</param>
        /// <param name="normalize">Indicates whether to normalize the fields.</param>
        public CsvRecord(long recordId, 
            string record, 
            bool normalize)
        {
            RecordId = recordId;
            Fields = CsvRecord.ParseRecord(record,delimiter, normalize);
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
                fields.Add(NormalizeField(field));
            }

            return fields;
        }

        /// <summary>
        /// Normalizes a field value by trimming whitespace and converting it to a consistent case (e.g., lowercase).
        /// </summary>
        /// <param name="field">The field value to normalize.</param>
        /// <returns>The normalized field value.</returns>
        private static string NormalizeField(string field)
        {
            // Implement Normalization logic as needed
            return field;
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
    }
}
