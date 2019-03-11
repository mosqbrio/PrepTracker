using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices.AccountManagement;
using System.Web.Services;

namespace PrepTracker
{
    public partial class User : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            conn.Open();
            SqlCommand AppList_cmd = new SqlCommand("SELECT * FROM AppLink", conn);
            SqlDataAdapter app_da = new SqlDataAdapter(AppList_cmd);
            DataTable app_dt = new DataTable();
            app_da.Fill(app_dt);
            for (int i = 0; i < app_dt.Rows.Count; i++)
            {
                HyperLink hyperlink = new HyperLink();
                hyperlink.Text = app_dt.Rows[i]["App"].ToString();
                hyperlink.NavigateUrl = app_dt.Rows[i]["Link"].ToString(); ;
                Panel2.Controls.Add(hyperlink);
            }
            if (!IsPostBack)
            {
                
                SqlCommand permiss = new SqlCommand("SELECT COUNT(*) FROM Permission WHERE Account2000 = '" + Context.User.Identity.Name + "' AND AppName='PrepTracker';", conn);
                int permiss_count = Convert.ToInt32(permiss.ExecuteScalar());
                if (permiss_count == 0)
                {
                    conn.Close(); //close the connection
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "redirect", "alert('Sorry! You do not have access to this application.'); window.location='../';", true);
                }
                else
                {
                    
                    SqlCommand comm = new SqlCommand("SELECT Permission FROM Permission WHERE Account2000 = '" + Context.User.Identity.Name + "' AND AppName='PrepTracker';", conn);
                    String Permission = Convert.ToString(comm.ExecuteScalar());
                    SqlCommand DC_comm = new SqlCommand("SELECT (CASE WHEN x.OfficeID=0 then 'ALL' else y.Office end)'Office' FROM Permission x LEFT JOIN DC y ON y.OfficeID=x.OfficeID WHERE Account2000 = '" + Context.User.Identity.Name + "' AND AppName='PrepTracker';", conn);
                    String DC = Convert.ToString(DC_comm.ExecuteScalar());

                    Session["DC"] = DC;
                    if (Permission == "Basic" || Permission == "View")
                    {
                        Response.Redirect("Default.aspx");
                    }
                    Session["sortExpression"] = "Permission";
                    Session["direction"] = " ASC";

                    if (DC == "ALL")
                    {
                        SqlCommand cmd = new SqlCommand("SELECT Office, OfficeID FROM DC;", conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dddlDC.DataValueField = "OfficeID";
                        dddlDC.DataTextField = "Office";
                        dddlDC.DataSource = dt;
                        dddlDC.DataBind();
                        ddlDC.DataValueField = "OfficeID";
                        ddlDC.DataTextField = "Office";
                        ddlDC.DataSource = dt;
                        ddlDC.DataBind();
                        ddlDC.Items.Insert(0, new ListItem("ALL", "0"));
                    }
                    else
                    {
                        SqlCommand DCID_comm = new SqlCommand("SELECT OfficeID FROM DC WHERE Office='" + DC + "';", conn);
                        String DCID = Convert.ToString(DCID_comm.ExecuteScalar());
                        dddlDC.Items.Add(new ListItem(DC, DCID));
                        ddlDC.Items.Add(new ListItem(DC, DCID));
                    }
                    Get_Table();
                }

            }
            conn.Close(); //close the connection
        }
        protected void IndexChanged(object sender, EventArgs e)
        {
            GridView1.EditIndex = -1;
            Get_Table();
        }

        protected void Get_Table()
        {
            String DC = dddlDC.SelectedValue.ToString();
            
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT x.Account,x.Account2000,x.Permission,x.OfficeID,(CASE WHEN x.OfficeID=0 then 'ALL' else y.Office end)'Office',x.LastLogin FROM Permission x LEFT JOIN DC y ON y.OfficeID=x.OfficeID WHERE (x.OfficeID=" + DC + " OR x.OfficeID=0) AND AppName='PrepTracker' ORDER BY Permission;", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DataView dv = new DataView(dt);
            dv.Sort = Convert.ToString(Session["sortExpression"]) + Convert.ToString(Session["direction"]);
            //Session["datasource"] = dt;
            //Session["datasourcesort"] = dv;
            GridView1.DataSource = dv;
            GridView1.DataBind();
            con.Close();
        }
        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            Get_Table();
            //GridView1.DataSource = Session["datasourcesort"];
            GridView1.EditIndex = e.NewEditIndex;
            GridView1.DataBind();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            String DC = Session["DC"].ToString();
            GridViewRow row = GridView1.Rows[e.RowIndex];
            String ddleditPermission = ((DropDownList)row.Cells[2].FindControl("ddleditPermission")).Text;
            String ddleditOffice = ((DropDownList)row.Cells[3].FindControl("ddleditOffice")).SelectedValue;
            String lblAccount = ((Label)row.Cells[1].FindControl("lblAccount")).Text;
            Boolean Pass = true;
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            conn.Open();

            //SqlCommand account_comm = new SqlCommand("Select Count(*) FROM Permission WHERE Account = '" + lblAccount + "';", conn);
            //int account = Convert.ToInt32(account_comm.ExecuteScalar());
            //if (account > 1)
            //{
            //    SqlCommand per_cmd = new SqlCommand("Select Permission, Office, Team FROM Permission WHERE Account = '" + lblAccount + "';", conn);
            //    SqlDataAdapter pcmd = new SqlDataAdapter(per_cmd);
            //    DataTable pdt = new DataTable();
            //    pcmd.Fill(pdt);
            //    if (pdt.Rows[0]["Permission"].ToString() == "Lead" && ddleditPermission == "Lead" && pdt.Rows[0]["Office"].ToString() == ddleditOffice)
            //    {
            //        for (int i = 0; i < pdt.Rows.Count; i++)
            //        {
            //            if (pdt.Rows[i]["Team"].ToString() == ddleditTeam)
            //            {
            //                Response.Write("<script>alert('This Permission already exists.');</script>");
            //                Pass = false;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Response.Write("<script>alert('This user has more than two lead permissions');</script>");
            //        Pass = false;
            //    }
            //}
            
            SqlCommand comme = new SqlCommand("UPDATE Permission SET Permission='" + ddleditPermission + "', OfficeID=" + ddleditOffice + " WHERE Account='" + lblAccount + "' AND AppName='PrepTracker';", conn);
            comme.ExecuteNonQuery();
            
            //SqlCommand Log_comme = new SqlCommand("INSERT INTO Logs (Date,Type,Modification,ModifiedBy) VALUES ('" + DateTime.Now + "', 'UPDATE USER',  'Updated user [" + lblAccount + "] to Permission=" + ddleditPermission + ", Office=" + ddleditOffice + ", Team=" + ddleditTeam + "', '" + Context.User.Identity.Name + "')", conn);
            //Log_comme.ExecuteNonQuery();
            GridView1.EditIndex = -1;
            GridView1.DataBind();
            Get_Table();
            //}
            conn.Close();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridViewRow row = GridView1.Rows[e.RowIndex];
            String lblAccount = ((Label)row.Cells[1].FindControl("lblAccount")).Text;
            String lblPermission = ((Label)row.Cells[2].FindControl("lblPermission")).Text;
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            conn.Open();
            
            SqlCommand comme = new SqlCommand("DELETE FROM Permission WHERE Account='" + lblAccount + "' AND AppName='PrepTracker';", conn);
            comme.ExecuteNonQuery();
                //SqlCommand Log_comme = new SqlCommand("INSERT INTO Logs (Date,Type,Modification,ModifiedBy) VALUES ('" + DateTime.Now + "', 'REMOVE USER',  'Removed user [" + lblAccount + "]', '" + Context.User.Identity.Name + "')", conn);
                //Log_comme.ExecuteNonQuery();
            
            conn.Close();
            GridView1.EditIndex = -1;
            Get_Table();
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                String DC = Session["DC"].ToString();
                String lblPermission = ((Label)e.Row.Cells[2].FindControl("lblPermission")).Text;
                //check if is in edit mode
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    DropDownList ddleditOffice = (DropDownList)e.Row.FindControl("ddleditOffice");
                    String lblOffice = ((Label)e.Row.Cells[3].FindControl("lblOffice")).Text;
                    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
                    conn.Open();
                    if (DC == "ALL")
                    {
                        SqlCommand cmd = new SqlCommand("SELECT Office, OfficeID FROM DC;", conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddleditOffice.DataValueField = "OfficeID";
                        ddleditOffice.DataTextField = "Office";
                        ddleditOffice.DataSource = dt;
                        ddleditOffice.DataBind();
                        ddleditOffice.Items.Insert(0, new ListItem("ALL", "0"));
                        ddleditOffice.Text = lblOffice;
                    }
                    else
                    {
                        SqlCommand DCID_comm = new SqlCommand("SELECT OfficeID FROM DC WHERE Office='" + DC + "';", conn);
                        String DCID = Convert.ToString(DCID_comm.ExecuteScalar());
                        ddleditOffice.Items.Add(new ListItem(DC, DCID));
                    }
                    conn.Close();
                }
                
                if (lblPermission == "Admin" && DC != "ALL")
                {
                    LinkButton Edit = (LinkButton)e.Row.FindControl("LinkButton1");
                    Edit.Visible = false;
                    LinkButton Delete = (LinkButton)e.Row.FindControl("LinkButton3");
                    Delete.Visible = false;
                }
                if (dddlDC.SelectedItem.ToString() == "WC")
                {
                    String LastLogin = e.Row.Cells[4].Text;
                    if (LastLogin != "&nbsp;")
                    {
                        DateTime last = Convert.ToDateTime(LastLogin);
                        e.Row.Cells[4].Text = last.AddHours(-2).ToString();
                    }
                }
                else if (dddlDC.SelectedItem.ToString() == "EC")
                {
                    String LastLogin = e.Row.Cells[4].Text;
                    if (LastLogin != "&nbsp;")
                    {
                        DateTime last = Convert.ToDateTime(LastLogin);
                        e.Row.Cells[4].Text = last.AddHours(+1).ToString();
                    }
                }
            }

        }
        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Get_Table();
            //GridView1.DataSource = Session["datasourcesort"];
            //GridView1.DataBind();
            //EditButton();
        }

        private const string ASCENDING = " ASC";
        private const string DESCENDING = " DESC";

        public SortDirection GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDirection.Ascending;

                return (SortDirection)ViewState["sortDirection"];
            }
            set { ViewState["sortDirection"] = value; }
        }

        protected void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridView1.EditIndex = -1;
            GridView1.DataBind();
            string sortExpression = e.SortExpression;

            if (GridViewSortDirection == SortDirection.Ascending)
            {
                GridViewSortDirection = SortDirection.Descending;
                SortGridView(sortExpression, DESCENDING);
            }
            else
            {
                GridViewSortDirection = SortDirection.Ascending;
                SortGridView(sortExpression, ASCENDING);
            }

        }

        private void SortGridView(string sortExpression, string direction)
        {
            Session["sortExpression"] = sortExpression;
            Session["direction"] = direction;
            Get_Table();
            //DataTable dt = Session["datasource"] as DataTable;
            //DataView dv = new DataView(dt);
            //dv.Sort = sortExpression + direction;
            //Session["sortExpression"] = sortExpression;
            //Session["direction"] = direction;
            //Session["datasourcesort"] = dv;
            //GridView1.DataSource = dv;
            //GridView1.DataBind();
            
        }
        protected void Add_Click(Object sender, EventArgs e)
        {
            Boolean Pass = true;
            msgoffice.Text = "";
            msgpermission.Text = "";
            msguser.Text = "";

            if (username.Text == "")
            {
                msguser.Text = "*";
                Pass = false;
                mp1.Show();
            }
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
                conn.Open();
            if (Pass == true)
            {
                SqlCommand account_comm = new SqlCommand("Select Count(*) FROM Permission WHERE Account = '" + username.Text + "' AND AppName='PrepTracker';", conn);
                int account = Convert.ToInt32(account_comm.ExecuteScalar());

                if (account > 0)
                {
                        Response.Write("<script>alert('This username already exists.');</script>");
                        Pass = false;
                        mp1.Show();
                }
            }

            if (Pass == true)
            {
                String un = username.Text.ToLower();

                String user2000 = "";
                int length = un.Length;
                if (length > 20)
                {
                    user2000 = un.Substring(0, 20);
                }
                else
                {
                    user2000 = un;
                }

                SqlCommand comm = new SqlCommand("INSERT INTO Permission (AppName,Account,Account2000,Permission,OfficeID) VALUES ('PrepTracker','" + un + "', '" + user2000 + "', '" + ddlPermission.SelectedItem + "', '" + ddlDC.SelectedValue + "')", conn);
                comm.ExecuteNonQuery();
                //SqlCommand Log_comme = new SqlCommand("INSERT INTO Logs (Date,Type,Modification,ModifiedBy) VALUES ('" + DateTime.Now + "', 'ADD USER',  'Added user [" + un + "] to Permission=" + ddlPermission.SelectedItem + ", Office=" + ddlDC.SelectedItem + ", Team=" + Team + "', '" + Context.User.Identity.Name + "')", conn);
                //Log_comme.ExecuteNonQuery();
                // //close the connection
                //Response.Redirect(Request.RawUrl);
                Get_Table();
                username.Text = "";
            }
            conn.Close();
        }
        //TextBox Auto Complete
        [WebMethod]
        public static List<string> GetItems(string item)
        {
            List<string> itemsResult = new List<string>();

            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "SUNBASKET");

            // define a "query-by-example" principal - here, we search for a UserPrincipal 
            UserPrincipal qbeUser = new UserPrincipal(ctx);
            qbeUser.DisplayName = "*" + item + "*";

            // create your principal searcher passing in the QBE principal    
            PrincipalSearcher srch = new PrincipalSearcher(qbeUser);

            // find all matches
            int i = 0;
            foreach (var found in srch.FindAll())
            {
                // do whatever here - "found" is of type "Principal"
                UserPrincipal userFound = found as UserPrincipal;
                if (userFound != null)
                {
                    // do something with your user principal here....
                    itemsResult.Add(userFound.DisplayName + " - " + userFound.SamAccountName);
                }
                i++;
                if (i > 9)
                {
                    return itemsResult;
                }
            }
            return itemsResult;
        }
    }
}