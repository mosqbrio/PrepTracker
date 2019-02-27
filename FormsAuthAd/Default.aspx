<%@ Page Language="c#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PrepTracker.Default" %>

<%@ Import Namespace="System.Security.Principal" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />

    <title>PrepTracker</title>

    <!-- Bootstrap core CSS -->
    <link href="vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/jquery-ui.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="css/styles.css?v=2.0" />
    <script src="vendor/jquery/jquery-1.12.4.js"></script>
    <script src="vendor/jquery/jquery-ui.js"></script>
    
<script type="text/javascript">
 function MakeStaticHeader(gridId, height, width, headerHeight, isFooter) {
 var tbl = document.getElementById(gridId);
 if (tbl) {
 var DivHR = document.getElementById('DivHeaderRow');
 var DivMC = document.getElementById('DivMainContent');
 var DivFR = document.getElementById('DivFooterRow');

 //*** Set divheaderRow Properties ****
 DivHR.style.height = headerHeight + 'px';
 DivHR.style.width = (parseInt(width) - 16) + 'px';
 DivHR.style.position = 'relative';
 DivHR.style.top = '0px';
 DivHR.style.zIndex = '10';
 DivHR.style.verticalAlign = 'top';

 //*** Set divMainContent Properties ****
 DivMC.style.width = width + 'px';
DivMC.style.height = (window.innerHeight - 215) + 'px';
 DivMC.style.position = 'relative';
 DivMC.style.top = -headerHeight + 'px';
 DivMC.style.zIndex = '1';

 //*** Set divFooterRow Properties ****
 DivFR.style.width = (parseInt(width) - 16) + 'px';
 DivFR.style.position = 'relative';
 DivFR.style.top = -headerHeight + 'px';
 DivFR.style.verticalAlign = 'top';
 DivFR.style.paddingtop = '2px';

 if (isFooter) {
 var tblfr = tbl.cloneNode(true);
 tblfr.removeChild(tblfr.getElementsByTagName('tbody')[0]);
 var tblBody = document.createElement('tbody');
 tblfr.style.width = '100%';
 tblfr.cellSpacing = "0";
 tblfr.border = "0px";
  tblfr.rules = "none";
 //*****In the case of Footer Row *******
 tblBody.appendChild(tbl.rows[tbl.rows.length - 1]);
 tblfr.appendChild(tblBody);
 DivFR.appendChild(tblfr);
 }
 //****Copy Header in divHeaderRow****
 DivHR.appendChild(tbl.cloneNode(true));
 }
}



function OnScrollDiv(Scrollablediv) {
  document.getElementById('DivHeaderRow').scrollLeft = Scrollablediv.scrollLeft;
document.getElementById('DivFooterRow').scrollLeft = Scrollablediv.scrollLeft;
}


</script>

