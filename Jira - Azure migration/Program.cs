// See https://aka.ms/new-console-template for more information
using System.Configuration;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System;
using Jira___Azure_migration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

var migration = new Migration();
migration.launchMigration();

