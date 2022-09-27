using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Jira___Azure_migration;
using static System.Net.Mime.MediaTypeNames;

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
            connection = new DB_Connection();
            connection.query = "SELECT TOP 1 * FROM jiraissue, project WHERE project.id=jiraissue.project and issuetype != 10800 and not (project.id = 10000 or project.id= 13301) ORDER BY CREATED DESC;\r\n";
            connection.query = "SELECT * FROM jiraissue, project WHERE project.id = jiraissue.project and issuenum= 148 and project = 15000 and project.pname not like '%Hotline%' ORDER BY jiraissue.CREATED DESC;\r\n";
            var dict_of_pbi = connection.getDictOfPBI();

            foreach (var dict in dict_of_pbi.Values)
            {
                Translate_Jira_To_Azure = new Translate_Jira_To_Azure(dict);
                var json = Translate_Jira_To_Azure.createJsonWithPBIToPost();
                Post_PBI_To_Azure = new Post_PBI_To_Azure();
                Post_PBI_To_Azure.PostPBIToAzure(json);
                string PBI_ID = Post_PBI_To_Azure.ID_of_PBI;

                //Get every Attachments related of PBI
                connection.query = $"SELECT id ,mimetype, filename FROM fileattachment Where issueid = {dict["issueID"]};";
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
                    var Patch_PBI_To_Azure = new Patch_PBI_To_Azure(PBI_ID);
                    Patch_PBI_To_Azure.patchPBIToAzure(attachment_json_to_post);
                }

                //Get every comments related of PBI
                connection.query = $"SELECT jiraaction.actionbody FROM jiraissue, project, jiraaction WHERE jiraaction.issueid = jiraissue.id  and project.id = jiraissue.project and issuenum = {dict["issueNb"]} and project = {dict["project"]} ORDER BY jiraissue.CREATED DESC;";
                var list_of_comments = connection.getListOfComments();
                foreach (var comment in list_of_comments)
                {
                    Translate_Jira_To_Azure.comment = comment;
                    var comment_json_to_post = Translate_Jira_To_Azure.createJsonWithCommentToPost();
                    Post_Comment_To_Azure_PBI = new Post_Comment_To_Azure_PBI(PBI_ID);
                    Post_Comment_To_Azure_PBI.postCommentToAzurePBI(comment_json_to_post);
                }


                //THIS IS THE CODE TO UPDATE YOUR PBI IN CASE YOU NEED :)
                /*             
                Patch_PBI_To_Azure = new Patch_PBI_To_Azure(PBI_ID);
                string jsonToPatch = Translate_Jira_To_Azure.createJsonToPatchPBI();
                Patch_PBI_To_Azure.patchPBIToAzure(jsonToPatch);
               */

                // Comment the next line if you want export mass data in once
                Console.WriteLine("press ENTER to keep going");
                Console.ReadLine();
            }
        }
    }
}