using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira___Azure_migration
{
    public class Translate_Jira_To_Azure
    {
        Dictionary<string, string> ticketData;

        public Translate_Jira_To_Azure(Dictionary<string, string> dict)
        {
            ticketData = dict;
        }

        public string createJsonWillPostToAzure()
        {
            string created = ticketData["created"];
            var parsedDate = DateTime.Parse(created);
            var createdDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToString("s") + ".000Z";
            Console.WriteLine(createdDate);
            // I need --> 2022-09-15T14:03:21.42Z


            foreach (var item in ticketData.Keys)
            {
                ticketData[item] = formatText(ticketData[item]);
            }


            string jsonToPost = "[{ \"op\": \"add\", \"path\": \"/fields/System.Title\", \"from\": null, \"value\": \"" + ticketData["summary"] + "\"}";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] + "\"} ";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.State\", \"from\": null, \"value\": \"Development in Progress\"}";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.CreatedDate\", \"from\": null, \"value\": \"" + createdDate + "\"}";
            jsonToPost += "]";

            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string formatText(string toformat)
        {
            toformat = toformat.Replace("\"", " ");
            return toformat;
        }


    }
}
