using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace Jira___Azure_migration
{
    public class Post_Attachment_To_Azure
    {
        public const string BASE = "https://dev.azure.com";
        string PAT;
        string ID;
        public const string ORG = "IRIUMSOFTWARE";
        public const string API = "api-version=6.0";
        public const string PROJECT = "TEST_ALEXIS";


        public Post_Attachment_To_Azure(string ID)
        {
            JObject data = JObject.Parse(File.ReadAllText("data.json"));
            this.PAT = (string?)data["azureToken"];
            this.ID = ID;
        }


        public string PatchAttachmentToAzureServer(string linkToPost, string filename)
        {
            //var url = $"https://dev.azure.com/IRIUMSOFTWARE/TEST_ALEXIS/_apis/wit/attachments?fileName=logo.png&api-version=6.0";

            string url = String.Join("?", String.Join("/", BASE, ORG, PROJECT, "_apis/wit/attachments"),"fileName="+ filename + "&" + API);

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", get_credentials("azureToken")))));

            WebClient wc = new WebClient();

            wc.Headers.Add("Authorization", "Basic " + GetEncodedCredentials());

            byte[] byteData = wc.DownloadData(linkToPost);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                HttpResponseMessage response = client.PostAsync(url, content).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
                dynamic response_url = JsonConvert.DeserializeObject<object>(responseBody);
                Console.WriteLine(JsonConvert.SerializeObject(response_url, Formatting.Indented));
                return response_url["url"].ToString();
            }
        }

        private string GetEncodedCredentials()
        {
            string mergedCredentials = string.Format("{0}:{1}", get_credentials("jira_username"), get_credentials("jira_pwd"));
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }

        private string get_credentials(string key)
        {
            JObject data = JObject.Parse(File.ReadAllText("data.json"));
            return (string?)data[key];
        }

    }
}
