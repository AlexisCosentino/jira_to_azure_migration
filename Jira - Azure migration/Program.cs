// See https://aka.ms/new-console-template for more information
using System.Configuration;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System;
using Jira___Azure_migration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


var pwd = get_credentials("db_pwd");
var db_username = get_credentials("db_username");
var db_hostname = get_credentials("db_hostname");
var db_name = get_credentials("db_name");

//Get every jira issues, from every project, except from hotline projects.
string query = "SELECT TOP 1 * FROM jiraissue, project WHERE project.id=jiraissue.project and project.pname not like '%Hotline%' ORDER BY CREATED DESC;\r\n";

try
{
    var startConnect = new Connect_to_DB(pwd, db_name, db_hostname, db_username, query);
    var answerQuery = startConnect.start_connection();
    var token = get_credentials("azureToken");
    var createPBI = new Post_To_Azure(token);



    Console.WriteLine("now we will send datas from query, to azure, press ENTER if you want to keep going");
    Console.ReadLine();



    foreach (var dict in answerQuery.Values)
    {
        var translation = new Translate_Jira_To_Azure(dict);
        string jsonToPost = translation.createJsonWillPostToAzure();
        createPBI.post_to_azure(jsonToPost);
    }

}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}


static string get_credentials(string key)
{
    JObject data = JObject.Parse(File.ReadAllText("data.json"));
    string value = (string?)data[key];
    return value;
}






