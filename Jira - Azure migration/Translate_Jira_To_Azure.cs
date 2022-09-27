using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Jira___Azure_migration
{

    public class Translate_Jira_To_Azure
    {
        Dictionary<string, string> ticketData;
        public string comment { get; set; }
        public string attachment { get; set; }



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
            if (parsedDate.Date == DateTime.Today)
            {
                // WARNING = if date of creation is less than 2hours, an error gonna occur, thats why i substract 2h in case of ticket from today.
                parsedDate = parsedDate.AddHours(-2);
            }
            var createdDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToString("s") + ".000Z";

            foreach (var item in ticketData.Keys)
            {
                ticketData[item] = cleanJson(ticketData[item]);
            }
    

            string jsonToPost = "[{ \"op\": \"add\", \"path\": \"/fields/System.Title\", \"from\": null, \"value\": \"" + ticketData["summary"] + "\"}";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] +"\"} ";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.State\", \"from\": null, \"value\": \"Development in Progress\"}";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.CreatedBy\", \"value\": \"" + ticketData["creator"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.AssignedTo\", \"value\": \"" + ticketData["assignee"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.CreatedDate\", \"value\": \""+ createdDate +"\" }";
            //jsonToPost += ", {\"op\": \"add\", \"path\": \"/relations/-\", \"value\": { \"rel\": \"AttachedFile\", \"url\": \"https://dev.azure.com/IRIUMSOFTWARE/94d3079f-fdfd-48f4-b420-2a41ec9be70d/_apis/wit/attachments/1507a21d-ba08-4cee-88bc-0c36bc380097?fileName=Classe%20UML%20migration%20jira%20to%20azure.png&download=true&api-version=5.0-preview.2\", \"attributes\": {\"comment\": \"Spec for the work\"}}}";
            //jsonToPost += ", {\"op\": \"add\", \"path\": \"/relations/-\", \"value\": { \"rel\": \"AttachedFile\", \"url\": \"https://dev.azure.com/IRIUMSOFTWARE/94d3079f-fdfd-48f4-b420-2a41ec9be70d/_apis/wit/attachments/0868b30c-b65d-4c30-8112-c84fac7826d6?fileName=portrait2.png&download=true&api-version=5.0-preview.2\", \"attributes\": {\"comment\": \"Spec for the work\"}}}";
            jsonToPost += "]";

            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string createJsonWithCommentToPost()
        {
            comment = cleanJson(comment);
            string jsonToPost = "{ \"text\": \"" + comment + "\"}";
            return jsonToPost;
        }


        public string createJsonToPatchPBIWithAttachment()
        {
            string json = "[{\"op\": \"add\", \"path\": \"/relations/-\", \"value\": { \"rel\": \"AttachedFile\", \"url\": \""+ attachment +"\", \"attributes\": {\"comment\": \"Spec for the work\"}}}";
            json += "]";
            return json;
        }

        public string cleanJson(string toformat)
        {
            toformat = toformat.Replace("\r\n *****", "<br>&emsp;&emsp;&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n ****", "<br>&emsp;&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n ***", "<br>&emsp;&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n **", "<br>&emsp;&emsp;\t■");
            toformat = toformat.Replace("\r\n *", "<br>&emsp;\t■");
            toformat = toformat.Replace("\r\n", "<br>"); //Transate line breaker
            toformat = toformat.Replace("\"", " "); // Remove every double quote of the text
            toformat = toformat.Replace("\\", "");  // Remove every backslash of the text
            toformat = toformat.Replace("*[", "<strong>[");
            toformat = toformat.Replace("]*", "]</strong>");

            // h2. = H2 Title
            return toformat;
        }
    }
}




