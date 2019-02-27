using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
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
                    
                SqlCommand permiss = new SqlCommand("SELECT COUNT(*) FROM PrepTrackerPermission WHERE Account2000 = '" + Context.User.Identity.Name + "';", conn);
                int permiss_count = Convert.ToInt32(permiss.ExecuteScalar());
                if (permiss_count == 0)
                {
                    conn.Close(); //close the connection
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "redirect", "alert('Sorry! You do not have access to this application.'); window.location='../';", true);
                }
                else
                {
                    
                    SqlCommand comm = new SqlCommand("SELECT Permission FROM PrepTrackerPermission WHERE Account2000 = '" + Context.User.Identity.Name + "';", conn);
                    String Permission = Convert.ToString(comm.ExecuteScalar());
                    SqlCommand DC_comm = new SqlCommand("SELECT (CASE WHEN x.OfficeID=0 then 'ALL' else y.Office end)'Office' FROM PrepTrackerPermission x LEFT JOIN DC y ON y.OfficeID=x.OfficeID WHERE Account2000 = '" + Context.User.Identity.Name + "';", conn);
                    String DC = Convert.ToString(DC_comm.ExecuteScalar());
                    DateTime today = DateTime.Now;
                    SqlCommand commdate = new SqlCommand("UPDATE PrepTrackerPermission SET LastLogin='" + today + "' WHERE Account2000 = '" + Context.User.Identity.Name + "';", conn);
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
                string fulfill = "";
                if (cbFulfill.Checked == true)
                {
                    fulfill = "WHERE (OnHand-rsved-Fri-Sat-Sun-Mon-Tue-Wed)<0";
                }
                else
                {
                    fulfill = "";
                }
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
                                SELECT No_,'',[Shipment Date],SUM(Quantity)'Qty',''
                                FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
                                WHERE [Location Code]=@location AND [Shipment Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND No_!='WELCOME_BOOKLET' AND No_!='FREIGHT' AND No_!='MENU_BOOKLET' AND No_!=''
                                GROUP BY No_,[Shipment Date]

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
	                                SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty,@item
	                                FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
									LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	                                WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	                                DELETE FROM #Output WHERE Item=@item AND Date=@pdate
                                END

                                INSERT INTO #Output
                                SELECT Item,Decpt,Date,Qty
                                FROM #Temp

                                DELETE FROM #Temp WHERE Item NOT LIKE '6%'

CREATE TABLE #6K (
    Item varchar(255)
    )
INSERT INTO #6K
SELECT Item
FROM #Temp
GROUP BY Item

While (Select Count(*) From #6K) > 0
Begin
	Select Top 1 @item = Item From #6K
	SELECT @hand = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] WHERE [Location Code]=@location AND [Item No_]=@item COLLATE DATABASE_DEFAULT
	IF @hand>0
	Begin
		CREATE TABLE #6K_Date (
		Date Date
		)
		INSERT INTO #6K_Date
		SELECT Date
		FROM #TEMP
		WHERE Item=@item

		While (Select Count(*) From #6K_Date) > 0
		Begin
			Select Top 1 @pdate = Date From #6K_Date ORDER BY Date
			Select @qty = Qty From #Temp WHERE Item=@item AND Date=@pdate
			if @qty<=@hand
			Begin
				Select @hand = @hand-@qty
                UPDATE #TEMP SET Qty = 0 WHERE Item =@item AND Date=@pdate
				--DELETE FROM #TEMP WHERE Item =@item AND Date=@pdate
			End
			Else
			Begin
				UPDATE #TEMP SET Qty = (@qty-@hand) WHERE Item =@item AND Date=@pdate
				Select @hand = 0
			End
			DELETE FROM #6K_Date WHERE Date=@pdate
		End
		DROP TABLE #6K_Date
	End
	DELETE FROM #6K WHERE Item=@item
End
DROP TABLE #6K

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
	                                SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty
	                                FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
									LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	                                WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%' OR x.[No_] LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	                                DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
                                END
                                CREATE TABLE #Final (
                                    Item varchar(255),
	                                Decpt varchar(255),
	                                Fri int,
	                                Sat int,
	                                Sun int,
	                                Mon int,
	                                Tue int,
									Wed int
                                   )
                                INSERT INTO #Final
                                SELECT Item,Decpt,SUM(CASE WHEN Date=DATEADD(DAY, -3, @date) THEN Qty ELSE 0 END) 'Fri'
                                ,SUM(CASE WHEN Date=DATEADD(DAY, -2, @date) THEN Qty ELSE 0 END) 'Sat'
                                ,SUM(CASE WHEN Date=DATEADD(DAY, -1, @date) THEN Qty ELSE 0 END) 'Sun'
                                ,SUM(CASE WHEN Date=DATEADD(DAY, 0, @date) THEN Qty ELSE 0 END) 'Mon'
                                ,SUM(CASE WHEN Date=DATEADD(DAY, 1, @date) THEN Qty ELSE 0 END) 'Tue'
								,SUM(CASE WHEN Date=DATEADD(DAY, 2, @date) THEN Qty ELSE 0 END) 'Wed'
                                FROM #Output
								WHERE Item LIKE '" + ddlItem.Text + @"'
                                GROUP BY Item,Decpt
