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
        public Dictionary<string, string> comment_dict { get; set; }



        public Translate_Jira_To_Azure(Dictionary<string, string> dict)
        {
            ticketData = dict;
        }


        public string createJsonWithPBIToPost()
        {
            string created = ticketData["created"];
            var parsedDate = DateTime.Parse(created);
            if (parsedDate.Date == DateTime.Today)
            {
                // WARNING = if date of creation is less than 2hours, an error gonna occurS, thats why i substract 2h in case of ticket from today.
                parsedDate = parsedDate.AddHours(-2);
            }
            var createdDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToString("s") + ".000Z";

            foreach (var item in ticketData.Keys)
            {
                ticketData[item] = cleanJson(ticketData[item]);
            }

            var issueStatus = translateStatusToAzure(ticketData["issueStatus"]);
    

            string jsonToPost = "[{ \"op\": \"add\", \"path\": \"/fields/System.Title\", \"from\": null, \"value\": \"" + ticketData["summary"] + "\"}";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] +"\"} ";
            jsonToPost += ", { \"op\": \"add\", \"path\": \"/fields/System.State\", \"from\": null, \"value\": \""+ issueStatus +"\"}";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.CreatedBy\", \"value\": \"" + ticketData["creator"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.AssignedTo\", \"value\": \"" + ticketData["assignee"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.CreatedDate\", \"value\": \""+ createdDate +"\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.TeamProject\", \"value\": \"TEST_ALEXIS\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.AreaPath\", \"value\": \"TEST_ALEXIS\\\\Dev Team\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/System.Tags\", \"value\": \""+ ticketData["ListOfLabels"] +" \" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Custom.Type\", \"value\": \"" + ticketData["issueType"] + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Custom.PriorityField\", \"value\": \"" + ticketData["priority"] + "\" }";
            jsonToPost += "]";
            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string createJsonWithCommentToPost()
        {
            comment = cleanJson(comment_dict["comment"]);
            comment = String.Join("<br><br>", $"<h2><strong>Ecrit par {comment_dict["author"]}</strong></h2> <h4>Le {comment_dict["created_date"]}</h4>", comment);
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
            return toformat;
        }

        public string translateStatusToAzure(string status)
        {
            switch(status)
            {
                case "Acceptée":
                    status = "Approved";
                    return status;
                case "A Compléter":
                    status = "To Complete";
                    return status;
                case "Attente test":
                    status = "Test";
                    return status;
                case "Cloturée":
                    status = "Done";
                    return status;
                case "Demande":
                    status = "New";
                    return status;
                case "EN ATTENTE":
                    status = "Pending";
                    return status;
                case "En cours":
                    status = "Developement in Progress";
                    return status;
                case "Rejetée":
                    status = "Denied";
                    return status;
                case "Terminée":
                    status = "Commited";
                    return status;
                case "Test KO":
                    status = "Test Ko";
                    return status;
                case "A tester":
                    status = "Test";
                    return status;
                case "A Valider":
                    status = "New";
                    return status;
                default:
                    status = "New";
                    return status;
            }
        }
    }
}