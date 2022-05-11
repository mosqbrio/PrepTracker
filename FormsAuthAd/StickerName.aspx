<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StickerName.aspx.cs" Inherits="PrepTracker.StickerName" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />

    <title>Sticker Name</title>

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
DivMC.style.height = (window.innerHeight - 185) + 'px';
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
    <form id="Form1" method="post" runat="server">
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
                        <li class="nav-item">
                            <asp:LinkButton ID="PrepTacker_R" class="nav-link" runat="server" href="Default.aspx">Prep Tracker</asp:LinkButton>
                        </li>
                        <li class="nav-item active">
                            <asp:LinkButton ID="Sticker" class="nav-link" runat="server" href="StickerName.aspx">Sticker Name<span class="sr-only">(current)</span></asp:LinkButton>
                        </li>
                        <li class="nav-item active">
                            <asp:LinkButton ID="Manage_Users" class="manuser-active" runat="server" href="User.aspx">Manage Users</asp:LinkButton>
                        </li>
                        <li class="nav-item">
                            <asp:LinkButton ID="LinkButton2" class="signout-link" OnClick="Logout_Click" runat="server">Sign out</asp:LinkButton>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>

        <!-- Page Content -->
        <div class="container containertop"><br />
            <strong>CYCLE:</strong>
            <asp:DropDownList ID="ddlCycle" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCycle_SelectedIndexChanged" >
            </asp:DropDownList>
            <asp:DropDownList ID="ddlDC" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDC_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:DropDownList ID="ddlItem" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlItem_SelectedIndexChanged">
                <asp:ListItem Value="Item NOT LIKE '3%'">WIP</asp:ListItem>
                <asp:ListItem Value="Item  NOT LIKE '6%'">Bag</asp:ListItem>
                <asp:ListItem Value="LEN(Item) != 4">Meal</asp:ListItem>
            </asp:DropDownList>
            <asp:Label ID="lblSource" runat="server"></asp:Label>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
            <strong>PRODUCTION DATE:</strong>
            <asp:DropDownList ID="ddlProductionDate" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProductionDate_SelectedIndexChanged">
            </asp:DropDownList>
    <div id="DivRoot" align="left">
    <div style="overflow: hidden;" id="DivHeaderRow">
    </div>
    <div style="overflow-y:scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">

            <asp:GridView ID="GridView1" style="font-size:15px" runat="server" AutoGenerateColumns="False" Width="100%" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" EmptyDataText="Fulfilled for this cycle!" OnRowDataBound="GridView1_RowDataBound">
                            <AlternatingRowStyle BackColor="#FFF0D2" ForeColor="#333333" />
                <Columns>
                    <asp:TemplateField HeaderText="Item"  SortExpression="Item">
                        <ItemTemplate>
                            <asp:HyperLink runat="server" Text='<%# Bind("Item") %>' NavigateUrl='<%# Eval("URL") %>' ID="lblItem"></asp:HyperLink>
                        </ItemTemplate>
                        <HeaderStyle Width="55px"></HeaderStyle>
                        <ItemStyle Width="55px"></ItemStyle>
                        
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Decpt" SortExpression="Decpt">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Decpt") %>' ID="lblDecpt"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="600px"></HeaderStyle>
                        <ItemStyle Width="600px"></ItemStyle>
                        
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Sticker Name" SortExpression="Sticker">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("StickerName") %>' ID="lblSticker"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="400px"></HeaderStyle><ItemStyle Width="400px"></ItemStyle>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Allergens" SortExpression="Allergen">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Bind("Allergen") %>' ID="lblAllergen"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="500px"></HeaderStyle><ItemStyle Width="500px"></ItemStyle>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Expiration" SortExpression="Expiration">
                        <ItemTemplate>
                            <asp:Label runat="server" Text='<%# Eval("Exp") %>' ID="lblExp"></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Width="150px"></HeaderStyle><ItemStyle Width="150px"></ItemStyle>
                    </asp:TemplateField>
                   <%-- <asp:TemplateField HeaderText="URL" SortExpression="URL">
                        <ItemTemplate>
                            <asp:HyperLink  runat="server" Text='<%# Eval("URL") %>' NavigateUrl='<%# Eval("URL","http://{0}") %>' ID="lblURL"></asp:HyperLink>
                        </ItemTemplate>
                        <HeaderStyle Width="150px"></HeaderStyle><ItemStyle Width="150px"></ItemStyle>
                    </asp:TemplateField>--%>
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