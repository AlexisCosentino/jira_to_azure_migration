using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira___Azure_migration
{
    public class DB_Connection  
    {
        string username;
        string dbname;
        string hostname;
        string password;
        public string query { get; set; }


        public Dictionary<string, Dictionary<string, string>> getDictOfPBI()
        {
            get_credentials();
            var queryAnswerDict = new Dictionary<string, Dictionary<string, string>>();
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
                        value_dict.Add("issueTypeID", reader[7].ToString());
                        value_dict.Add("summary", reader[8].ToString());
                        value_dict.Add("description", reader[9].ToString());
                        value_dict.Add("created", reader[14].ToString());
                        value_dict.Add("updated", reader[15].ToString());
                        value_dict.Add("dueDate", reader[16].ToString());
                        value_dict.Add("ProjectName", reader[31].ToString());
                        value_dict.Add("issueType", reader[43].ToString());
                        value_dict.Add("priority", reader[56].ToString());
                        value_dict.Add("issueStatus", reader[50].ToString());
                        value_dict.Add("issueID", reader[0].ToString());
                        queryAnswerDict.Add(ID, value_dict);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
                    Console.WriteLine(JsonConvert.SerializeObject(queryAnswerDict).ToString());
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

        public Dictionary<string, Dictionary<string, string>> getDictOfComments()
        {
            get_credentials();
            var queryAnswerDict = new Dictionary<string, Dictionary<string, string>>();
            SqlConnection conn = new SqlConnection($"Server={hostname};Database={dbname};User Id={username};Password={password};");
            try
            {
                SqlCommand command = new SqlCommand(this.query, conn);

                conn.Open();
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        var value_dict = new Dictionary<string, string>();
                        var ID = reader[4].ToString();
                        value_dict.Add("issueID", reader[0].ToString());
                        value_dict.Add("author", reader[1].ToString());
                        value_dict.Add("comment", reader[2].ToString());
                        value_dict.Add("created_date", reader[3].ToString());
                        queryAnswerDict.Add(ID, value_dict);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                conn.Close();
                Console.WriteLine(JsonConvert.SerializeObject(queryAnswerDict).ToString());

            }
            return queryAnswerDict;
        }

        public List<string> getListOfComments()
        {
            get_credentials();
            List<string> queryAnswerList = new List<string>();
            SqlConnection conn = new SqlConnection($"Server={hostname};Database={dbname};User Id={username};Password={password};");
            try
            {
                SqlCommand command = new SqlCommand(this.query, conn);

                conn.Open();
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        queryAnswerList.Add(reader[2].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                conn.Close();
                Console.WriteLine(JsonConvert.SerializeObject(queryAnswerList).ToString());

            }
            return queryAnswerList;
        }

        public List<string> getListOfAttachments()
        {
            get_credentials();
            List<string> queryAnswerList = new List<string>();
            SqlConnection conn = new SqlConnection($"Server={hostname};Database={dbname};User Id={username};Password={password};");
            try
            {
                SqlCommand command = new SqlCommand(this.query, conn);

                conn.Open();
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        var attachment_link = $"https://worklog.vega-systems.com/secure/attachment/{reader[0]}/{reader[2]}";
                        Console.WriteLine(attachment_link);
                        queryAnswerList.Add(attachment_link);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    reader.Close();
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
            return queryAnswerList;
        }

        private void get_credentials()
        {
            JObject data = JObject.Parse(File.ReadAllText("data.json"));
            this.password = (string?)data["db_pwd"];
            this.username = (string?)data["db_username"];
            this.dbname = (string?)data["db_name"];
            this.hostname = (string?)data["db_hostname"];
        }
    }
}