CREATE TABLE #Final2 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
	Wed int,
	OnHand int,
	rsved int
    )
INSERT INTO #Final2
SELECT Item,Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END)'OnHand','0' 'rsved'
FROM #Final x LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] y ON [Location Code]=@location AND [Item No_]=Item COLLATE DATABASE_DEFAULT
GROUP BY Item,Decpt,Fri,Sat,Sun,Mon,Tue,Wed

CREATE TABLE #Final3 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
	Wed int,
	OnHand int,
	rsved int,
	OnProd int
    )
INSERT INTO #Final3
SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,(CASE WHEN SUM(y.[Remaining Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(y.[Remaining Quantity])) END)
FROM #Final2 x 
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Prod_ Order Line] y ON y.[Location Code]=@location AND y.Status=3 AND y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved

SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(Fri+Sat+Sun+Mon+Tue+Wed)'Total',OnHand,rsved,OnProd,
(CASE WHEN [Expiration Calculation] LIKE '%' THEN 1*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 7*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 30*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 365*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE 0 END) END) END) END)'life'
FROM #Final3 x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] n ON n.No_ = x.Item COLLATE DATABASE_DEFAULT
" + fulfill + @"
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,OnProd,[Expiration Calculation]

DROP TABLE #Sub
DROP TABLE #Output
DROP TABLE #Temp
DROP TABLE #Final
DROP TABLE #Final2
DROP TABLE #Final3
";
                }
                else if (DateTime.Today >= Convert.ToDateTime(ddlCycle.Text).AddDays(-5))
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
SELECT [Item No_],[Description],[Forecast Date],[Forecast Quantity],''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location

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
DECLARE @fused int
DECLARE @wused int
DECLARE @BOM VARCHAR(50)

While (Select Count(*) From #Output) > 0
Begin
	Select Top 1 @item = Item From #Output
	Select Top 1 @pdate = Date From #Output
	Select Top 1 @qty = Qty From #Output

	INSERT INTO #Temp
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty,@item
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

CREATE TABLE #6K (
    Item varchar(255)
    )
CREATE TABLE #6K_Date (
	Date Date
	)
INSERT INTO #6K
SELECT Item
FROM #Temp
GROUP BY Item

