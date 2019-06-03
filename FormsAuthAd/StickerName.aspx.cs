using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PrepTracker
{
    public partial class StickerName : System.Web.UI.Page
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
                Panel1.Controls.Add(hyperlink);
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
                        Manage_Users.Visible = false;
                    }
                    Session["sortExpression"] = "Permission";
                    Session["direction"] = " ASC";

                    if (DC == "ALL")
                    {
                        SqlCommand cmd = new SqlCommand("SELECT Office FROM DC WHERE DateDeleted IS NULL", conn);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ddlDC.DataTextField = "Office";
                        ddlDC.DataValueField = "Office";
                        ddlDC.DataSource = dt;
                        ddlDC.DataBind();
                    }
                    else
                    {
                        ddlDC.Items.Add(DC);
                    }
                    int dayofweek = Convert.ToInt32(DateTime.Today.DayOfWeek);
                    DateTime Cycle = DateTime.Today;
                    switch (dayofweek)
                    {
                        case 1:
                            Cycle = DateTime.Today;
                            break;
                        case 2:
                            Cycle = DateTime.Today.AddDays(-1);
                            break;
                        case 3:
                            Cycle = DateTime.Today.AddDays(5);
                            break;
                        case 4:
                            Cycle = DateTime.Today.AddDays(4);
                            break;
                        case 5:
                            Cycle = DateTime.Today.AddDays(3);
                            break;
                        case 6:
                            Cycle = DateTime.Today.AddDays(2);
                            break;
                        case 0:
                            Cycle = DateTime.Today.AddDays(1);
                            break;
                    }
                    ddlCycle.Items.Add(Cycle.AddDays(-7).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(7).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(14).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(21).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(28).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(35).ToString("M/d/yyyy"));
                    ddlCycle.Items.Add(Cycle.AddDays(42).ToString("M/d/yyyy"));
                    ddlCycle.Text = Cycle.ToString("M/d/yyyy");
                    GetData();
                }

            }
            conn.Close();
        }
        DataTable gdt = new DataTable();
        protected void GetData()
        {
            string sql = "";
            if (DateTime.Today >= Convert.ToDateTime(ddlCycle.Text).AddDays(-4))
            {
                lblSource.Text = "Source: Web Order";
                sql = @"DECLARE @date Date = '" + ddlCycle.Text + @"' 
                                DECLARE @location VARCHAR(5) = '" + ddlDC.Text + @"'
DECLARE @ORDER TABLE(
Item varchar(25),
Date Date,
Exp bigint,
Level int
)
DECLARE @STKU TABLE(
Item varchar(25),
Date Date,
Exp bigint
--,Code varchar(50)
)
DECLARE @Final TABLE (
Item varchar(25),
Exp varchar(10),
Allergen varchar(255),
Processed bigint
)

INSERT INTO @ORDER
SELECT No_,[Planned Shipment Date],(CASE WHEN No_ LIKE 'K%' OR No_ LIKE 'Z%' THEN 1 ELSE 0 END),0
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
WHERE [Location Code]=@location AND [Planned Shipment Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
GROUP BY No_,[Planned Shipment Date]

--EXPLORE THE BOM
DECLARE @level int = 0
While (Select Count(*) From @ORDER WHERE Level=@level) > 0
Begin
	DELETE FROM @STKU
	INSERT INTO @STKU
	SELECT (CASE WHEN (y.[Production BOM No_]='' OR y.[Production BOM No_] IS NULL) THEN x.Item ELSE y.[Production BOM No_] END),Date,Exp--,z.[Item Category Code] 
	FROM @ORDER x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
	--LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] z ON z.No_=x.Item
	WHERE Level=@level	
	GROUP BY x.Item,y.[Production BOM No_],Date,Exp--,z.[Item Category Code]
	INSERT INTO @ORDER
	SELECT x.[No_],z.Date,z.Exp,(@level+1)
	FROM @STKU z
	INNER JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x ON x.[Production BOM No_]=Item 
	WHERE (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '8%') 
	AND ((x.[Starting Date]<=z.Date AND x.[Ending Date]>=z.Date) OR (x.[Starting Date]<=z.Date AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=z.Date))
	GROUP BY x.No_,[Quantity per],z.Date,z.Exp
	SET @level=@level+1
END

DELETE FROM @ORDER WHERE Item LIKE '8%' OR " + ddlItem.Text + @"

;WITH ITEMEXP AS(SELECT Item,MAX(Exp)'EXP' FROM @ORDER GROUP BY Item)
INSERT INTO @Final
SELECT Item,(CASE WHEN EXP=1 AND Item LIKE '3%' THEN CONVERT(VARCHAR(10), DATEADD(DAY,8,@date), 101) ELSE '' END),[Allergen Code],0
FROM ITEMEXP
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Allergen] ON [Item No_]=Item

DECLARE @item VARCHAR(25)
DECLARE @expdate VARCHAR(10)
DECLARE @allg VARCHAR(10)
DECLARE @prefix VARCHAR(50)
While (Select Count(*) From @Final WHERE Processed=0) > 0
BEGIN
	Select Top 1 @item = Item From @Final WHERE Processed=0
	Select Top 1 @expdate = Exp From @Final WHERE Processed=0
	Select Top 1 @allg = Allergen From @Final WHERE Processed=0
	IF (Select Count(*) From @Final WHERE Item=@item AND Processed=0 AND (Allergen LIKE LEFT(@allg,2)+'%' OR Allergen IS NULL)) = 1
	BEGIN
		UPDATE @Final SET Processed=1, Allergen=(SELECT [Description] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] WHERE [Code]=@allg)
		WHERE Item=@item AND (Allergen=@allg OR Allergen IS NULL)
	END
	ELSE
	BEGIN
		DELETE FROM @Final WHERE Item=@item AND Allergen=LEFT(@allg,2)
		Select Top 1 @prefix = SUBSTRING([Description],0, CHARINDEX('(',[Description])) From @Final LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] ON Code=Allergen COLLATE DATABASE_DEFAULT WHERE Allergen LIKE LEFT(@allg,2)+'%'
		INSERT INTO @Final
		SELECT @item,@expdate,RTRIM(@prefix)+' ('+STUFF((SELECT ', ' + CAST((SELECT SUBSTRING([Description],CHARINDEX('(',[Description])+1, CHARINDEX(')',[Description],CHARINDEX('(',[Description])+1)-CHARINDEX('(',[Description])-1)) AS VARCHAR(80)) AS [text()] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] LEFT JOIN @Final ON Allergen=Code COLLATE DATABASE_DEFAULT  WHERE Allergen LIKE LEFT(@allg,2)+'%' AND Item=@item FOR XML PATH('')), 1, 2, NULL)+')',1
		DELETE FROM @Final WHERE Item=@item AND Allergen LIKE LEFT(@allg,2)+'%' AND Processed=0
	END
