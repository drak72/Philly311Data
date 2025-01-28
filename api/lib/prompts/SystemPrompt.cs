namespace opendata.lib.prompts {
    public class SystemPrompt {
        public static List<InvokeMessage> ConstructQuery(string question, DataSource source) {
            return
            [
                new() {
                    role = "assistant",
                    content = 
                        "You are a helpful assistant that can turn a user's natural language question prompt into a duck DB query.\n" +
                        "Duck DB is mostly postgres SQL compatible; \n" +
                        $"for a file with the following schema: {Data.ToJson(source)} \n" +
                        $"construct the query with FROM public_cases_fc"
                },
                new() {
                    role = "user",
                    content = 
                        "Write a duck DB query to answer the following question: " +
                        question + 
                        " Format the query as a single line of SQL.\n" +
                        "respond with the query only; any other text will cause an error"
                }
            ];
        }
    }
}