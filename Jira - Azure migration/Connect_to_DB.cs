using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;

namespace Jira___Azure_migration
{
    public class Connect_to_DB
    {
        string username;
        string dbname;
        string hostname;
        string password;
        string query;
        Dictionary<string, Dictionary<string, string>> queryAnswerDict = new Dictionary<string, Dictionary<string, string>>() ;



        public Connect_to_DB(string pwd, string db_name, string db_hostname, string db_username, string query)
        {
            this.password = pwd;
            this.query = query;
            this.username = db_username;
            this.dbname = db_name;
            this.hostname = db_hostname;
        }

        public Dictionary<string, Dictionary<string, string>> start_connection()
        {
            SqlConnection conn = new SqlConnection($"Server={hostname};Database={dbname};User Id={username};Password={password};");
            try
            {
                SqlCommand command = new SqlCommand(this.query, conn);

                conn.Open();
                Console.WriteLine("We are in bro");

                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        var value_dict = new Dictionary<string, string>();
                        var ID = reader[0].ToString();
                        value_dict.Add("issueNb", reader[2].ToString());
                        value_dict.Add("project", reader[3].ToString());
                        value_dict.Add("assignee", reader[5].ToString());
                        value_dict.Add("creator", reader[6].ToString());
                        value_dict.Add("issueType", reader[7].ToString());
                        value_dict.Add("summary", reader[8].ToString());
                        value_dict.Add("description", reader[9].ToString());
                        value_dict.Add("created", reader[14].ToString());
                        value_dict.Add("updated", reader[15].ToString());
                        value_dict.Add("dueDate", reader[16].ToString());
                        value_dict.Add("ProjectName", reader[31].ToString());
                        this.queryAnswerDict.Add(ID, value_dict);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
                    var new_dict = toJson(queryAnswerDict);
                    Console.WriteLine(new_dict.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                conn.Close();
            }
            return queryAnswerDict;
        }

        public string toJson(IDictionary dict)
        {
            return JsonConvert.SerializeObject(dict);
        }
    }
}