END

SELECT Item,([Description]+[Description 2])'Decpt',[Sticker Name]'StickerName','Contains: ' + STUFF((SELECT ', ' + CAST(Allergen AS VARCHAR(80)) AS [text()] FROM @Final y WHERE y.Item=x.Item FOR XML PATH('')), 1, 2, NULL)'Allergen',Exp
FROM @Final x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] ON No_=Item COLLATE DATABASE_DEFAULT 
GROUP BY Item,[Sticker Name],[Description],[Description 2],Exp";
            }
            else
            {
                lblSource.Text = "Source: Forecast";
                sql = @"DECLARE @date Date = '" + ddlCycle.Text + @"' 
                                DECLARE @location VARCHAR(5) = '" + ddlDC.Text + @"'
DECLARE @ORDER TABLE(
Item varchar(25),
Date Date,
Exp bigint,
Level int
)
DECLARE @STKU TABLE(
Item varchar(25),
Date Date,
Exp bigint
--,Code varchar(50)
)
DECLARE @Final TABLE (
Item varchar(25),
Exp varchar(10),
Allergen varchar(255),
Processed bigint
)

INSERT INTO @ORDER
SELECT [Item No_],[Forecast Date],(CASE WHEN [Item No_] LIKE 'K%' OR [Item No_] LIKE 'Z%' THEN 1 ELSE 0 END),0
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location

--EXPLORE THE BOM
DECLARE @level int = 0
While (Select Count(*) From @ORDER WHERE Level=@level) > 0
Begin
	DELETE FROM @STKU
	INSERT INTO @STKU
	SELECT (CASE WHEN (y.[Production BOM No_]='' OR y.[Production BOM No_] IS NULL) THEN x.Item ELSE y.[Production BOM No_] END),Date,Exp--,z.[Item Category Code] 
	FROM @ORDER x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
	--LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] z ON z.No_=x.Item
	WHERE Level=@level	
	GROUP BY x.Item,y.[Production BOM No_],Date,Exp--,z.[Item Category Code]
	INSERT INTO @ORDER
	SELECT x.[No_],z.Date,z.Exp,(@level+1)
	FROM @STKU z
	INNER JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x ON x.[Production BOM No_]=Item 
	WHERE (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '8%') 
	AND ((x.[Starting Date]<=z.Date AND x.[Ending Date]>=z.Date) OR (x.[Starting Date]<=z.Date AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=z.Date))
	GROUP BY x.No_,[Quantity per],z.Date,z.Exp
	SET @level=@level+1