While (Select Count(*) From #6K) > 0
Begin
	Select Top 1 @item = Item From #6K
	SELECT @BOM = [Production BOM No_] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] WHERE [No_]=@item
	SELECT @fused = (CASE WHEN SUM([Forecast Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Forecast Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry] WHERE [Forecast Date] <DATEADD(DAY, -4, @date) AND [Location Code]=@location AND [Item No_]=@BOM
    SELECT @hand = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] WHERE [Location Code]=@location AND [Item No_]=@item COLLATE DATABASE_DEFAULT
	SELECT @hand= @hand-@fused
	IF @hand>0
	Begin
		INSERT INTO #6K_Date
		SELECT Date
		FROM #TEMP
		WHERE Item=@item

		While (Select Count(*) From #6K_Date) > 0
		Begin
			Select Top 1 @pdate = Date From #6K_Date ORDER BY Date
			Select @qty = Qty From #Temp WHERE Item=@item AND Date=@pdate
			if @qty<=@hand
			Begin
				Select @hand = @hand-@qty
				DELETE FROM #TEMP WHERE Item =@item AND Date=@pdate
			End
			Else
			Begin
				UPDATE #TEMP SET Qty = (@qty-@hand) WHERE Item =@item AND Date=@pdate
				Select @hand = 0
			End
			DELETE FROM #6K_Date WHERE Date=@pdate
		End
	End
	DELETE FROM #6K WHERE Item=@item
End

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
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%' OR x.[No_] LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END

CREATE TABLE #Final (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
    Wed int
    )
INSERT INTO #Final
SELECT Item,Decpt,SUM(CASE WHEN Date=DATEADD(DAY, -3, @date) THEN Qty ELSE 0 END) 'Fri'
,SUM(CASE WHEN Date=DATEADD(DAY, -2, @date) THEN Qty ELSE 0 END) 'Sat'
,SUM(CASE WHEN Date=DATEADD(DAY, -1, @date) THEN Qty ELSE 0 END) 'Sun'
,SUM(CASE WHEN Date=DATEADD(DAY, 0, @date) THEN Qty ELSE 0 END) 'Mon'
,SUM(CASE WHEN Date=DATEADD(DAY, 1, @date) THEN Qty ELSE 0 END) 'Tue'
,SUM(CASE WHEN Date=DATEADD(DAY, 2, @date) THEN Qty ELSE 0 END) 'Wed'
FROM #Output
WHERE Item LIKE '" + ddlItem.Text + @"'
GROUP BY Item,Decpt

DELETE FROM #Output

INSERT INTO #Sub
SELECT [Item No_],'','1/1/2018',SUM([Forecast Quantity]),''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location
GROUP BY [Item No_]

INSERT INTO #Output
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty
FROM #Sub x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Sub

While (Select Count(*) From #Output) > 0
Begin
	Select Top 1 @item = Item From #Output
	Select Top 1 @pdate = Date From #Output
	Select Top 1 @qty = Qty From #Output

	INSERT INTO #Temp
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty,@item
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

INSERT INTO #6K
SELECT Item
FROM #Temp
GROUP BY Item

While (Select Count(*) From #6K) > 0
Begin
	Select Top 1 @item = Item From #6K
	SELECT @hand = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] WHERE [Location Code]=@location AND [Item No_]=@item COLLATE DATABASE_DEFAULT
	IF @hand>0
	Begin
		INSERT INTO #6K_Date
		SELECT Date
		FROM #TEMP
		WHERE Item=@item

		While (Select Count(*) From #6K_Date) > 0
		Begin
			Select Top 1 @pdate = Date From #6K_Date ORDER BY Date
			Select @qty = Qty From #Temp WHERE Item=@item AND Date=@pdate
			if @qty<=@hand
			Begin
				Select @hand = @hand-@qty
				DELETE FROM #TEMP WHERE Item =@item AND Date=@pdate
			End
			Else
			Begin
				UPDATE #TEMP SET Qty = (@qty-@hand) WHERE Item =@item AND Date=@pdate
				Select @hand = 0
			End
			DELETE FROM #6K_Date WHERE Date=@pdate
		End
	End
	DELETE FROM #6K WHERE Item=@item
End

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
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%' OR x.[No_] LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END

INSERT INTO #Temp
SELECT Item,'','1/1/2018',SUM(Qty),''
FROM #Output
GROUP BY Item

CREATE TABLE #Final2 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
	Wed int,
	OnHand int,
	rsved int
    )
INSERT INTO #Final2
SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(CASE WHEN SUM(y.[Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(y.[Quantity])) END)'OnHand', (CASE WHEN z.Qty IS NULL THEN 0 ELSE FLOOR(z.Qty) END)'rsved'
FROM #Final x 
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] y ON y.[Location Code]=@location AND y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT
LEFT JOIN #Temp z ON z.Item=x.Item
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,z.Qty

CREATE TABLE #Final3 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
    Wed int,
	OnHand int,
	rsved int,
	OnProd int
    )
INSERT INTO #Final3
SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,(CASE WHEN SUM(y.[Remaining Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(y.[Remaining Quantity])) END)
FROM #Final2 x 
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Prod_ Order Line] y ON y.[Location Code]=@location AND y.Status=3 AND y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved

SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(Fri+Sat+Sun+Mon+Tue+Wed)'Total',OnHand,rsved,OnProd,
(CASE WHEN [Expiration Calculation] LIKE '%' THEN 1*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 7*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 30*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 365*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE 0 END) END) END) END)'life'
FROM #Final3 x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] n ON n.No_ = x.Item COLLATE DATABASE_DEFAULT
" + fulfill + @"
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,OnProd,[Expiration Calculation]

