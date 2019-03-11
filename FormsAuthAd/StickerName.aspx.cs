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
CREATE TABLE #Output (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int
)
CREATE TABLE #Temp (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int,
Meal varchar(50)
)
CREATE TABLE #Sub (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int,
Meal varchar(50)
)

INSERT INTO #Sub
SELECT No_,'',@date,SUM(Quantity)'Qty',''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
WHERE [Location Code]=@location AND [Shipment Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
GROUP BY No_

INSERT INTO #Output
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty
FROM #Sub x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Sub

DECLARE @item VARCHAR(50)
DECLARE @meal VARCHAR(50)
DECLARE @pdate Date
DECLARE @qty int
DECLARE @hand int

While (Select Count(*) From #Output) > 0
Begin
Select Top 1 @item = Item From #Output
Select Top 1 @pdate = Date From #Output
Select Top 1 @qty = Qty From #Output

INSERT INTO #Temp
SELECT x.[No_],'',@pdate,[Quantity per]*@qty,@item
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

INSERT INTO #Sub
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty,Meal
FROM #Temp x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Temp

While (Select Count(*) From #Sub) > 0
Begin
Select Top 1 @item = Item From #Sub
Select Top 1 @pdate = Date From #Sub
Select Top 1 @qty = Qty From #Sub
Select Top 1 @meal = Meal From #Sub

INSERT INTO #Output
SELECT x.[No_],'',@pdate,[Quantity per]*@qty
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END
CREATE TABLE #Final (
Item varchar(255),
Decpt varchar(255),
Allergen varchar(255)
)
CREATE TABLE #Final2 (
Item varchar(255),
Decpt varchar(255),
Allergen varchar(255)
)

INSERT INTO #Output
SELECT No_,'',@date,SUM(Quantity)'Qty'
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
WHERE LEN(No_)=4 AND [Location Code]=@location AND [Shipment Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
GROUP BY No_

INSERT INTO #Final
SELECT Item,Decpt,[Allergen Code]
FROM #Output
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Allergen] ON [Item No_]=Item COLLATE DATABASE_DEFAULT
WHERE " + ddlItem.Text + @"
GROUP BY Item,Decpt,[Allergen Code]

DECLARE @des VARCHAR(255)
DECLARE @allg VARCHAR(10)
DECLARE @prefix VARCHAR(255)
While (Select Count(*) From #Final) > 0
BEGIN
Select Top 1 @item = Item From #Final
Select Top 1 @des = Decpt From #Final
Select Top 1 @allg = Allergen From #Final
IF (Select Count(*) From #Final WHERE Item=@item AND (Allergen LIKE LEFT(@allg,2)+'%' OR Allergen IS NULL)) = 1
BEGIN
INSERT INTO #Final2 Values(@item,@des,(SELECT [Description] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] WHERE [Code]=@allg))
DELETE FROM #Final WHERE Item=@item AND (Allergen=@allg OR Allergen IS NULL)
END
ELSE
BEGIN
DELETE FROM #Final WHERE Item=@item AND Allergen=LEFT(@allg,2)
Select Top 1 @prefix = SUBSTRING([Description],0, CHARINDEX('(',[Description])) From #Final LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] ON Code=Allergen COLLATE DATABASE_DEFAULT WHERE Allergen LIKE LEFT(@allg,2)+'%'

INSERT INTO #Final2
SELECT @item,@des,RTRIM(@prefix)+' ('+STUFF((SELECT ', ' + CAST((SELECT SUBSTRING([Description],CHARINDEX('(',[Description])+1, CHARINDEX(')',[Description],CHARINDEX('(',[Description])+1)-CHARINDEX('(',[Description])-1)) AS VARCHAR(80)) AS [text()] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] LEFT JOIN #Final ON Allergen=Code COLLATE DATABASE_DEFAULT  WHERE Allergen LIKE LEFT(@allg,2)+'%' AND Item=@item FOR XML PATH('')), 1, 2, NULL)+')'
DELETE FROM #Final WHERE Item=@item AND Allergen LIKE LEFT(@allg,2)+'%'
END
END

SELECT Item,([Description]+[Description 2])'Decpt',[Sticker Name]'StickerName','Contains: ' + STUFF((SELECT ', ' + CAST(Allergen AS VARCHAR(80)) AS [text()] FROM #Final2 y WHERE y.Item=x.Item FOR XML PATH('')), 1, 2, NULL) 'Allergen'
FROM #Final2 x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] ON No_=Item COLLATE DATABASE_DEFAULT 
GROUP BY Item,Decpt,[Sticker Name],Description,[Description 2]
DROP TABLE #Sub
DROP TABLE #Output
DROP TABLE #Temp
DROP TABLE #Final
DROP TABLE #Final2";
            }
            else
            {
                lblSource.Text = "Source: Forecast";
                sql = @"DECLARE @date Date = '" + ddlCycle.Text + @"' 
                                DECLARE @location VARCHAR(5) = '" + ddlDC.Text + @"'
CREATE TABLE #Output (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int
)
CREATE TABLE #Temp (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int,
Meal varchar(50)
)
CREATE TABLE #Sub (
Item varchar(255),
Decpt varchar(255),
Date Date,
Qty int,
Meal varchar(50)
)
INSERT INTO #Sub
SELECT [Item No_],'',@date,SUM([Forecast Quantity]),''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location
GROUP BY [Item No_]

INSERT INTO #Output
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty
FROM #Sub x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Sub

DECLARE @item VARCHAR(50)
DECLARE @meal VARCHAR(50)
DECLARE @pdate Date
DECLARE @qty int
DECLARE @hand int

While (Select Count(*) From #Output) > 0
Begin
Select Top 1 @item = Item From #Output
Select Top 1 @pdate = Date From #Output
Select Top 1 @qty = Qty From #Output

INSERT INTO #Temp
SELECT x.[No_],'',@pdate,[Quantity per]*@qty,@item
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

INSERT INTO #Sub
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty,Meal
FROM #Temp x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Temp

While (Select Count(*) From #Sub) > 0
Begin
Select Top 1 @item = Item From #Sub
Select Top 1 @pdate = Date From #Sub
Select Top 1 @qty = Qty From #Sub
Select Top 1 @meal = Meal From #Sub

INSERT INTO #Output
SELECT x.[No_],'',@pdate,[Quantity per]*@qty
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END
CREATE TABLE #Final (
Item varchar(255),
Decpt varchar(255),
Allergen varchar(255)
)
CREATE TABLE #Final2 (
Item varchar(255),
Decpt varchar(255),
Allergen varchar(255)
)
INSERT INTO #Output
SELECT [Item No_],'',@date,SUM([Forecast Quantity])
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE LEN([Item No_])=4 AND [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location
GROUP BY [Item No_]

INSERT INTO #Final
SELECT Item,Decpt,[Allergen Code]
FROM #Output
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Allergen] ON [Item No_]=Item COLLATE DATABASE_DEFAULT
WHERE " + ddlItem.Text + @"
GROUP BY Item,Decpt,[Allergen Code]

DECLARE @des VARCHAR(255)
DECLARE @allg VARCHAR(10)
DECLARE @prefix VARCHAR(255)
While (Select Count(*) From #Final) > 0
BEGIN
Select Top 1 @item = Item From #Final
Select Top 1 @des = Decpt From #Final
Select Top 1 @allg = Allergen From #Final
IF (Select Count(*) From #Final WHERE Item=@item AND (Allergen LIKE LEFT(@allg,2)+'%' OR Allergen IS NULL)) = 1
BEGIN
INSERT INTO #Final2 Values(@item,@des,(SELECT [Description] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] WHERE [Code]=@allg))
DELETE FROM #Final WHERE Item=@item AND (Allergen=@allg OR Allergen IS NULL)
END
ELSE
BEGIN
DELETE FROM #Final WHERE Item=@item AND Allergen=LEFT(@allg,2)
Select Top 1 @prefix = SUBSTRING([Description],0, CHARINDEX('(',[Description])) From #Final LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] ON Code=Allergen COLLATE DATABASE_DEFAULT WHERE Allergen LIKE LEFT(@allg,2)+'%'
INSERT INTO #Final2
SELECT @item,@des,RTRIM(@prefix)+' ('+STUFF((SELECT ', ' + CAST((SELECT SUBSTRING([Description],CHARINDEX('(',[Description])+1, CHARINDEX(')',[Description],CHARINDEX('(',[Description])+1)-CHARINDEX('(',[Description])-1)) AS VARCHAR(80)) AS [text()] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Allergen] LEFT JOIN #Final ON Allergen=Code COLLATE DATABASE_DEFAULT  WHERE Allergen LIKE LEFT(@allg,2)+'%' AND Item=@item FOR XML PATH('')), 1, 2, NULL)+')'
DELETE FROM #Final WHERE Item=@item AND Allergen LIKE LEFT(@allg,2)+'%'
END
END

SELECT Item,([Description]+[Description 2])'Decpt',[Sticker Name]'StickerName','Contains: ' + STUFF((SELECT ', ' + CAST(Allergen AS VARCHAR(80)) AS [text()] FROM #Final2 y WHERE y.Item=x.Item FOR XML PATH('')), 1, 2, NULL) 'Allergen'
FROM #Final2 x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] ON No_=Item COLLATE DATABASE_DEFAULT 
GROUP BY Item,Decpt,[Sticker Name],[Description],[Description 2]
DROP TABLE #Sub
DROP TABLE #Output
DROP TABLE #Temp
DROP TABLE #Final
DROP TABLE #Final2";
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

            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GridView1.ClientID + "', 760, 1555, 32 ,false); </script>", false);
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