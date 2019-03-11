using System;
using System.Text;
using System.Collections;
using System.DirectoryServices;
using System.Data.SqlClient;
using System.Configuration;

namespace PrepTracker
{
    public class LdapAuthentication
    {
        private String _path;
        private String _filterAttribute;

        public LdapAuthentication(String path)
        {
            _path = path;
        }

        public string IsAuthenticated(String domain, String username, String pwd)
        {
            String user = "";
            int length = username.Length;
            if (length > 20)
            {
                user = username.Substring(0, 20);
            }
            else
            {
                user = username;
            }
            String domainAndUsername = domain + @"\" + user;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            try
            {//Bind to the native AdsObject to force authentication.
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + user + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return "false";
                }

                //Update the new path to the user in the directory.
                _path = result.Path;
                _filterAttribute = (String)result.Properties["cn"][0];

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
                conn.Open();
                SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM Permission WHERE Account ='" + username + "' AND AppName='PrepTracker';", conn);
                Int32 count = Convert.ToInt32(comm.ExecuteScalar());
                conn.Close(); //close the connection
                if (count == 0)
                {
                    return "HR";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error authenticating user. " + ex.Message);
            }

            DateTime today = DateTime.Now;
            SqlConnection conndate = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            conndate.Open();
            SqlCommand commdate = new SqlCommand("UPDATE Permission SET LastLogin='" + today + "' WHERE Account = '" + username + "' AND AppName='PrepTracker';", conndate);
            commdate.ExecuteScalar();
            conndate.Close(); //close the connection
            return "true";
        }

        public String GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(_path);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();

            try
            {
                SearchResult result = search.FindOne();

                int propertyCount = result.Properties["memberOf"].Count;

                String dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (String)result.Properties["memberOf"][propertyCounter];

                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }

                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
        }
    }
}