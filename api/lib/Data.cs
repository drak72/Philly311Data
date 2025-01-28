using opendata.lib.schemas;

namespace opendata.lib {
    public class Column
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
    };

    public class Schema
    {
        public List<Column> Columns { get; set; } = new();
    }

    public enum DataSource {
        I311Requests,
    }


    public static class Data {
        public static string ToJson(DataSource source) {
            return source switch {
                /** Register new data sources here, for now. */
                DataSource.I311Requests => I311Schema.ToJson(),
                _ => throw new ArgumentException(
                    "Invalid data source: " + source + 
                    "valid values are: " + string.Join(", ", Enum.GetValues(typeof(DataSource)))
                )
            };
        }
    }
}