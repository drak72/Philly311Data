using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

namespace opendata.lib
{
    public class InvokeMessage
    {
        [JsonPropertyName("role")]
        public string role { get; set; } = "";

        [JsonPropertyName("content")]
        public string content { get; set; } = "";
    }

    /** Bedrock Class */
    public class Agent
    {

        public async Task<string> Converse(List<InvokeMessage> messages)
        {
            var modelId = "anthropic.claude-3-5-sonnet-20240620-v1:0";
            var response = await Conversation(modelId, messages);
            return response;
        }


        private static async Task<string> Conversation(string modelId, List<InvokeMessage> messages)
        {
            var client = new AmazonBedrockRuntimeClient(RegionEndpoint.USWest2);
            try {       
                var request = new InvokeModelRequest()
                {
                    ModelId = modelId,
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
                        anthropic_version = "bedrock-2023-05-31",
                        max_tokens = 4096,
                        messages
                    })))
                };

                
                var res = await client.InvokeModelAsync(request);
                var modelResponse = await JsonNode.ParseAsync(res.Body);
                var responseText = modelResponse["content"][0]["text"].ToString();


                if (string.IsNullOrEmpty(responseText))
                {
                    throw new Exception("Invalid Model response");
                }

                Console.WriteLine("Bedrock Response: " + responseText);
                return responseText;
            }
            catch (Exception e)
            {
                Console.WriteLine("Bedrock Error: " + e);
                throw;
            }
        }
    }

}
