using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira___Azure_migration
{

    public class Translate_Jira_To_Azure
    {
        Dictionary<string, string> ticketData;
        public string comment { get; set; }


        public Translate_Jira_To_Azure(Dictionary<string, string> dict)
        {
            ticketData = dict;
        }

        public Translate_Jira_To_Azure(string comment)
        {
            this.comment = comment;
        }


        public string createJsonWithPBIToPost()
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
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] +"\"} ";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.State\", \"from\": null, \"value\": \"Development in Progress\"}";
            jsonToPost += "]";

            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string createJsonWithCommentToPost()
        {
            comment = formatText(comment);
            string jsonToPost = "{ \"text\": \"" + comment + "\"}";
            return jsonToPost;
        }

        public string formatText(string toformat)
        {
            toformat = toformat.Replace("\"", " "); // Remove every double quote of the text
            toformat = toformat.Replace("\\", "");  // Remove every backslash of the text
            toformat = toformat.Replace("\r\n", "<br>"); //Transate line breaker
            return toformat;
        }
    }
}