END

DELETE FROM @ORDER WHERE Item LIKE '8%' OR " + ddlItem.Text + @"

;WITH ITEMEXP AS(SELECT Item,MAX(Exp)'EXP' FROM @ORDER GROUP BY Item)
INSERT INTO @Final
SELECT Item,(CASE WHEN EXP=1 AND Item LIKE '3%' THEN CONVERT(VARCHAR(10), DATEADD(DAY,8,@date), 101) ELSE '' END),[Allergen Code],0
FROM ITEMEXP
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Allergen] ON [Item No_]=Item

DECLARE @item VARCHAR(25)
DECLARE @expdate VARCHAR(10)
DECLARE @allg VARCHAR(10)
DECLARE @prefix VARCHAR(50)
While (Select Count(*) From @Final WHERE Processed=0) > 0
BEGIN
	Select Top 1 @item = Item From @Final WHERE Processed=0
	Select Top 1 @expdate = Exp From @Final WHERE Processed=0
	Select Top 1 @allg = Allergen From @Final WHERE Processed=0
	IF (Select Count(*) From @Final WHERE Item=@item AND Processed=0 AND (Allergen LIKE LEFT(@allg,2)+'%' OR Allergen IS NULL)) = 1
	BEGIN
		UPDATE @Final SET Processed=1, Allergen=(SELECT [Description] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] WHERE [Code]=@allg)
		WHERE Item=@item AND (Allergen=@allg OR Allergen IS NULL)
	END
	ELSE
	BEGIN
		DELETE FROM @Final WHERE Item=@item AND Allergen=LEFT(@allg,2)
		Select Top 1 @prefix = SUBSTRING([Description],0, CHARINDEX('(',[Description])) From @Final LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] ON Code=Allergen COLLATE DATABASE_DEFAULT WHERE Allergen LIKE LEFT(@allg,2)+'%'
		INSERT INTO @Final
		SELECT @item,@expdate,RTRIM(@prefix)+' ('+STUFF((SELECT ', ' + CAST((SELECT SUBSTRING([Description],CHARINDEX('(',[Description])+1, CHARINDEX(')',[Description],CHARINDEX('(',[Description])+1)-CHARINDEX('(',[Description])-1)) AS VARCHAR(80)) AS [text()] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] LEFT JOIN @Final ON Allergen=Code COLLATE DATABASE_DEFAULT  WHERE Allergen LIKE LEFT(@allg,2)+'%' AND Item=@item FOR XML PATH('')), 1, 2, NULL)+')',1
		DELETE FROM @Final WHERE Item=@item AND Allergen LIKE LEFT(@allg,2)+'%' AND Processed=0
	END
END

SELECT Item,([Description]+[Description 2])'Decpt',[Sticker Name]'StickerName','Contains: ' + STUFF((SELECT ', ' + CAST(Allergen AS VARCHAR(80)) AS [text()] FROM @Final y WHERE y.Item=x.Item FOR XML PATH('')), 1, 2, NULL)'Allergen',Exp
FROM @Final x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] ON No_=Item COLLATE DATABASE_DEFAULT 
GROUP BY Item,[Sticker Name],[Description],[Description 2],Exp";
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString2"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString1"].ConnectionString);
            conn.Open();
            SqlCommand cmmd = new SqlCommand("SELECT Meal, Sticker FROM [Operation].[dbo].[MealStickerName]", conn);
            SqlDataAdapter gda = new SqlDataAdapter(cmmd);
            gda.Fill(gdt);
            conn.Close();

            GridView1.DataSource = dt;
            GridView1.DataBind();
            con.Close();

            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GridView1.ClientID + "', 760, 1705, 32 ,false); </script>", false);
        }

        protected void ddlCycle_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }
        protected void ddlDC_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }
        protected void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if(ddlItem.Text == "LEN(Item) = 4")
                {
                    string lblNo = ((Label)e.Row.FindControl("lblItem")).Text;
                    DataView dv = new DataView(gdt);
                    dv.RowFilter = "Meal ='" + lblNo + "'";
                    if(dv.Count == 1)
                    {
                        Label stickername = (Label)e.Row.FindControl("lblSticker");
                        stickername.Text = dv[0]["Sticker"].ToString();
                    }
                }
            }
        }
    }
}