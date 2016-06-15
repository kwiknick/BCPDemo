using System;
using System.Configuration;
using System.Data.SqlClient;


namespace BCPDemo
{
    public static class BCPDataHandler
    {

        public static void DataLoader(string outputfilename, string fullyQualifiedTableName, string serverName)
        {
            var tableHasData = CheckTableForData(fullyQualifiedTableName);
            if (tableHasData) return;
            var cmdExe = fullyQualifiedTableName + " in " + outputfilename + " -c -T -S " + serverName;
            var p = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    WorkingDirectory = @"C:\\TEMP\\BCPFiles",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    FileName = "BCP",
                    Arguments = cmdExe
                }
            };

            try
            {
                p.Start();
                p.StandardOutput.ReadToEnd();

                var myStreamReader = p.StandardError;
                Console.WriteLine(myStreamReader.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
            p.WaitForExit();
        }

        public static void FileCreator(string query, string outputFileName, string serverName)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "BCP",
                    WorkingDirectory = @"C:\\TEMP\\BCPFiles",
                    Arguments = @"" + query + " queryout " + outputFileName + " -S " + serverName + " -c -T"
                }
            };
            process.Start();
        }

        private static bool CheckTableForData(string fullyQualifiedTableName)
        {
            var hasData = false;
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["AdventureWorksConnectionString"].ConnectionString))
            {
                con.Open();

                using (var command = new SqlCommand("SELECT COUNT(1) FROM " + fullyQualifiedTableName, con))
                {
                    //command.Parameters.Add(new SqlParameter("TableName", tableName));

                    var reader = command.ExecuteScalar();

                    var returnValue = (int)command.ExecuteScalar();
                    if (returnValue > 0)
                    {
                        //var rows1 = reader.HasRows;
                        hasData = true;
                    }
                    else if (returnValue == 0)
                    {
                        //var rows0 = reader.HasRows;
                        hasData = false;
                    }
                    else
                    {
                        throw new Exception("Neither True nor False was returned from the query.");
                    }
                }
            }
            return hasData;
        }

        public static string GetServerName()
        {
            var connectString = ConfigurationManager.ConnectionStrings["AdventureWorksConnectionString"].ToString();
            var builder = new SqlConnectionStringBuilder(connectString);
            var serverName = builder.DataSource;
            return serverName;
        }
    }
}