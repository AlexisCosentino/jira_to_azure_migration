﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jira___Azure_migration
{
    public class Patch_To_Azure
    {
        public const string BASE = "https://dev.azure.com";
        string PAT;
        string ID;
        public const string ORG = "IRIUMSOFTWARE";
        public const string API = "bypassRules=true&api-version=6.0";
        public const string PROJECT = "TEST_ALEXIS";

        public Patch_To_Azure(string token, string ID)
        {
            this.PAT = token;
            this.ID = ID;
        }

        public void patch_to_azure(string jsonToPost)
        {
            HttpClient client = new HttpClient();

            // Set Media Type of Response.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Generate base64 encoded authorization header.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT))));

            // Build the URI for creating Work Item.
            string uri = String.Join("?", String.Join("/", BASE, ORG, PROJECT, "_apis/wit/workitems", ID), API);

            Console.WriteLine(jsonToPost);
            Console.WriteLine(ID);
            Console.WriteLine(uri);

            // Create Request body in JSON format.
            HttpContent content = new StringContent(jsonToPost, Encoding.UTF8, "application/json-patch+json");

            // Call CreateWIT method.
            string result = UpdateWIT(client, uri, content).Result;

            // Pretty print the JSON if result not empty or null.
            if (!String.IsNullOrEmpty(result))
            {
                dynamic wit = JsonConvert.DeserializeObject<object>(result);
                Console.WriteLine(JsonConvert.SerializeObject(wit, Formatting.Indented));
            }

            // Presss any key to exit
            // TAKE OFF THIS LINE IF YOU WANT YOU WANT TRANSFER MASS DATA IN ONCE
            Console.ReadLine();
            /////////////////////////////////////////////////////////////////////
            client.Dispose();
        }


        public async Task<string> UpdateWIT(HttpClient client, string uri, HttpContent content)
        {
            try
            {
                // Send asynchronous POST request.
                using (HttpResponseMessage response = await client.PatchAsync(uri, content))
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
        } // End of CreateWIT method
    }
}