DROP TABLE #Sub
DROP TABLE #6K_Date
DROP TABLE #6K
DROP TABLE #Output
DROP TABLE #Temp
DROP TABLE #Final
DROP TABLE #Final2
DROP TABLE #Final3
";
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
SELECT [Item No_],[Description],[Forecast Date],[Forecast Quantity],''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] BETWEEN DATEADD(DAY, -3, @date) AND DATEADD(DAY, +2, @date) AND [Location Code]=@location

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
DECLARE @fused int
DECLARE @wused int
DECLARE @BOM VARCHAR(50)

While (Select Count(*) From #Output) > 0
Begin
	Select Top 1 @item = Item From #Output
	Select Top 1 @pdate = Date From #Output
	Select Top 1 @qty = Qty From #Output

	INSERT INTO #Temp
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty,@item
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

CREATE TABLE #6K (
    Item varchar(255)
    )
CREATE TABLE #6K_Date (
	Date Date
	)
INSERT INTO #6K
SELECT Item
FROM #Temp
GROUP BY Item

While (Select Count(*) From #6K) > 0
Begin
	Select Top 1 @item = Item From #6K
	SELECT @BOM = [Production BOM No_] FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] WHERE [No_]=@item
	SELECT @fused = (CASE WHEN SUM([Forecast Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Forecast Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry] WHERE [Forecast Date] <DATEADD(DAY, -4, @date) AND [Location Code]=@location AND [Item No_]=@BOM
	SELECT @wused = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line] WHERE [Shipment Date] <DATEADD(DAY, -4, @date) AND [Location Code]=@location AND No_=@BOM
	SELECT @hand = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] WHERE [Location Code]=@location AND [Item No_]=@item COLLATE DATABASE_DEFAULT
	SELECT @hand= @hand-@fused-@wused
	IF @hand>0
	Begin
		INSERT INTO #6K_Date
		SELECT Date
		FROM #TEMP
		WHERE Item=@item

		While (Select Count(*) From #6K_Date) > 0
		Begin
			Select Top 1 @pdate = Date From #6K_Date ORDER BY Date
			Select @qty = Qty From #Temp WHERE Item=@item AND Date=@pdate
			if @qty<=@hand
			Begin
				Select @hand = @hand-@qty
				DELETE FROM #TEMP WHERE Item =@item AND Date=@pdate
			End
			Else
			Begin
				UPDATE #TEMP SET Qty = (@qty-@hand) WHERE Item =@item AND Date=@pdate
				Select @hand = 0
			End
			DELETE FROM #6K_Date WHERE Date=@pdate
		End
	End
	DELETE FROM #6K WHERE Item=@item
End

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
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%' OR x.[No_] LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END

CREATE TABLE #Final (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
    Wed int
    )
INSERT INTO #Final
SELECT Item,Decpt,SUM(CASE WHEN Date=DATEADD(DAY, -3, @date) THEN Qty ELSE 0 END) 'Fri'
,SUM(CASE WHEN Date=DATEADD(DAY, -2, @date) THEN Qty ELSE 0 END) 'Sat'
,SUM(CASE WHEN Date=DATEADD(DAY, -1, @date) THEN Qty ELSE 0 END) 'Sun'
,SUM(CASE WHEN Date=DATEADD(DAY, 0, @date) THEN Qty ELSE 0 END) 'Mon'
,SUM(CASE WHEN Date=DATEADD(DAY, 1, @date) THEN Qty ELSE 0 END) 'Tue'
,SUM(CASE WHEN Date=DATEADD(DAY, 2, @date) THEN Qty ELSE 0 END) 'Wed'
FROM #Output
WHERE Item LIKE '" + ddlItem.Text + @"'
GROUP BY Item,Decpt

