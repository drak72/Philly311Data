using System.Text.Json;
using DuckDB.NET.Data;

namespace Duck
{
    public class Utils
    {
        public static string SerializeResponse(DuckDBDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value ?? "null";
                }
                results.Add(row);
            }

            return JsonSerializer.Serialize(results, new JsonSerializerOptions { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static List<Dictionary<string, object>> ReadResponse(DuckDBDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value ?? "null";
                }
                results.Add(row);
            }
            return results;
        }
    }
}