using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Jira___Azure_migration;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;

namespace Jira___Azure_migration
{
    public class Migration
    {
        DB_Connection connection;
        Post_PBI_To_Azure Post_PBI_To_Azure;
        Post_Comment_To_Azure_PBI Post_Comment_To_Azure_PBI;
        Translate_Jira_To_Azure Translate_Jira_To_Azure;
        Post_Attachment_To_Azure Post_Attachment_To_Azure;
        Patch_PBI_To_Azure Patch_PBI_To_Azure;
        public Migration()
        {
        }

        public void launchMigration()
        {
            Console.WriteLine("Press :\r\n 1 -> Select issue n°199770 ( comments, attachment, pretty description ) \r\n 2 -> Last issue from jira \r\n 3 -> 10 last issues from jira \r\n ");
            string choice = Console.ReadLine();

            //GET EXECUTION TIME !!
            Stopwatch stwatch = new Stopwatch();
            stwatch.Start();

            connection = new DB_Connection();
            switch (choice)
            {
                case "1":
                    connection.query = get_query("issue_199770");
                    break;
                case "2": default:
                    connection.query = get_query("last_issue");
                    break;
                case "3":
                    connection.query = get_query("last_10_issues");
                    break;
            }
            var dict_of_pbi = connection.getDictOfPBI();

            using (var progress = new ProgressBar())
            {
                var total = dict_of_pbi.Count;
                var i = 1;
                foreach (var dict in dict_of_pbi.Values)
                {
                    progress.Report((double)i / total);
                    i++;

                    
                    connection.query = "select component.cname from component, jiraissue inner join nodeassociation on nodeassociation.ASSOCIATION_TYPE = 'IssueComponent' where nodeassociation.sink_node_id = component.id and jiraissue.id = nodeassociation.SOURCE_NODE_ID and jiraissue.id = " + dict["issueNb"] + ";";
                    List<string> components = connection.getListOfComponents();

                    connection.query = "select projectversion.vname from projectversion, jiraissue inner join nodeassociation on nodeassociation.ASSOCIATION_TYPE = 'IssueFixVersion' where nodeassociation.sink_node_id = projectversion.id and jiraissue.id = nodeassociation.SOURCE_NODE_ID and jiraissue.id = "+ dict["issueNb"] +";";
                    List<string> fixedVersion = connection.getListOfFixedVersion();

                    connection.query = "select label.label from label where label.issue = " + dict["issueNb"] + ";";
                    List<string> labels = connection.getListOfLabels();

                    string labelsString = dict["ProjectName"] + "; ";

                    foreach (var label in labels)
                    {
                        labelsString += label + "; ";
                    }
                    foreach (var fv in fixedVersion)
                    {
                        labelsString += "fixedV : " + fv + "; ";
                    }
                    foreach(var c in components)
                    {
                        labelsString += "component :" + c + "; ";
                    }

                    Console.WriteLine(labelsString);
                    dict["ListOfLabels"] = labelsString;


                    Translate_Jira_To_Azure = new Translate_Jira_To_Azure(dict);
                    var json = Translate_Jira_To_Azure.createJsonWithPBIToPost();
                    Post_PBI_To_Azure = new Post_PBI_To_Azure();
                    Post_PBI_To_Azure.PostPBIToAzure(json);
                    string PBI_ID = Post_PBI_To_Azure.ID_of_PBI;

                    //Get every Attachments related of PBI
                    connection.query = $"SELECT id ,mimetype, filename FROM fileattachment Where issueid = {dict["issueNb"]};";
                    var list_of_attachments = connection.getListOfAttachments();
                    
                    foreach (var attachment in list_of_attachments)
                    {
                        // first we need post jira link to azure server, dont forget use webclient to connect to jira account because data is secured
                        // return azure link and use it to make json
                        // then use this json to patch on azure PBI
                        Post_Attachment_To_Azure = new Post_Attachment_To_Azure(PBI_ID);
                        var filename = attachment.Split('/').Last();
                        var azure_link = Post_Attachment_To_Azure.PatchAttachmentToAzureServer(attachment, filename);
                        Translate_Jira_To_Azure.attachment = azure_link;
                        var attachment_json_to_post = Translate_Jira_To_Azure.createJsonToPatchPBIWithAttachment();
                        Patch_PBI_To_Azure = new Patch_PBI_To_Azure(PBI_ID);
                        Patch_PBI_To_Azure.patchPBIToAzure(attachment_json_to_post);
                    }
                    Console.WriteLine("IM OUT OF ATTACHMENT FOR LOOP");
                    var descriptionJson = Translate_Jira_To_Azure.getDescriptionJson();
                    if (descriptionJson != "false")
                    {
                        Patch_PBI_To_Azure.patchPBIToAzure(descriptionJson);
                    }




                    //Get every comments related of PBI
                    connection.query = $"SELECT jiraaction.issueid, jiraaction.author, jiraaction.actionbody, jiraaction.CREATED,  jiraaction.id FROM jiraissue, project, jiraaction WHERE jiraaction.issueid = jiraissue.id  and project.id = jiraissue.project and issueid = {dict["issueNb"]} and project = {dict["project"]} ORDER BY jiraissue.CREATED DESC;";
                    var dict_of_comments = connection.getDictOfComments();
                    foreach (var comment in dict_of_comments.Values)
                    {
                        Translate_Jira_To_Azure.comment_dict = comment;
                        var comment_json_to_post = Translate_Jira_To_Azure.createJsonWithCommentToPost();
                        Post_Comment_To_Azure_PBI = new Post_Comment_To_Azure_PBI(PBI_ID);
                        Post_Comment_To_Azure_PBI.postCommentToAzurePBI(comment_json_to_post);
                    }
                }
            }
            stwatch.Stop();
            var exec_time = String.Format("{0:00}:{1:00}.{2:00}", stwatch.Elapsed.Minutes, stwatch.Elapsed.Seconds,
            stwatch.Elapsed.Milliseconds / 10);
            Console.WriteLine($"Execution time is {exec_time}");
        }

        private string get_query(string query_name)
        {
            JObject data = JObject.Parse(File.ReadAllText("queries.json"));
            return (string?)data[query_name];

        }
    }
}