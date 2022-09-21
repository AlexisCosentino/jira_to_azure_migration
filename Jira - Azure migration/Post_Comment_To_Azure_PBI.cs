using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira___Azure_migration
{
    public class Post_Comment_To_Azure_PBI
    {
        public const string BASE = "https://dev.azure.com";
        string PAT;
        public const string ORG = "IRIUMSOFTWARE";
        public const string API = "api-version=6.0";
        public const string PROJECT = "TEST_ALEXIS";
        public string ID_of_PBI;

        public Post_Comment_To_Azure_PBI(string ID)
        {
            JObject data = JObject.Parse(File.ReadAllText("data.json"));
            this.PAT = (string?)data["azureToken"];
            this.ID_of_PBI = ID;
        }

        public void postCommentToAzurePBI(string jsonComment)
        {
             // REMINDER : When create the string content, dont use json-patch+json, and dont put bracket around the json to post

            HttpClient client = new HttpClient();

             // Set Media Type of Response.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

             // Generate base64 encoded authorization header.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT))));

            // Build the URI for creating Work Item.
            string uri = String.Join("?", String.Join("/", BASE, ORG, PROJECT, "_apis/wit/workItems", this.ID_of_PBI, "comments"), API + "-preview.3");

            Console.WriteLine($"le json : {jsonComment}, l'uri est {uri}");

            // Create Request body in JSON format.
            HttpContent content = new StringContent(jsonComment, Encoding.UTF8, "application/json");

            // Call CreateWIT method.
            string result = CreateWIT(client, uri, content).Result;

            // Pretty print the JSON if result not empty or null.
            if (!String.IsNullOrEmpty(result))
            {
                 dynamic data = JsonConvert.DeserializeObject<object>(result);
                 Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        public async Task<string> CreateWIT(HttpClient client, string uri, HttpContent content)
        {
            try
            {
                // Send asynchronous POST request.
                using (HttpResponseMessage response = await client.PostAsync(uri, content))
                {
                    response.EnsureSuccessStatusCode();
                    return (await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }
    }
}
