using System;
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
            var  date = parsedDate.ToString("YYYY-MM-DD HH:mm:ss", CultureInfo.InvariantCulture);

            // I need --> 2022-09-15T14:03:21.42Z

            Console.WriteLine($"la date qui sort est {created}, une fois parsé : {parsedDate}, et la très bizarre culture : {date}");
            Console.WriteLine($"la date qui sort est {created.GetType()}, une fois parsé : {parsedDate.GetType()}, et voici la petite dernière {date.GetType()}");


            string jsonToPost = "[{ \"op\": \"add\", \"path\": \"/fields/System.Title\", \"from\": null, \"value\": \"" + ticketData["summary"] + "\"}, { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] + "\"}]";
            return jsonToPost;
        }
    }
}
