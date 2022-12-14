using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
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
        public List<string> attachment_url { get; set; }= new List<string>();
        public Dictionary<string, string> comment_dict { get; set; }



        public Translate_Jira_To_Azure(Dictionary<string, string> dict)
        {
            ticketData = dict;
        }


        public string createJsonWithPBIToPost()
        {
            var parsedDate = DateTime.Parse(ticketData["created"]);
            if (parsedDate.Date == DateTime.Today)
            {
                // WARNING = if date of creation is less than 2hours, an error gonna occurS, thats why i substract 2h in case of ticket from today.
                parsedDate = parsedDate.AddHours(-2);
            }
            var createdDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToString("s") + ".000Z";

            string dueDate = translateDateTimeToAzure(ticketData["dueDate"]);
            string startDate = translateDateTimeToAzure(ticketData["startDate"]);
            string endDate = translateDateTimeToAzure(ticketData["endDate"]);
            string worklog = getWorkLog(ticketData["workLog"], ticketData["totalWorkTime"]);

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
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Microsoft.VSTS.Scheduling.DueDate\", \"value\": \"" + dueDate + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Microsoft.VSTS.Scheduling.StartDate\", \"value\": \"" + startDate + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Custom.Enddate\", \"value\": \"" + endDate + "\" }";
            jsonToPost += ", {\"op\": \"add\", \"path\": \"/fields/Custom.WorkLog\", \"value\": \"" + worklog + "\" }";

            jsonToPost += "]";
            Console.WriteLine(jsonToPost);
            return jsonToPost;
        }

        public string createJsonWithCommentToPost()
        {
            comment = cleanJson(comment_dict["comment"]);
            foreach (var a in this.attachment_url)
            {
                comment = editHTMLTagsDependsAttachment(a, comment);
            }
            comment = String.Join("<br><br>", $"<h2><strong>Ecrit par {comment_dict["author"]}</strong></h2> <h4>Le {comment_dict["created_date"]}</h4>", comment);
            string jsonToPost = "{ \"text\": \"" + comment + "\"}";
            return jsonToPost;
        }

        public string createJsonToPatchPBIWithAttachment()
        {
            string json = "[{\"op\": \"add\", \"path\": \"/relations/-\", \"value\": { \"rel\": \"AttachedFile\", \"url\": \""+ attachment +"\", \"attributes\": {\"comment\": \"Spec for the work\"}}}";
            json += "]";
            this.attachment_url.Add(attachment);
            return json;
        }

        public string getDescriptionJson()
        {
            if (attachment_url.Count > 0)
            {
                foreach (var a in this.attachment_url)
                {
                    ticketData["description"] = editHTMLTagsDependsAttachment(a, ticketData["description"]);

                }
                string json = "[{ \"op\": \"add\", \"path\": \"/fields/System.Description\", \"from\": null, \"value\": \"" + ticketData["description"] + "\"}]";

                return json;
            }
            else
            {
              return "false";
            }
        }

        public string cleanJson(string toformat)
        {
            toformat = toformat.Replace("{code:java}", "<code>");
            toformat = toformat.Replace("{code:java}", "<code>");
            
            toformat = toformat.Replace("{code}", "</code>");
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

        public string editHTMLTagsDependsAttachment(string a, string desc)
        {
            string formated;
            string file = a.Split("fileName=").Last();
            if (file.Split('.').Last() == "png" || file.Split('.').Last() == "jpeg" || file.Split('.').Last() == "jpg" || file.Split('.').Last() == "gif")
            {
                formated = desc.Replace(file, $"<img alt='img_url' src='{a}' >"); ;
            }
            else
            {
                formated = desc.Replace(file, $"<a href='{a}' target='_blank'>{file}</a>");
            }
            return formated;
        }

        public string translateDateTimeToAzure(string date)
        {
            if (!String.IsNullOrEmpty(date))
            {
                date = DateTime.SpecifyKind(DateTime.Parse(date), DateTimeKind.Utc).ToString("s") + ".000Z";
            }
            return date;
        }
        
        public string getWorkLog(string wl, string total)
        {
            string workLogString = "";
            if (!String.IsNullOrEmpty(wl))
            {
                foreach (string log in wl.Split(';'))
                {
                    workLogString += $"{log} <br> ";
                }
                workLogString += $"Soit un total de <strong>{total} heures </strong>";
            }
            return workLogString;
        }
    }
}