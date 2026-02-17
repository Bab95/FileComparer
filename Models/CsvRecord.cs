using System;
using System.Collections.Generic;

namespace FileComparer.Models
{
    public class CsvRecord
    {
        public long RecordId { get; }
        public List<string> Fields { get; }

        public CsvRecord(long recordId, IEnumerable<string> fields, bool normalize, Dictionary<string, string> normalizedCache)
        {
            RecordId = recordId;
            Fields = new List<string>();

            foreach (var field in fields)
            {
                Fields.Add(normalize ? NormalizeField(field, normalizedCache) : field ?? string.Empty);
            }
        }

        public string GetField(int index)
        {
            if (index < 0 || index >= Fields.Count)
            {
                return string.Empty;
            }

            return Fields[index];
        }

        private static string NormalizeField(string field, Dictionary<string, string> normalizedCache)
        {
            var normalized = (field ?? string.Empty).Trim();
            if (normalizedCache.TryGetValue(normalized, out var cached))
            {
                return cached;
            }

            normalizedCache[normalized] = normalized;
            return normalized;
        }
    }
}
