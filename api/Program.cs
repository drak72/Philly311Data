using DuckDB.NET.Data;
using System.Net.Mime;
using opendata.lib;
using opendata.lib.prompts;

/** Create API Builder */
var builder = WebApplication.CreateBuilder(args);

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

/** Register minimal root endpoint */
app.MapGet("/", static async (string question) =>
{
 
    /** Create & Open Connection to local (file)Duck DB */
    using var connection = new DuckDBConnection("Data Source=mydata.duckdb");
    connection.Open();
    var createIfNotExists = connection.CreateCommand();
    createIfNotExists.CommandText = 
        "CREATE TABLE IF NOT EXISTS public_cases_fc \n" +
        "(\n" +
            "ObjectID INTEGER PRIMARY KEY, \n" + 
            "service_request_id INTEGER, \n" + 
            "subject VARCHAR, \n" + 
            "status VARCHAR, \n" + 
            "status_notes VARCHAR, \n" + 
            "service_name VARCHAR, \n" + 
            "service_code VARCHAR, \n" + 
            "agency_responsible VARCHAR, \n" + 
            "service_notice VARCHAR, \n" + 
            "requested_datetime TIMESTAMP, \n" + 
            "updated_datetime TIMESTAMP, \n" + 
            "expected_datetime TIMESTAMP, \n" + 
            "closed_datetime TIMESTAMP, \n" + 
            "address VARCHAR, \n" + 
            "zipcode VARCHAR, \n" + 
            "media_url VARCHAR, \n" + 
            "lat DOUBLE, \n" + 
            "lon DOUBLE \n" + 
        ")";
    createIfNotExists.ExecuteNonQuery();

 
    /** Obtain current highest local PK & Query for all records > this ID to update our local cache from the carto csv*/
    var getHighestLocalId = connection.CreateCommand();
    getHighestLocalId.CommandText = 
        "SELECT \n" + 
            "CASE \n" + 
                "WHEN MAX(ObjectID) IS NULL \n" + 
                "THEN -1 \n" + 
                "ELSE MAX(ObjectID) \n" + 
            "END \n" + 
            "AS max_object_id \n" + 
        "FROM public_cases_fc";
    
    var highestIdReader = getHighestLocalId.ExecuteReader();
    var highestIdJson =  Duck.Utils.ReadResponse(highestIdReader)[0]["max_object_id"];

    /** Fetch latest data from Carto and add to local db */
    var fetchLatest = connection.CreateCommand();
    fetchLatest.CommandText = 
        "COPY public_cases_fc " + 
        "FROM 'https://phl.carto.com/api/v2/sql?" +
        "filename=public_cases_fc" + 
        "&format=csv" + 
        "&skipfields=cartodb_id,the_geom,the_geom_webmercator" + 
        "&q=SELECT%20*%20FROM%20public_cases_fc%20WHERE%20ObjectID%20%3E%20" + 
        highestIdJson + "'";   
    
    var newRows = fetchLatest.ExecuteReader();
    Console.WriteLine("Fetched " + newRows + " new rows");

    /** Define base (system) prompt for messages | Prepared for future abstraction*/
    var messages = SystemPrompt.ConstructQuery(question, DataSource.I311Requests);

    try
    {
        /** Create Agent */
        var agent = new Agent();

        /** Define Local Duck DB Query */
        using var cmd = connection.CreateCommand();
        cmd.CommandText = await agent.Converse(messages); // Get the query
        var reader = cmd.ExecuteReader(); // Execute the query
        var queryJson = Duck.Utils.SerializeResponse(reader); // Serialize the results


        /** Add Query Results to the Agent conversation */
        messages.Add(new()
        {
            role = "user",
            content = $"Here are the results of your query: {queryJson} \n" +
                    "Summarize the results in a few sentences. Using your knowledge of Phildelphia: \n" +
                    "what inferences can you make from these results? \n" +
                    "What might be interesting about these results? \n" +
                    "return your response as a JSON string[] with each element being a sentence" +
                    "respond only with the JSON; other text will cause an error"
        });

        var res = await agent.Converse(messages);
        return Results.Content(res, MediaTypeNames.Application.Json);
    }
    catch (Exception e)
    {

        return Results.Content(e.Message, MediaTypeNames.Text.Plain);
    }
});

app.Run();
