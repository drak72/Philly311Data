using DuckDB.NET.Data;
using System.Text.Json;
using System.Net.Mime;
using Bedrock;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime.Internal;

var builder = WebApplication.CreateBuilder(args);
/**
  Make it work
  Make it good
  Make it fast 
*/


/** Set AWS Profile */
if (builder.Environment.IsDevelopment())
{
    Environment.SetEnvironmentVariable("AWS_PROFILE", "personal");
}

/** Add Cors Policy */
builder.Services.AddCors(static options =>
{
    options.AddPolicy("AllowAllOrigins", static builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

/** Build Services */
var app = builder.Build();

/** Register Cors Policy */
app.UseCors("AllowAllOrigins");

/** Register minimal endpoint */
app.MapGet("/", static async (string question) =>
{
    /** Create & Open Connection */
    using var connection = new DuckDBConnection("Data Source=mydata.duckdb");
    connection.Open();
    var createIfNotExists = connection.CreateCommand();
    createIfNotExists.CommandText = "CREATE TABLE IF NOT EXISTS public_cases_fc (ObjectID INTEGER PRIMARY KEY, service_request_id INTEGER, subject VARCHAR, status VARCHAR, status_notes VARCHAR, service_name VARCHAR, service_code VARCHAR, agency_responsible VARCHAR, service_notice VARCHAR, requested_datetime TIMESTAMP, updated_datetime TIMESTAMP, expected_datetime TIMESTAMP, closed_datetime TIMESTAMP, address VARCHAR, zipcode VARCHAR, media_url VARCHAR, lat DOUBLE, lon DOUBLE)";
    createIfNotExists.ExecuteNonQuery();

 
    /** Obtain current highest identifier */
    var highestId = connection.CreateCommand();
    highestId.CommandText = "SELECT CASE WHEN MAX(ObjectID) IS NULL THEN -1 ELSE MAX(ObjectID) END AS max_object_id FROM public_cases_fc";
    var highestIdReader = highestId.ExecuteReader();
    var highestIdJson = ReadDuckDbResults(highestIdReader)[0]["max_object_id"];
    Console.WriteLine(highestIdJson);

    var fetchLatest = connection.CreateCommand();
    //'https://phl.carto.com/api/v2/sql?filename=public_cases_fc&format=csv&skipfields=cartodb_id,the_geom,the_geom_webmercator&q=SELECT%20*%20FROM%20public_cases_fc%20WHERE%20requested_datetime%20%3E=%20%272025-01-01%27%20AND%20requested_datetime%20%3C%20%272026-01-01%27%20AND%20ObjectID%20%3E%20" + highestIdJson + ")' TIMESTAMPFORMAT='%Y-%m-%d %H:%M:%S'
    var query = "COPY public_cases_fc FROM 'https://phl.carto.com/api/v2/sql?filename=public_cases_fc&format=csv&skipfields=cartodb_id,the_geom,the_geom_webmercator&q=SELECT%20*%20FROM%20public_cases_fc%20WHERE%20ObjectID%20%3E%20" + highestIdJson + "'";
    Console.WriteLine(query);
    fetchLatest.CommandText = query;
    var fetchLatestReader = fetchLatest.ExecuteReader();

    var bedrock = new Bedrock.Bedrock();
    // Ideally create a crawler that can get the schema from the url; catalogue would be even better. 
    // Or use AWS Glue
    var schema = new Schema
    {
        // TODO: Copy file to local if not present? Check last record?
        Columns =
        [
            new Column { Name = "Objectid", Type = "INTEGER", Description = "The unique identifier for the record." },
            new Column { Name = "service_request_id", Type = "INTEGER", Description = "The unique identifier for the service request." },
            new Column { Name = "subject", Type = "VARCHAR", Description = "The subject of the service request." },
            new Column { Name = "status", Type = "VARCHAR", Description = "The status of the service request. This can be a value of 'Open' or 'Closed'." },
            new Column { Name = "status_notes", Type = "VARCHAR", Description = "Additional notes about the status of the service request." },
            new Column { Name = "service_name", Type = "VARCHAR", Description = "The name of the service request." },
            new Column { Name = "service_code", Type = "VARCHAR", Description = "Code attached to the service type - format of SR-{Program}{code}" },
            new Column { Name = "agency_responsible", Type = "VARCHAR", Description = "The agency responsible for the service request." },
            new Column { Name = "service_notice", Type = "VARCHAR", Description = "Number of business days to respond to the request." },
            new Column { Name = "requested_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was made." },
            new Column { Name = "updated_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was last updated." },
            new Column { Name = "expected_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was expected to be closed." },
            new Column { Name = "closed_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was closed." },
            new Column { Name = "address", Type = "VARCHAR", Description = "The address of the service request." },
            new Column { Name = "zipcode", Type = "INTEGER", Description = "The zip code of the service request." },
            new Column { Name = "media_url", Type = "VARCHAR", Description = "The media url of images attached to the service request." },
            new Column { Name = "lat", Type = "DOUBLE", Description = "The latitude of the service request." },
            new Column { Name = "lon", Type = "DOUBLE", Description = "The longitude of the service request." },
        ]
    };
    var schemaJson = JsonSerializer.Serialize(schema);

    /** Define base prompt for messages */
    var messages = new List<InvokeMessage>()
    {
        new() {
            role = "assistant",
            content = "You are a helpful assistant that can turn a user's natural language question prompt into a duck DB query. Duck DB is mostly postgres SQL compatible; \n" +
                    $"for a file with the following schema: {schemaJson} \n" +
                    $"construct the query with FROM public_cases_fc"
        },
        new() {
            role = "user",
            content = "Write a duck DB query to answer the following question: " + question + 
                " Format the query as a single line of SQL.\n" +
                "Otherwise, return only the query as a string"
        }
    };

    try
    {
        Console.WriteLine("Bedrock Prompt: " + question);
        var response = await bedrock.Converse(messages);
        Console.WriteLine("Bedrock Response: " + response);

        /** Execute Query if valid - how to best sanitize? */
        using var cmd = connection.CreateCommand();
        cmd.CommandText = response;
        var reader = cmd.ExecuteReader();
        var queryJson = SerializeDuckDbResults(reader);
        Console.WriteLine("Query Results: " + queryJson);

        messages.Add(new()
        {
            role = "user",
            content = $"Here are the results of your query: {queryJson} \n" +
                    "Summarize the results in a few sentences. Using your knowledge of Phildelphia, \n" +
                    "what inferences can you make from these results? \n" +
                    "What might be interesting about these results? \n" +
                    "return your response as a JSON string[] with each element being a sentence" +
                    "If the results are empty, offer polite apologies and explain why the question may not have been answered. \n"
        });

        var resultsResponse = await bedrock.Converse(messages);
        Console.WriteLine("Bedrock Results Response: " + resultsResponse);
        return Results.Content(resultsResponse, MediaTypeNames.Application.Json);
    }
    catch (Exception e)
    {

        return Results.Content(e.Message, MediaTypeNames.Text.Plain);
    }
});


static string SerializeDuckDbResults(DuckDBDataReader reader)
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

    return JsonSerializer.Serialize(results, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
}

static List<Dictionary<string, object>> ReadDuckDbResults(DuckDBDataReader reader)    
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

app.Run();
