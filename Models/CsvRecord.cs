using System;
using System.Collections.Generic;
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

        public void PrintWithDifferences(CsvRecord other)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.Write($"[{timestamp}] [Thread {threadId}] [Info] File2:");

            for (int i = 0; i < NormalizedField.Count; i++)
            {
                string fieldValue = GetField(i);
                string otherFieldValue = other.GetField(i);
                if (string.Equals(fieldValue, otherFieldValue, StringComparison.Ordinal))
                {
                    Console.Write($"{fieldValue}{delimiter}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{fieldValue}");
                    Console.ResetColor();
                    Console.Write($"{delimiter}");
                }
            }
            Console.WriteLine();
        }

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

        public override string ToString()
        {
            string record = string.Empty;

            for (int i = 0; i < NormalizedField.Count; i++)
            {
                record += GetField(i);
                if (i < NormalizedField.Count - 1)
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
            if (index < 0 || index >= NormalizedField.Count)
            {
                return string.Empty;
            }

            return NormalizedField[index];
        }
    }
}
