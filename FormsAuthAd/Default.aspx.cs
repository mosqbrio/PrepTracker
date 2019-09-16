using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using DayPilot.Web.Ui;
using System.Collections;
using System.Data.Services.Client;
using System.Net;
using System.Drawing;

namespace PrepTracker
{
    public partial class Default : System.Web.UI.Page
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
                    DateTime today = DateTime.Now;
                    SqlCommand commdate = new SqlCommand("UPDATE Permission SET LastLogin='" + today + "' WHERE Account2000 = '" + Context.User.Identity.Name + "' AND AppName='PrepTracker';", conn);
                    commdate.ExecuteScalar();
                    Session["Permission"] = Permission;
                    Session["DC"] = DC;
                    if (Permission == "Basic")
                    {
                        Manage_Users.Visible = false;
                    }
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
        protected void GetData()
        {
            try
            {
                string type = ddlItem.SelectedItem.ToString();
                string fulfill = "";
                if (cbFulfill.Checked == true && type == "Tray")
                {
                    fulfill = "WHERE (z.Total-(CASE WHEN v.Qty IS NULL THEN 0 ELSE v.Qty END)-x.Total)<0 AND y.[Description] LIKE 'Tray %'";
                }
                else if (cbFulfill.Checked != true && type == "Tray")
                {
                    fulfill = "WHERE y.[Description] LIKE 'Tray %'";
                }
                else if (cbFulfill.Checked == true)
                {
                    fulfill = "WHERE (z.Total-(CASE WHEN v.Qty IS NULL THEN 0 ELSE v.Qty END)-x.Total)<0 AND y.[Description] NOT LIKE 'Tray %'";
                }
                else
                {
                    fulfill = "WHERE y.[Description] NOT LIKE 'Tray %'";
                }

                string sql = "";
                if (DateTime.Today >= Convert.ToDateTime(ddlCycle.Text).AddDays(-4))
                {
                    lblSource.Text = "Source: Web Order";
                    sql = @"DECLARE @date Date = '" + ddlCycle.Text + @"' 
                                DECLARE @location VARCHAR(5) = '" + ddlDC.Text + @"'
                                DECLARE @ORDER TABLE(
Item varchar(25),
Parent varchar(25),
Qty_Per Decimal(22,6),
Date Date,
Qty Decimal(22,6),
Level int,
Processed bigint
)
DECLARE @OLD_ORDER TABLE(
Item varchar(25),
Qty Decimal(22,6)
)
DECLARE @STKU TABLE(
Item varchar(25),
Parent varchar(25),
Date Date,
Qty Decimal(22,6),
OnHand Decimal(22,6),
Code varchar(50)
)
DECLARE @INVENTORY TABLE(
Item varchar(25),
Qty Decimal(22,6),
Total Decimal(22,6)
)
DECLARE @max INT
DECLARE @item varchar(25)
DECLARE @qty Decimal(22,6)
DECLARE @in_qty Decimal
DECLARE @min_date Date
DECLARE @max_date Date

INSERT INTO @ORDER
SELECT No_,'',0,[Planned Shipment Date],SUM(Quantity),0,0
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
WHERE [Location Code]=@location AND [Planned Shipment Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
GROUP BY No_,[Planned Shipment Date]

    INSERT INTO @ORDER
	SELECT [No_],'',0,DATEADD(DAY, -4, @date),SUM(Quantity),0,0
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
	WHERE [Planned Shipment Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
	GROUP BY [No_]

--EXPLORE THE BOM
DECLARE @level int = 0
While (Select Count(*) From @ORDER WHERE Level=@level) > 0
Begin
	DELETE FROM @STKU
	INSERT INTO @STKU
	SELECT (CASE WHEN (y.[Production BOM No_]='' OR y.[Production BOM No_] IS NULL) THEN x.Item ELSE y.[Production BOM No_] END),x.Item,Date,Qty,0,z.[Item Category Code] FROM @ORDER x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] z ON z.No_=x.Item
	WHERE Level=@level	
	GROUP BY x.Item,y.[Production BOM No_],Date,Qty,z.[Item Category Code]
	INSERT INTO @ORDER
	SELECT x.[No_],z.Parent,[Quantity per],z.Date,SUM([Quantity per]*Qty),(@level+1),0
	FROM @STKU z
	INNER JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x ON x.[Production BOM No_]=Item 
	WHERE ((x.No_ LIKE '1%' AND Code!='TRAY' AND x.[Production BOM No_] NOT LIKE '2%' AND x.[Production BOM No_] NOT LIKE '3%' AND x.[Production BOM No_] NOT LIKE '4%') OR (x.No_ NOT LIKE '1%' AND x.No_ LIKE '%')) 
	AND ((x.[Starting Date]<=z.Date AND x.[Ending Date]>=z.Date) OR (x.[Starting Date]<=z.Date AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=z.Date))
	GROUP BY x.No_,z.Parent,[Quantity per],z.Date
	SET @level=@level+1
END
DELETE FROM @ORDER WHERE Level=0 OR Item LIKE '8%'

--GET INVENTORY
;WITH ITEMS AS (SELECT Item FROM @ORDER GROUP BY Item)
INSERT INTO @INVENTORY
SELECT Item,SUM((CASE WHEN Quantity IS NULL THEN 0 ELSE Quantity END)),SUM((CASE WHEN Quantity IS NULL THEN 0 ELSE Quantity END))
FROM ITEMS x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
GROUP BY Item

--APPLY INVERTORY
SELECT @max = MAX(Level) FROM @ORDER
SELECT @min_date = MIN(Date) FROM @ORDER
SELECT @max_date = MAX(Date) FROM @ORDER
--LOOP DATE
WHILE @min_date<=@max_date
BEGIN
	SET @level=1
	--LOOP LEVEL
	WHILE @level<=@max
	BEGIN
		WHILE (SELECT COUNT(*) FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%')>0
		BEGIN
			SELECT TOP 1 @item = Item FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%'
			SELECT TOP 1 @qty = Qty FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%'
			SELECT @in_qty = Qty FROM @INVENTORY WHERE Item=@item
			if @qty<=@in_qty
			Begin
				UPDATE @INVENTORY SET Qty= @in_qty-@qty WHERE Item=@item
				UPDATE @ORDER SET Qty=0 WHERE Date=@min_date AND Level=(@level+1) AND Parent=@item
			End
			Else
			Begin
				UPDATE @ORDER SET Qty=(Qty-(@in_qty*Qty_Per)) WHERE Date=@min_date AND Level=(@level+1) AND Parent=@item
				UPDATE @INVENTORY SET Qty= 0 WHERE Item=@item
			End
			UPDATE @ORDER SET Processed=1 WHERE Date=@min_date AND Level=@level AND Item=@item
		END
		SET @level= @level+1
	END
	SET @min_date= DATEADD(DAY,1,@min_date)
END

INSERT INTO @OLD_ORDER SELECT Item,SUM(Qty) FROM @ORDER WHERE Date=DATEADD(DAY, -4, @date) GROUP BY Item
DELETE FROM @ORDER WHERE Date=DATEADD(DAY, -4, @date) OR Item NOT LIKE '" + ddlItem.Text + @"'

;WITH WeekQty AS(SELECT Item,SUM(CASE WHEN Date=DATEADD(DAY, -3, @date) THEN Qty ELSE 0 END) 'Fri'
,SUM(CASE WHEN Date=DATEADD(DAY, -2, @date) THEN Qty ELSE 0 END) 'Sat'
,SUM(CASE WHEN Date=DATEADD(DAY, -1, @date) THEN Qty ELSE 0 END) 'Sun'
,SUM(CASE WHEN Date=DATEADD(DAY, 0, @date) THEN Qty ELSE 0 END) 'Mon'
,SUM(CASE WHEN Date=DATEADD(DAY, 1, @date) THEN Qty ELSE 0 END) 'Tue'
,SUM(CASE WHEN Date=DATEADD(DAY, 2, @date) THEN Qty ELSE 0 END) 'Wed'
,SUM(Qty)'Total'
FROM @ORDER
GROUP BY Item)
SELECT x.Item,(y.[Description]+y.[Description 2])'Decpt',Fri,Sat,Sun,Mon,Tue,Wed,x.Total,z.Total 'OnHand',(CASE WHEN v.Qty IS NULL THEN 0 ELSE v.Qty END)'rsved',
(CASE WHEN SUM(n.[Remaining Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(n.[Remaining Quantity])) END)'OnProd',
(CASE WHEN [Expiration Calculation] LIKE '%' THEN 1*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 7*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 30*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 365*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE 0 END) END) END) END)'life'
FROM WeekQty x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.Item
LEFT JOIN @INVENTORY z ON z.Item=x.Item
LEFT JOIN @OLD_ORDER v ON v.Item=x.Item
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Prod_ Order Line] n ON n.[Location Code]=@location AND n.Status=3 AND n.[Item No_]=x.Item
" + fulfill + @"
GROUP BY x.Item,y.[Description],y.[Description 2],Fri,Sat,Sun,Mon,Tue,Wed,x.Total,z.Total,v.Qty,[Expiration Calculation]
";
                }
                else
                {
                    lblSource.Text = "Source: Forecast";
                    sql = @"DECLARE @date Date = '" + ddlCycle.Text + @"' 
                                DECLARE @location VARCHAR(5) = '" + ddlDC.Text + @"'
                                DECLARE @ORDER TABLE(
Item varchar(25),
Parent varchar(25),
Qty_Per Decimal(22,6),
Date Date,
Qty Decimal(22,6),
Level int,
Processed bigint
)
DECLARE @OLD_ORDER TABLE(
Item varchar(25),
Qty Decimal(22,6)
)
DECLARE @STKU TABLE(
Item varchar(25),
Parent varchar(25),
Date Date,
Qty Decimal(22,6),
OnHand Decimal(22,6),
Code varchar(50)
)
DECLARE @INVENTORY TABLE(
Item varchar(25),
Qty Decimal(22,6),
Total Decimal(22,6)
)
DECLARE @max INT
DECLARE @item varchar(25)
DECLARE @qty Decimal(22,6)
DECLARE @in_qty Decimal
DECLARE @min_date Date
DECLARE @max_date Date

INSERT INTO @ORDER
SELECT [Item No_],'',0,[Forecast Date],[Forecast Quantity],0,0
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location

--INSERT PRE DEMAND
IF GETDATE()>=DATEADD(DAY,-5,@date)
	INSERT INTO @ORDER
	SELECT [Item No_],'',0,DATEADD(DAY, -4, @date),SUM([Forecast Quantity]),0,0
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
	WHERE [Forecast Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location
	GROUP BY [Item No_]
ELSE
BEGIN
	WITH OLD AS (SELECT [Item No_],SUM([Forecast Quantity])'Qty'
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
	WHERE [Forecast Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location
	GROUP BY [Item No_] UNION SELECT [No_],SUM(Quantity)'Qty'
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
	WHERE [Planned Shipment Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
	GROUP BY [No_])
	INSERT INTO @ORDER
	SELECT [Item No_],'',0,DATEADD(DAY, -4, @date),SUM(Qty),0,0 FROM OLD
	GROUP BY [Item No_]
END

--EXPLORE THE BOM
DECLARE @level int = 0
While (Select Count(*) From @ORDER WHERE Level=@level) > 0
Begin
	DELETE FROM @STKU
	INSERT INTO @STKU
	SELECT (CASE WHEN (y.[Production BOM No_]='' OR y.[Production BOM No_] IS NULL) THEN x.Item ELSE y.[Production BOM No_] END),x.Item,Date,Qty,0,z.[Item Category Code] FROM @ORDER x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] z ON z.No_=x.Item
	WHERE Level=@level	
	GROUP BY x.Item,y.[Production BOM No_],Date,Qty,z.[Item Category Code]
	INSERT INTO @ORDER
	SELECT x.[No_],z.Parent,[Quantity per],z.Date,SUM([Quantity per]*Qty),(@level+1),0
	FROM @STKU z
	INNER JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x ON x.[Production BOM No_]=Item 
	WHERE ((x.No_ LIKE '1%' AND Code!='TRAY' AND x.[Production BOM No_] NOT LIKE '2%' AND x.[Production BOM No_] NOT LIKE '3%' AND x.[Production BOM No_] NOT LIKE '4%') OR (x.No_ NOT LIKE '1%' AND x.No_ LIKE '%')) 
	AND ((x.[Starting Date]<=z.Date AND x.[Ending Date]>=z.Date) OR (x.[Starting Date]<=z.Date AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=z.Date))
	GROUP BY x.No_,z.Parent,[Quantity per],z.Date
	SET @level=@level+1
END
DELETE FROM @ORDER WHERE Level=0 OR Item LIKE '8%'

--GET INVENTORY
;WITH ITEMS AS (SELECT Item FROM @ORDER GROUP BY Item)
INSERT INTO @INVENTORY
SELECT Item,SUM((CASE WHEN Quantity IS NULL THEN 0 ELSE Quantity END)),SUM((CASE WHEN Quantity IS NULL THEN 0 ELSE Quantity END))
FROM ITEMS x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] y ON y.[Item No_]=x.Item AND y.[Location Code]=@location
GROUP BY Item

--APPLY INVERTORY
SELECT @max = MAX(Level) FROM @ORDER
SELECT @min_date = MIN(Date) FROM @ORDER
SELECT @max_date = MAX(Date) FROM @ORDER
--LOOP DATE
WHILE @min_date<=@max_date
BEGIN
	SET @level=1
	--LOOP LEVEL
	WHILE @level<=@max
	BEGIN
		WHILE (SELECT COUNT(*) FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%')>0
		BEGIN
			SELECT TOP 1 @item = Item FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%'
			SELECT TOP 1 @qty = Qty FROM @ORDER WHERE Date=@min_date AND Level=@level AND Processed=0 AND Item NOT LIKE '1%'
			SELECT @in_qty = Qty FROM @INVENTORY WHERE Item=@item
			if @qty<=@in_qty
			Begin
				UPDATE @INVENTORY SET Qty= @in_qty-@qty WHERE Item=@item
				UPDATE @ORDER SET Qty=0 WHERE Date=@min_date AND Level=(@level+1) AND Parent=@item
			End
			Else
			Begin
				UPDATE @ORDER SET Qty=(Qty-(@in_qty*Qty_Per)) WHERE Date=@min_date AND Level=(@level+1) AND Parent=@item
				UPDATE @INVENTORY SET Qty= 0 WHERE Item=@item
			End
			UPDATE @ORDER SET Processed=1 WHERE Date=@min_date AND Level=@level AND Item=@item
		END
		SET @level= @level+1
	END
	SET @min_date= DATEADD(DAY,1,@min_date)
END

INSERT INTO @OLD_ORDER SELECT Item,SUM(Qty) FROM @ORDER WHERE Date=DATEADD(DAY, -4, @date) GROUP BY Item
DELETE FROM @ORDER WHERE Date=DATEADD(DAY, -4, @date) OR Item NOT LIKE '" + ddlItem.Text + @"'

;WITH WeekQty AS(SELECT Item,SUM(CASE WHEN Date=DATEADD(DAY, -3, @date) THEN Qty ELSE 0 END) 'Fri'
,SUM(CASE WHEN Date=DATEADD(DAY, -2, @date) THEN Qty ELSE 0 END) 'Sat'
,SUM(CASE WHEN Date=DATEADD(DAY, -1, @date) THEN Qty ELSE 0 END) 'Sun'
,SUM(CASE WHEN Date=DATEADD(DAY, 0, @date) THEN Qty ELSE 0 END) 'Mon'
,SUM(CASE WHEN Date=DATEADD(DAY, 1, @date) THEN Qty ELSE 0 END) 'Tue'
,SUM(CASE WHEN Date=DATEADD(DAY, 2, @date) THEN Qty ELSE 0 END) 'Wed'
,SUM(Qty)'Total'
FROM @ORDER
GROUP BY Item)
SELECT x.Item,(y.[Description]+y.[Description 2])'Decpt',Fri,Sat,Sun,Mon,Tue,Wed,x.Total,z.Total 'OnHand',(CASE WHEN v.Qty IS NULL THEN 0 ELSE v.Qty END)'rsved',
(CASE WHEN SUM(n.[Remaining Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(n.[Remaining Quantity])) END)'OnProd',
(CASE WHEN [Expiration Calculation] LIKE '%' THEN 1*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 7*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 30*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 365*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE 0 END) END) END) END)'life'
FROM WeekQty x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.Item
LEFT JOIN @INVENTORY z ON z.Item=x.Item
LEFT JOIN @OLD_ORDER v ON v.Item=x.Item
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Prod_ Order Line] n ON n.[Location Code]=@location AND n.Status=3 AND n.[Item No_]=x.Item
" + fulfill + @"
GROUP BY x.Item,y.[Description],y.[Description 2],Fri,Sat,Sun,Mon,Tue,Wed,x.Total,z.Total,v.Qty,[Expiration Calculation]
";
                }

                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["PrepTrackerConnectionString2"].ConnectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GridView1.DataSource = dt;
                GridView1.DataBind();
                con.Close();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GridView1.ClientID + "', 760, 1811, 57 ,true); </script>", false);
            }
            catch (Exception ex)
            {
                string error = ex.Message.Replace('"', ' ').Replace("'", " ");
                Response.Write("<script>alert('" + error + "');</script>");
            }
            

        }
        //decimal TTFNeeded = 0;
        //decimal TTFRsv = 0;
        //decimal TTSANeeded = 0;
        //decimal TTSARsv = 0;
        //decimal TTSUNeeded = 0;
        //decimal TTSURsv = 0;
        //decimal TTMNeeded = 0;
        //decimal TTMRsv = 0;
        //decimal TTTNeeded = 0;
        //decimal TTTRsv = 0;
        //decimal TTWNeeded = 0;
        //decimal TTWRsv = 0;
        //decimal TTDemand = 0;
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                decimal Total = Convert.ToDecimal(((Label)e.Row.FindControl("lblTotal")).Text);
                //TTDemand += Total;
                decimal onprod = Convert.ToDecimal(((Label)e.Row.FindControl("lblInProd")).Text);
                if (onprod < 0)
                {
                    ((Label)e.Row.FindControl("lblInProd")).Text = "0";
                }
                decimal onhand = Convert.ToDecimal(((Label)e.Row.FindControl("lblTotalOnHand")).Text);
                decimal committed = Convert.ToDecimal(((Label)e.Row.FindControl("lblCommitted")).Text);
                int life = Convert.ToInt32(((Label)e.Row.FindControl("lblLife")).Text)-1;
                
                onhand = onhand - committed;
                if (onhand < 0)
                {
                    onhand = 0;
                }
                Color green = System.Drawing.ColorTranslator.FromHtml("#00B050");
                Color yellow = System.Drawing.ColorTranslator.FromHtml("#FFA200");
                Color red = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                Color gray = Color.Gray;
                DateTime cycle = Convert.ToDateTime(ddlCycle.Text);
                if (DateTime.Today.AddDays(life) < cycle.AddDays(-3))
                {
                    e.Row.Cells[2].BackColor = gray;
                    e.Row.Cells[3].BackColor = gray;
                    e.Row.Cells[4].BackColor = gray;
                }
                if (DateTime.Today.AddDays(life) < cycle.AddDays(-2))
                {
                    e.Row.Cells[6].BackColor = gray;
                    e.Row.Cells[7].BackColor = gray;
                    e.Row.Cells[5].BackColor = gray;
                }
                if (DateTime.Today.AddDays(life) < cycle.AddDays(-1))
                {
                    e.Row.Cells[9].BackColor = gray;
                    e.Row.Cells[10].BackColor = gray;
                    e.Row.Cells[8].BackColor = gray;
                }
                if (DateTime.Today.AddDays(life) < cycle)
                {
                    e.Row.Cells[12].BackColor = gray;
                    e.Row.Cells[13].BackColor = gray;
                    e.Row.Cells[11].BackColor = gray;
                }
                if (DateTime.Today.AddDays(life) < cycle.AddDays(1))
                {
                    e.Row.Cells[15].BackColor = gray;
                    e.Row.Cells[16].BackColor = gray;
                    e.Row.Cells[14].BackColor = gray;
                }
                if (DateTime.Today.AddDays(life) < cycle.AddDays(2))
                {
                    e.Row.Cells[19].BackColor = gray;
                    e.Row.Cells[18].BackColor = gray;
                    e.Row.Cells[17].BackColor = gray;
                }
                Decimal fri = Convert.ToDecimal(((Label)e.Row.FindControl("lblFriNeeded")).Text);
                Label FriNeeded = (Label)e.Row.FindControl("lblFriNeeded");
                Label FriRsv = (Label)e.Row.FindControl("lblFriOnHand");
                Label FriPer = (Label)e.Row.FindControl("lblFriPer");
                //TTFNeeded += fri;
                if (fri == 0)
                {
                    FriRsv.Text = "-";
                    FriPer.Text = "-";
                }
                else if (fri <= onhand)
                {
                    FriRsv.Text = fri.ToString();
                    onhand = onhand - fri;
                    FriPer.Text = "100%";
                    FriNeeded.ForeColor = green;
                    FriRsv.ForeColor = green;
                    FriPer.ForeColor = green;
                    //TTFRsv += fri;
                }
                else
                {
                    FriRsv.Text = onhand.ToString("0.###");
                    FriPer.Text = (onhand * 100 / fri).ToString("0") + "%";
                    //TTFRsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle.AddDays(-4))/*org*/
                    {
                        FriNeeded.ForeColor = yellow;
                        FriRsv.ForeColor = yellow;
                        FriPer.ForeColor = yellow;
                        FriNeeded.Font.Bold = true;
                        FriRsv.Font.Bold = true;
                        FriPer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle.AddDays(-3))
                    {
                        FriNeeded.ForeColor = red;
                        FriRsv.ForeColor = red;
                        FriPer.ForeColor = red;
                        FriNeeded.Font.Bold = true;
                        FriRsv.Font.Bold = true;
                        FriPer.Font.Bold = true;
                    }
                }
                Decimal Sat = Convert.ToDecimal(((Label)e.Row.FindControl("lblSatNeeded")).Text);
                Label SatNeeded = (Label)e.Row.FindControl("lblSatNeeded");
                Label SatRsv = (Label)e.Row.FindControl("lblSatOnHand");
                Label SatPer = (Label)e.Row.FindControl("lblSatPer");
                //TTSANeeded += Sat;
                if (Sat == 0)
                {
                    SatRsv.Text = "-";
                    SatPer.Text = "-";
                }
                else if (Sat <= onhand)
                {
                    SatRsv.Text = Sat.ToString();
                    onhand = onhand - Sat;
                    SatPer.Text = "100%";
                    SatNeeded.ForeColor = green;
                    SatRsv.ForeColor = green;
                    SatPer.ForeColor = green;
                    //TTSARsv += Sat;
                }
                else
                {
                    SatRsv.Text = onhand.ToString("0.###");
                    SatPer.Text = (onhand * 100 / Sat).ToString("0") + "%";
                    //TTSARsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle.AddDays(-3))/*org*/
                    {
                        SatNeeded.ForeColor = yellow;
                        SatRsv.ForeColor = yellow;
                        SatPer.ForeColor = yellow;
                        SatNeeded.Font.Bold = true;
                        SatRsv.Font.Bold = true;
                        SatPer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle.AddDays(-2))
                    {
                        SatNeeded.ForeColor = red;
                        SatRsv.ForeColor = red;
                        SatPer.ForeColor = red;
                        SatNeeded.Font.Bold = true;
                        SatRsv.Font.Bold = true;
                        SatPer.Font.Bold = true;
                    }
                }
                Decimal Sun = Convert.ToDecimal(((Label)e.Row.FindControl("lblSunNeeded")).Text);
                Label SunNeeded = (Label)e.Row.FindControl("lblSunNeeded");
                Label SunRsv = (Label)e.Row.FindControl("lblSunOnHand");
                Label SunPer = (Label)e.Row.FindControl("lblSunPer");
                //TTSUNeeded += Sun;
                if (Sun == 0)
                {
                    SunRsv.Text = "-";
                    SunPer.Text = "-";
                }
                else if (Sun <= onhand)
                {
                    SunRsv.Text = Sun.ToString();
                    onhand = onhand - Sun;
                    SunPer.Text = "100%";
                    SunNeeded.ForeColor = green;
                    SunRsv.ForeColor = green;
                    SunPer.ForeColor = green;
                    //TTSURsv += Sun;
                }
                else
                {
                    SunRsv.Text = onhand.ToString("0.###");
                    SunPer.Text = (onhand * 100 / Sun).ToString("0") + "%";
                    //TTSURsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle.AddDays(-2))/*org*/
                    {
                        SunNeeded.ForeColor = yellow;
                        SunRsv.ForeColor = yellow;
                        SunPer.ForeColor = yellow;
                        SunNeeded.Font.Bold = true;
                        SunRsv.Font.Bold = true;
                        SunPer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle.AddDays(-1))
                    {
                        SunNeeded.ForeColor = red;
                        SunRsv.ForeColor = red;
                        SunPer.ForeColor = red;
                        SunNeeded.Font.Bold = true;
                        SunRsv.Font.Bold = true;
                        SunPer.Font.Bold = true;
                    }
                }
                Decimal Mon = Convert.ToDecimal(((Label)e.Row.FindControl("lblMonNeeded")).Text);
                Label MonNeeded = (Label)e.Row.FindControl("lblMonNeeded");
                Label MonRsv = (Label)e.Row.FindControl("lblMonOnHand");
                Label MonPer = (Label)e.Row.FindControl("lblMonPer");
                //TTMNeeded += Mon;
                if (Mon == 0)
                {
                    MonRsv.Text = "-";
                    MonPer.Text = "-";
                }
                else if (Mon <= onhand)
                {
                    MonRsv.Text = Mon.ToString();
                    onhand = onhand - Mon;
                    MonPer.Text = "100%";
                    MonNeeded.ForeColor = green;
                    MonRsv.ForeColor = green;
                    MonPer.ForeColor = green;
                    //TTMRsv += Mon;
                }
                else
                {
                    MonRsv.Text = onhand.ToString("0.###");
                    MonPer.Text = (onhand * 100 / Mon).ToString("0") + "%";
                    //TTMRsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle.AddDays(-1))/*org*/
                    {
                        MonNeeded.ForeColor = yellow;
                        MonRsv.ForeColor = yellow;
                        MonPer.ForeColor = yellow;
                        MonNeeded.Font.Bold = true;
                        MonRsv.Font.Bold = true;
                        MonPer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle)
                    {
                        MonNeeded.ForeColor = red;
                        MonRsv.ForeColor = red;
                        MonPer.ForeColor = red;
                        MonNeeded.Font.Bold = true;
                        MonRsv.Font.Bold = true;
                        MonPer.Font.Bold = true;
                    }
                }
                Decimal Tue = Convert.ToDecimal(((Label)e.Row.FindControl("lblTueNeeded")).Text);
                Label TueNeeded = (Label)e.Row.FindControl("lblTueNeeded");
                Label TueRsv = (Label)e.Row.FindControl("lblTueOnHand");
                Label TuePer = (Label)e.Row.FindControl("lblTuePer");
                //TTTNeeded += Tue;
                if (Tue == 0)
                {
                    TueRsv.Text = "-";
                    TuePer.Text = "-";
                }
                else if (Tue <= onhand)
                {
                    TueRsv.Text = Tue.ToString();
                    onhand = onhand - Tue;
                    TuePer.Text = "100%";
                    TueNeeded.ForeColor = green;
                    TueRsv.ForeColor = green;
                    TuePer.ForeColor = green;
                    //TTTRsv += Tue;
                }
                else
                {
                    TueRsv.Text = onhand.ToString("0.###");
                    TuePer.Text = (onhand * 100 / Tue).ToString("0") + "%";
                    //TTTRsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle)/*org*/
                    {
                        TueNeeded.ForeColor = yellow;
                        TueRsv.ForeColor = yellow;
                        TuePer.ForeColor = yellow;
                        TueNeeded.Font.Bold = true;
                        TueRsv.Font.Bold = true;
                        TuePer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle.AddDays(1))
                    {
                        TueNeeded.ForeColor = red;
                        TueRsv.ForeColor = red;
                        TuePer.ForeColor = red;
                        TueNeeded.Font.Bold = true;
                        TueRsv.Font.Bold = true;
                        TuePer.Font.Bold = true;
                    }
                }
                Decimal Wed = Convert.ToDecimal(((Label)e.Row.FindControl("lblWedNeeded")).Text);
                Label WedNeeded = (Label)e.Row.FindControl("lblWedNeeded");
                Label WedRsv = (Label)e.Row.FindControl("lblWedOnHand");
                Label WedPer = (Label)e.Row.FindControl("lblWedPer");
                //TTWNeeded += Wed;
                if (Wed == 0)
                {
                    WedRsv.Text = "-";
                    WedPer.Text = "-";
                }
                else if (Wed <= onhand)
                {
                    WedRsv.Text = Wed.ToString();
                    onhand = onhand - Wed;
                    WedPer.Text = "100%";
                    WedNeeded.ForeColor = green;
                    WedRsv.ForeColor = green;
                    WedPer.ForeColor = green;
                    //TTWRsv += Wed;
                }
                else
                {
                    WedRsv.Text = onhand.ToString("0.###");
                    WedPer.Text = (onhand * 100 / Wed).ToString("0") + "%";
                    //TTWRsv += onhand;
                    onhand = onhand - onhand;
                    if (DateTime.Today == cycle.AddDays(1))/*org*/
                    {
                        WedNeeded.ForeColor = yellow;
                        WedRsv.ForeColor = yellow;
                        WedPer.ForeColor = yellow;
                        WedNeeded.Font.Bold = true;
                        WedRsv.Font.Bold = true;
                        WedPer.Font.Bold = true;
                    }
                    else if (DateTime.Today >= cycle.AddDays(2))
                    {
                        WedNeeded.ForeColor = red;
                        WedRsv.ForeColor = red;
                        WedPer.ForeColor = red;
                        WedNeeded.Font.Bold = true;
                        WedRsv.Font.Bold = true;
                        WedPer.Font.Bold = true;
                    }
                }
            }
            //if (e.Row.RowType == DataControlRowType.Footer)
            //{
            //    ((Label)e.Row.FindControl("lblTotalFriNeeded")).Text = TTFNeeded.ToString();
            //    if(TTFNeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalFriRsv")).Text = TTFRsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalFriPer")).Text = ((TTFRsv * 100) / TTFNeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalSatNeeded")).Text = TTSANeeded.ToString("0.###");
            //    if (TTSANeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalSatRsv")).Text = TTSARsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalSatPer")).Text = ((TTSARsv * 100) / TTSANeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalSunNeeded")).Text = TTSUNeeded.ToString("0.###");
            //    if (TTSUNeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalSunRsv")).Text = TTSURsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalSunPer")).Text = ((TTSURsv * 100) / TTSUNeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalMonNeeded")).Text = TTMNeeded.ToString("0.###");
            //    if (TTMNeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalMonRsv")).Text = TTMRsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalMonPer")).Text = ((TTMRsv * 100) / TTMNeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalTueNeeded")).Text = TTTNeeded.ToString("0.###");
            //    if (TTTNeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalTueRsv")).Text = TTTRsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalTuePer")).Text = ((TTTRsv * 100) / TTTNeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalWedNeeded")).Text = TTWNeeded.ToString("0.###");
            //    if (TTWNeeded != 0)
            //    {
            //        ((Label)e.Row.FindControl("lblTotalWedRsv")).Text = TTWRsv.ToString("0.###");
            //        ((Label)e.Row.FindControl("lblTotalWedPer")).Text = ((TTWRsv * 100) / TTWNeeded).ToString("0") + "%";
            //    }
            //    ((Label)e.Row.FindControl("lblTotalDemand")).Text = TTDemand.ToString("0.###");

            //}
        }

        protected void ddlCycle_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }

        protected void ddlDC_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }
        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                GridView HeaderGrid = (GridView)sender;
                GridViewRow HeaderGridRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Insert);
                TableCell HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderGridRow.Cells.Add(HeaderCell);

                HeaderCell = new TableCell();
                HeaderCell.Text = "Friday";
                HeaderCell.ColumnSpan = 3;
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 3;
                HeaderCell.Text = "Saturday";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 3;
                HeaderCell.Text = "Sunday";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 3;
                HeaderCell.Text = "Monday";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 3;
                HeaderCell.Text = "Tuesday";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 3;
                HeaderCell.Text = "Wednesday";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderCell.Text = "Total";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderCell.Text = "Total";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderCell.Text = "Previous";
                HeaderGridRow.Cells.Add(HeaderCell);
                HeaderCell = new TableCell();
                HeaderCell.ColumnSpan = 1;
                HeaderCell.Text = "Remaining";
                HeaderGridRow.Cells.Add(HeaderCell);

                GridView1.Controls[0].Controls.AddAt(0, HeaderGridRow);

            }
        }

        protected void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }
        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            GetData();
        }
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            GetData();
        }
    }
}