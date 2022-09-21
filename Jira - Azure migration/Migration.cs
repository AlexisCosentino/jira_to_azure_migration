using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jira___Azure_migration;

namespace Jira___Azure_migration
{
    public class Migration
    {
        DB_Connection connection;
        Post_PBI_To_Azure Post_PBI_To_Azure;
        Post_Comment_To_Azure_PBI Post_Comment_To_Azure_PBI;
        Translate_Jira_To_Azure Translate_Jira_To_Azure;
        public Migration()
        {
        }

        public void launchMigration()
        {
            connection = new DB_Connection();
            connection.query = "SELECT TOP 1 * FROM jiraissue, project WHERE project.id=jiraissue.project and project.pname not like '%Hotline%' ORDER BY CREATED DESC;\r\n";
            var dict_of_pbi = connection.getDictOfPBI();

            foreach (var dict in dict_of_pbi.Values)
            {
                Translate_Jira_To_Azure = new Translate_Jira_To_Azure(dict);
                var json = Translate_Jira_To_Azure.createJsonWithPBIToPost();
                Post_PBI_To_Azure = new Post_PBI_To_Azure();
                Post_PBI_To_Azure.PostPBIToAzure(json);
                string PBI_ID = Post_PBI_To_Azure.ID_of_PBI;
                connection.query = $"SELECT jiraaction.actionbody FROM jiraissue, project, jiraaction WHERE jiraaction.issueid = jiraissue.id  and project.id = jiraissue.project and issuenum = {dict["issueNb"]} and project = {dict["project"]} ORDER BY jiraissue.CREATED ASC;";
                var list_of_comments = connection.getListOfComments();
                foreach (var comment in list_of_comments)
                {
                    Translate_Jira_To_Azure.comment = comment;
                    var comment_json_to_post = Translate_Jira_To_Azure.createJsonWithCommentToPost();
                    Post_Comment_To_Azure_PBI = new Post_Comment_To_Azure_PBI(PBI_ID);
                    Post_Comment_To_Azure_PBI.postCommentToAzurePBI(comment_json_to_post);
                }
            }
        }
    }
}