DELETE FROM #Output

INSERT INTO #Sub
SELECT [Item No_],'','1/1/2018',SUM([Forecast Quantity]),''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production Forecast Entry]
WHERE [Forecast Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location
GROUP BY [Item No_]
INSERT INTO #Sub
SELECT [No_],'','1/2/2018',SUM(Quantity),''
FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Web Order Line]
WHERE [Shipment Date] < DATEADD(DAY, -4, @date) AND [Location Code]=@location
GROUP BY [No_]

INSERT INTO #Output
SELECT (CASE WHEN ([Production BOM No_]='' OR [Production BOM No_] IS NULL) THEN x.Item ELSE [Production BOM No_] END),Decpt,Date,Qty
FROM #Sub x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Stockkeeping Unit] y ON y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT AND y.[Location Code]=@location

DELETE FROM #Sub

While (Select Count(*) From #Output) > 0
Begin
	Select Top 1 @item = Item From #Output
	Select Top 1 @pdate = Date From #Output
	Select Top 1 @qty = Qty From #Output

	INSERT INTO #Temp
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty,@item
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.No_ LIKE '3%' OR x.No_ LIKE '6%' OR x.No_ LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Output WHERE Item=@item AND Date=@pdate
END

INSERT INTO #Output
SELECT Item,Decpt,Date,Qty
FROM #Temp

DELETE FROM #Temp WHERE Item NOT LIKE '6%'

INSERT INTO #6K
SELECT Item
FROM #Temp
GROUP BY Item

While (Select Count(*) From #6K) > 0
Begin
	Select Top 1 @item = Item From #6K
	SELECT @hand = (CASE WHEN SUM([Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM([Quantity])) END) FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] WHERE [Location Code]=@location AND [Item No_]=@item COLLATE DATABASE_DEFAULT
	IF @hand>0
	Begin
		INSERT INTO #6K_Date
		SELECT Date
		FROM #TEMP
		WHERE Item=@item

		While (Select Count(*) From #6K_Date) > 0
		Begin
			Select Top 1 @pdate = Date From #6K_Date ORDER BY Date
			Select @qty = Qty From #Temp WHERE Item=@item AND Date=@pdate
			if @qty<=@hand
			Begin
				Select @hand = @hand-@qty
				DELETE FROM #TEMP WHERE Item =@item AND Date=@pdate
			End
			Else
			Begin
				UPDATE #TEMP SET Qty = (@qty-@hand) WHERE Item =@item AND Date=@pdate
				Select @hand = 0
			End
			DELETE FROM #6K_Date WHERE Date=@pdate
		End
	End
	DELETE FROM #6K WHERE Item=@item
End

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
	SELECT x.[No_],(y.[Description]+y.[Description 2]),@pdate,[Quantity per]*@qty
	FROM [SUNBASKET_1000_TEST].[dbo].[Receiving$Production BOM Line] x
	LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] y ON y.No_=x.No_
	WHERE x.[Production BOM No_]=@item AND (x.[No_] LIKE '3%' OR x.[No_] LIKE '1%') AND ((x.[Starting Date]<=@pdate AND x.[Ending Date]>=@pdate) OR (x.[Starting Date]<=@pdate AND x.[Ending Date]='1753-01-01') OR (x.[Starting Date]='1753-01-01' AND x.[Ending Date]>=@pdate))

	DELETE FROM #Sub WHERE Item=@item AND Date=@pdate AND Meal=@meal
END

INSERT INTO #Temp
SELECT Item,'','1/1/2018',SUM(Qty),''
FROM #Output
GROUP BY Item

CREATE TABLE #Final2 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
	Wed int,
	OnHand int,
	rsved int
    )
INSERT INTO #Final2
SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(CASE WHEN SUM(y.[Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(y.[Quantity])) END)'OnHand', (CASE WHEN z.Qty IS NULL THEN 0 ELSE FLOOR(z.Qty) END)'rsved'
FROM #Final x 
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item Ledger Entry] y ON y.[Location Code]=@location AND y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT
LEFT JOIN #Temp z ON z.Item=x.Item
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,z.Qty

CREATE TABLE #Final3 (
    Item varchar(255),
	Decpt varchar(255),
	Fri int,
	Sat int,
	Sun int,
	Mon int,
	Tue int,
    Wed int,
	OnHand int,
	rsved int,
	OnProd int
    )
