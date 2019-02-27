<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="User.aspx.cs" Inherits="PrepTracker.User" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />

    <title>Manage User</title>

    <!-- Bootstrap core CSS -->
    <link href="vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/jquery-ui.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="css/styles.css?v=2.0" />
    <script src="vendor/jquery/jquery-1.12.4.js"></script>
    <script src="vendor/jquery/jquery-ui.js"></script>
    <script type="text/javascript">
       $(document).ready(function(){
           
           $("#username").autocomplete({
                source: function(request, response) {  
                    $.ajax({  
                        type: "POST",  
                        contentType: "application/json; charset=utf-8",  
                        url: "User.aspx/GetItems",  
                        data: "{'item':'" + $('#username').val() + "'}",  
                        dataType: "json",  
                        success: function(data) {  
                            response(data.d);
                        },  
                        error: function(result) {  
                            alert("No Match");  
                        }  
                    });  
               },
               select: function (event, ui) {
                   var fulluser = ui.item.label;
                   var user = fulluser.substr(fulluser.indexOf(" - ") + 3);
                   $("#username").val(user);
                    return false
               },
                change: function (event, ui) {
                    if (!ui.item) {
                        $("#username").val("");
                    }
                }
            });
        });
        
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
                    <asp:Panel ID="Panel2" runat="server"></asp:Panel>
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
                        <li class="nav-item">
                            <asp:LinkButton ID="StickerName" class="nav-link" runat="server" href="StickerName.aspx">Sticker Name</asp:LinkButton>
                        </li>
                        <li class="nav-item active">
                            <asp:LinkButton ID="Manage_Users" class="manuser-active" runat="server" href="User.aspx">Manage Users<span class="sr-only">(current)</span></asp:LinkButton>
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
            <br />
            <table align="center">
                <tr><td><table width="100%"><tr>
                    <td><cc1:ToolkitScriptManager runat="server">
            </cc1:ToolkitScriptManager>
            <asp:Button ID="btnShow" runat="server" Text="Add User" class="adduser" /></td>
                    <td align="right"><asp:DropDownList ID="dddlDC" class="ddl" runat="server" AutoPostBack="true" OnSelectedIndexChanged="IndexChanged"></asp:DropDownList><asp:Label ID="EditIndex" runat="server" Text="" Visible="false"></asp:Label></td>
                </tr></table>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" OnRowEditing="GridView1_RowEditing" OnRowUpdating="GridView1_RowUpdating" OnRowCancelingEdit="GridView1_RowCancelingEdit" OnSorting="GridView_Sorting" AllowSorting="True" OnRowDeleting="GridView1_RowDeleting"  OnRowDataBound="GridView1_RowDataBound">
                <AlternatingRowStyle BackColor="#FFF0D2" ForeColor="#333333" />
                <Columns>
                    <asp:TemplateField ShowHeader="False">
                        <EditItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" ForeColor="#333333"></asp:LinkButton>
                            &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" ForeColor="#333333"></asp:LinkButton>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" ForeColor="#333333"></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Account" SortExpression="Account">
                        <EditItemTemplate>
                            <asp:Label ID="lblAccount" runat="server" Text='<%# Eval("Account") %>'></asp:Label>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="lblAccount" runat="server" Text='<%# Bind("Account") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="250px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Permission" SortExpression="Permission">
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddleditPermission" runat="server" Text='<%# Bind("Permission") %>' ><asp:ListItem>Admin</asp:ListItem><asp:ListItem>Basic</asp:ListItem></asp:DropDownList>
                            <asp:Label ID="lblPermission" runat="server" Text='<%# Bind("Permission") %>' Visible="false"></asp:Label>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="lblPermission" runat="server" Text='<%# Bind("Permission") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="150px" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Office" SortExpression="Office">
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddleditOffice" runat="server" ></asp:DropDownList>
                            <asp:Label ID="lblOffice" runat="server" Text='<%# Bind("OfficeID") %>' Visible="false"></asp:Label>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("Office") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="100px" />
                    </asp:TemplateField>

                    <asp:BoundField DataField="LastLogin" HeaderText="LastLogin" ItemStyle-Width="300px" ReadOnly="True" SortExpression="LastLogin" >
<ItemStyle Width="250px"></ItemStyle>
                    </asp:BoundField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton3" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" ForeColor="#333333" OnClientClick="return confirm('Are you sure to delete this user?')"></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EditRowStyle BackColor="#9fed8e" />
                <FooterStyle BackColor="#5D7B9D" ForeColor="White" Font-Bold="True" />
                <HeaderStyle BackColor="#FAAC18" Font-Bold="True" ForeColor="White" />
                <PagerStyle ForeColor="White" HorizontalAlign="Center" BackColor="#284775" />
                <RowStyle BackColor="#fcfcfc" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
            </asp:GridView>
                    </td>
                </tr>
            </table>
        </div>


        <!-- ModalPopupExtender -->
        <cc1:ModalPopupExtender ID="mp1" runat="server" PopupControlID="Panel1" TargetControlID="btnShow"
            CancelControlID="btnClose" BackgroundCssClass="modalBackground">
        </cc1:ModalPopupExtender>
        <asp:Panel ID="Panel1" runat="server" CssClass="AddUsermodalPopup" align="center" Style="display: none">
            <div align="center"><br />
                <asp:Label ID="Label1" runat="server" Font-Size="30px" ForeColor="#5B5B5B"><b>Add User</b></asp:Label><br>
                <table>
                    <tr height="50px">
                        <td align="right" width="100px">Username:</td>
                        <td>
                            <asp:TextBox ID="username" Width="200px" onFocus="this.select()" runat="server"></asp:TextBox><asp:Label ID="msguser" runat="server" ForeColor="#FF3300" Text=""></asp:Label></td>
                    </tr>
                    <tr height="50px">
                        <td align="right">Permission:</td>
                        <td>
                            <asp:DropDownList ID="ddlPermission" Width="200px" runat="server"><asp:ListItem>Admin</asp:ListItem><asp:ListItem>Basic</asp:ListItem></asp:DropDownList>
                            <asp:Label ID="msgpermission" ForeColor="#FF3300" runat="server" Text=""></asp:Label></td>
                    </tr>
                    <tr height="50px">
                        <td align="right">Office:</td>
                        <td>
                            <asp:DropDownList ID="ddlDC" Width="200px" runat="server">
                            </asp:DropDownList>
                            <asp:Label ID="msgoffice" ForeColor="#FF3300" runat="server" Text=""></asp:Label></td>
                    </tr>
                </table>
                <br />
            </div>
            <asp:Button ID="Button2" Class="add" runat="server" Text="Add User" OnClick="Add_Click" /><asp:Button ID="btnClose" Class="cancel" runat="server" Text="Close" />
        </asp:Panel>
        <!-- ModalPopupExtender -->
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