</head>
<body>
    <!-- Navigation -->
    <form id="Form1" runat="server">
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark fixed-top">
            <div class="container">
                <div class="dropdown">
                <a class="navbar-brand" href="../">
                    <img src="img/logo.png" width="200" alt="">
                </a>
                <div class="dropdown-content">
                    <asp:Panel ID="Panel1" runat="server"></asp:Panel>
                </div>
            </div>
                <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarResponsive" aria-controls="navbarResponsive" aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarResponsive">
                    <ul class="navbar-nav ml-auto">
                        <li class="nav-item active">
                            <asp:LinkButton ID="PrepTacker_R" class="nav-link" runat="server" href="Default.aspx">Prep Tracker<span class="sr-only">(current)</span></asp:LinkButton>
                        </li>
                        <li class="nav-item">
                            <asp:LinkButton ID="StickerName" class="nav-link" runat="server" href="StickerName.aspx">Sticker Name</asp:LinkButton>
                        </li>
                        <li class="nav-item">
                            <asp:LinkButton ID="Manage_Users" class="manuser-link" runat="server" href="User.aspx">Manage Users</asp:LinkButton>
                        </li>
                        <li class="nav-item">
                            <asp:LinkButton ID="LinkButton2" class="signout-link" OnClick="Logout_Click" runat="server">Sign out</asp:LinkButton>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>

        <!-- Page Content -->
        
        <div class="container containertop">
            <asp:DropDownList ID="ddlCycle" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCycle_SelectedIndexChanged" >
            </asp:DropDownList>
            <asp:DropDownList ID="ddlDC" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDC_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:DropDownList ID="ddlItem" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlItem_SelectedIndexChanged">
                <asp:ListItem Value="3%">WIP</asp:ListItem>
                <asp:ListItem Value="1%">DROP</asp:ListItem>
                <asp:ListItem Value="6%">Meal</asp:ListItem>
            </asp:DropDownList>
            <asp:Label ID="lblSource" runat="server"></asp:Label>
            <asp:CheckBox ID="cbFulfill" runat="server" AutoPostBack="true" OnCheckedChanged="CheckBox1_CheckedChanged" Text="Hide Fulfilled" />
            <asp:Button ID="btnRefresh" runat="server" class="refresh" OnClick="btnRefresh_Click" />
    <div id="DivRoot" align="left">
    <div style="overflow: hidden;" id="DivHeaderRow">
    </div>
    <div style="overflow-y:scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">

            <asp:GridView ID="GridView1" style="font-size:13px" runat="server" AutoGenerateColumns="False" Width="100%" ShowFooter="true" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" OnRowDataBound="GridView1_RowDataBound" OnRowCreated="GridView1_RowCreated" EmptyDataText="Fulfilled for this cycle!">
                            <AlternatingRowStyle BackColor="#FFF0D2" ForeColor="#333333" />
                <Columns>
                    <asp:TemplateField HeaderText="Item" SortExpression="Item">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Item") %>' ID="lblItem"></asp:Label>
                            <asp:Label runat="server" Text='<%# Bind("life") %>' ID="lblLife" Visible="false"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="52px"></HeaderStyle>
                        <ItemStyle Width="52px"></ItemStyle>
                        <FooterStyle Width="52px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Decpt" SortExpression="Decpt">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Decpt") %>' ID="lblDecpt"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="Total" runat="server" Text="Total:"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="417px"></HeaderStyle>
                        <ItemStyle Width="417px"></ItemStyle>
                        <FooterStyle Width="417px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="FriNeeded">
                        <FooterTemplate>
                            <asp:Label ID="lblTotalFriNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Fri") %>' ID="lblFriNeeded"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="FriOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblFriOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalFriRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="FriPer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblFriPer"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalFriPer" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="SatNeeded">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Sat") %>' ID="lblSatNeeded"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSatNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="SatOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblSatOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSatRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="SatPer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblSatPer"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSatPer" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="SunNeeded">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Sun") %>' ID="lblSunNeeded"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSunNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="SunOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblSunOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSunRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="SunPer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblSunPer"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalSunPer" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="MonNeeded">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Mon") %>' ID="lblMonNeeded"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalMonNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="MonOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblMonOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalMonRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="MonPer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblMonPer"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalMonPer" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="TueNeeded">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Tue") %>' ID="lblTueNeeded"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalTueNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="TueOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblTueOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalTueRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="TuePer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblTuePer" Text="-"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalTuePer" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Needed" SortExpression="WedNeeded">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Wed") %>' ID="lblWedNeeded"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalWedNeeded" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="57px"></HeaderStyle><ItemStyle Width="57px"></ItemStyle>
                        <FooterStyle Width="57px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reserved" SortExpression="WedOnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblWedOnHand"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalWedRsv" runat="server" Text="-"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="65px"></HeaderStyle><ItemStyle Width="65px"></ItemStyle>
                        <FooterStyle Width="65px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%" SortExpression="WedPer">
                        <ItemTemplate>
                            <asp:Label runat="server" ID="lblWedPer" Text="-"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalWedPer" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="42px"></HeaderStyle><ItemStyle Width="42px"></ItemStyle>
                        <FooterStyle Width="42px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Demand" SortExpression="Total">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Total") %>' ID="lblTotal"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblTotalDemand" runat="server"></asp:Label>
                        </FooterTemplate>
                        <HeaderStyle Width="60px"></HeaderStyle><ItemStyle Width="60px"></ItemStyle>
                        <FooterStyle Width="60px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="On Hand" SortExpression="OnHand">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("OnHand") %>' ID="lblTotalOnHand"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="75px"></HeaderStyle><ItemStyle Width="75px"></ItemStyle>
                        <FooterStyle Width="75px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Demand" SortExpression="Committed">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("rsved") %>' ID="lblCommitted"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="95px"></HeaderStyle><ItemStyle Width="95px"></ItemStyle>
                        <FooterStyle Width="95px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="On Prod." SortExpression="InProd">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("OnProd") %>' ID="lblInProd"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="95px"></HeaderStyle><ItemStyle Width="95px"></ItemStyle>
                        <FooterStyle Width="95px" />
                    </asp:TemplateField>
                </Columns>

                <EditRowStyle BackColor="#9fed8e" />
                            <FooterStyle BackColor="#FAAC18" ForeColor="White" Font-Bold="True" BorderColor="#DEDFDE" />
                            <HeaderStyle BackColor="#FAAC18" Font-Bold="True" ForeColor="White" />
                            <PagerStyle ForeColor="White" HorizontalAlign="Center" BackColor="#284775" />
                            <RowStyle BackColor="#fcfcfc" ForeColor="#333333" />
                            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                            <SortedAscendingCellStyle BackColor="#E9E7E2" />
                            <SortedAscendingHeaderStyle BackColor="#506C8C" />
                            <SortedDescendingCellStyle BackColor="#FFFDF8" />
                            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                        </asp:GridView>

            </div>
    <div id="DivFooterRow" style="overflow:hidden">
    </div>
</div>

        </div>
        
    </form>
    <!-- Bootstrap core JavaScript -->
    <script src="vendor/bootstrap/js/bootstrap.bundle.min.js"></script>



</body>
</html>
<script runat="server">
    void Logout_Click(Object sender, EventArgs e)
    {
        FormsAuthentication.SignOut();
        FormsAuthentication.RedirectToLoginPage();
    }
</script>