INSERT INTO #Final3
SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,(CASE WHEN SUM(y.[Remaining Quantity]) IS NULL THEN 0 ELSE FLOOR(SUM(y.[Remaining Quantity])) END)
FROM #Final2 x 
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Prod_ Order Line] y ON y.[Location Code]=@location AND y.Status=3 AND y.[Item No_]=x.Item COLLATE DATABASE_DEFAULT
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved

SELECT x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,(Fri+Sat+Sun+Mon+Tue+Wed)'Total',OnHand,rsved,OnProd,
(CASE WHEN [Expiration Calculation] LIKE '%' THEN 1*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 7*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 30*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE (CASE WHEN [Expiration Calculation] LIKE '%' THEN 365*LEFT([Expiration Calculation],LEN([Expiration Calculation])-1) ELSE 0 END) END) END) END)'life'
FROM #Final3 x
LEFT JOIN [SUNBASKET_1000_TEST].[dbo].[Receiving$Item] n ON n.No_ = x.Item COLLATE DATABASE_DEFAULT
" + fulfill + @"
GROUP BY x.Item,x.Decpt,Fri,Sat,Sun,Mon,Tue,Wed,OnHand,rsved,OnProd,[Expiration Calculation]

DROP TABLE #Sub
DROP TABLE #6K_Date
DROP TABLE #6K
DROP TABLE #Output
DROP TABLE #Temp
DROP TABLE #Final
DROP TABLE #Final2
DROP TABLE #Final3
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
        int TTFNeeded = 0;
        int TTFRsv = 0;
        int TTSANeeded = 0;
        int TTSARsv = 0;
        int TTSUNeeded = 0;
        int TTSURsv = 0;
        int TTMNeeded = 0;
        int TTMRsv = 0;
        int TTTNeeded = 0;
        int TTTRsv = 0;
        int TTWNeeded = 0;
        int TTWRsv = 0;
        int TTDemand = 0;
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int Total = Convert.ToInt32(((Label)e.Row.FindControl("lblTotal")).Text);
                TTDemand += Total;
                int onprod = Convert.ToInt32(((Label)e.Row.FindControl("lblInProd")).Text);
                if (onprod < 0)
                {
                    ((Label)e.Row.FindControl("lblInProd")).Text = "0";
                }
                int onhand = Convert.ToInt32(((Label)e.Row.FindControl("lblTotalOnHand")).Text);
                int committed = Convert.ToInt32(((Label)e.Row.FindControl("lblCommitted")).Text);
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
                int fri = Convert.ToInt32(((Label)e.Row.FindControl("lblFriNeeded")).Text);
                Label FriNeeded = (Label)e.Row.FindControl("lblFriNeeded");
                Label FriRsv = (Label)e.Row.FindControl("lblFriOnHand");
                Label FriPer = (Label)e.Row.FindControl("lblFriPer");
                TTFNeeded += fri;
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
                    TTFRsv += fri;
                }
                else
                {
                    FriRsv.Text = onhand.ToString();
                    FriPer.Text = (onhand * 100 / fri) + "%";
                    TTFRsv += onhand;
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
                int Sat = Convert.ToInt32(((Label)e.Row.FindControl("lblSatNeeded")).Text);
                Label SatNeeded = (Label)e.Row.FindControl("lblSatNeeded");
                Label SatRsv = (Label)e.Row.FindControl("lblSatOnHand");
                Label SatPer = (Label)e.Row.FindControl("lblSatPer");
                TTSANeeded += Sat;
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
                    TTSARsv += Sat;
                }
                else
                {
                    SatRsv.Text = onhand.ToString();
                    SatPer.Text = (onhand * 100 / Sat) + "%";
                    TTSARsv += onhand;
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
                int Sun = Convert.ToInt32(((Label)e.Row.FindControl("lblSunNeeded")).Text);
                Label SunNeeded = (Label)e.Row.FindControl("lblSunNeeded");
                Label SunRsv = (Label)e.Row.FindControl("lblSunOnHand");
                Label SunPer = (Label)e.Row.FindControl("lblSunPer");
                TTSUNeeded += Sun;
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
                    TTSURsv += Sun;
                }
                else
                {
                    SunRsv.Text = onhand.ToString();
                    SunPer.Text = (onhand * 100 / Sun) + "%";
                    TTSURsv += onhand;
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
                int Mon = Convert.ToInt32(((Label)e.Row.FindControl("lblMonNeeded")).Text);
                Label MonNeeded = (Label)e.Row.FindControl("lblMonNeeded");
                Label MonRsv = (Label)e.Row.FindControl("lblMonOnHand");
                Label MonPer = (Label)e.Row.FindControl("lblMonPer");
                TTMNeeded += Mon;
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
                    TTMRsv += Mon;
                }
                else
                {
                    MonRsv.Text = onhand.ToString();
                    MonPer.Text = (onhand * 100 / Mon) + "%";
                    TTMRsv += onhand;
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
                int Tue = Convert.ToInt32(((Label)e.Row.FindControl("lblTueNeeded")).Text);
                Label TueNeeded = (Label)e.Row.FindControl("lblTueNeeded");
                Label TueRsv = (Label)e.Row.FindControl("lblTueOnHand");
                Label TuePer = (Label)e.Row.FindControl("lblTuePer");
                TTTNeeded += Tue;
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
                    TTTRsv += Tue;
                }
                else
                {
                    TueRsv.Text = onhand.ToString();
                    TuePer.Text = (onhand * 100 / Tue) + "%";
                    TTTRsv += onhand;
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
                int Wed = Convert.ToInt32(((Label)e.Row.FindControl("lblWedNeeded")).Text);
                Label WedNeeded = (Label)e.Row.FindControl("lblWedNeeded");
                Label WedRsv = (Label)e.Row.FindControl("lblWedOnHand");
                Label WedPer = (Label)e.Row.FindControl("lblWedPer");
                TTWNeeded += Wed;
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
                    TTWRsv += Wed;
                }
                else
                {
                    WedRsv.Text = onhand.ToString();
                    WedPer.Text = (onhand * 100 / Wed) + "%";
                    TTWRsv += onhand;
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
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                ((Label)e.Row.FindControl("lblTotalFriNeeded")).Text = TTFNeeded.ToString();
                if(TTFNeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalFriRsv")).Text = TTFRsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalFriPer")).Text = (TTFRsv * 100) / TTFNeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalSatNeeded")).Text = TTSANeeded.ToString();
                if (TTSANeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalSatRsv")).Text = TTSARsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalSatPer")).Text = (TTSARsv * 100) / TTSANeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalSunNeeded")).Text = TTSUNeeded.ToString();
                if (TTSUNeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalSunRsv")).Text = TTSURsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalSunPer")).Text = (TTSURsv * 100) / TTSUNeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalMonNeeded")).Text = TTMNeeded.ToString();
                if (TTMNeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalMonRsv")).Text = TTMRsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalMonPer")).Text = (TTMRsv * 100) / TTMNeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalTueNeeded")).Text = TTTNeeded.ToString();
                if (TTTNeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalTueRsv")).Text = TTTRsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalTuePer")).Text = (TTTRsv * 100) / TTTNeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalWedNeeded")).Text = TTWNeeded.ToString();
                if (TTWNeeded != 0)
                {
                    ((Label)e.Row.FindControl("lblTotalWedRsv")).Text = TTWRsv.ToString();
                    ((Label)e.Row.FindControl("lblTotalWedPer")).Text = (TTWRsv * 100) / TTWNeeded + "%";
                }
                ((Label)e.Row.FindControl("lblTotalDemand")).Text = TTDemand.ToString();

            }
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