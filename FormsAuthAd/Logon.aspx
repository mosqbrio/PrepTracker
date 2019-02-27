<%@ Page language="c#" AutoEventWireup="true" CodeBehind="Logon.aspx.cs" Inherits="PrepTracker.Logon"%>
<%@ Import Namespace="PrepTracker" %>
<% @Import Namespace="System.Data.SqlClient" %>
<html>
  <head>
        
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
        <meta name="description" content="">
        <meta name="author" content="">

        <title>PrepTracker</title>

    <!-- Bootstrap core CSS -->
    <link href="vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/jquery-ui.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="css/styles.css?v=2.0" />
    <script src="vendor/jquery/jquery-1.12.4.js"></script>
    <script src="vendor/jquery/jquery-ui.js"></script>
</head>
<body class="login-bg">
    <!-- Navigation -->
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
        </div>
    </nav>

    <!-- Page Content -->
    <div class="container containertop">
        <table style="width: 100%; height: 90%">
            <tr>
                <td align="center">
                    <div style="background: rgba(232, 232, 232, 0.88); height: 420px; width: 500px; border-radius: 10px;">
                        <br>
                        <br>
                        <br>
                        <asp:Label ID="Label1" runat="server" Font-Size="30px" ForeColor="#5B5B5B"><b>Sign In</b></asp:Label><br>
                        <br>
                        <form id="Login" method="post" runat="server">
                            <table>
                                <tr height="50px">
                                    <td align="right">
                                        <asp:Label ID="Label2" runat="server" Font-Size="20px" ForeColor="#5B5B5B"><b>Username:</b></asp:Label></td>
                                    <td width="10px"></td>
                                    <td>
                                        <asp:TextBox ID="txtUsername" runat="server" CssClass="usrlogin" Font-Size="22px"></asp:TextBox></td>
                                </tr>
                                <tr height="50px">
                                    <td align="right">
                                        <asp:Label ID="Label5" runat="server" Font-Size="20px" ForeColor="#5B5B5B"><b>Password:</b></asp:Label></td>
                                    <td width="10px"></td>
                                    <td>
                                        <asp:TextBox ID="txtPassword" runat="server" CssClass="usrlogin" TextMode="Password" Font-Size="22px"></asp:TextBox></td>
                                </tr>
                                <tr height="50px">
                                    <td align="right">
                                        <asp:Label ID="Label10" runat="server" ForeColor="#5B5B5B" Font-Size="20px"><b>Domain:</b></asp:Label></td>
                                    <td width="10px"></td>
                                    <td><b>
                                        <asp:Label ID="txtDomain" runat="server" ReadOnly="True" Font-Size="20px" ForeColor="#5B5B5B">SUNBASKET</asp:Label></b></td>
                                </tr>
                            </table>
                            <asp:Button ID="btnLogin" runat="server" Text="Sign In" Class="login" OnClick="Login_Click" Font-Size="25px"></asp:Button><br>
                            <asp:Label ID="errorLabel" runat="server" ForeColor="#FF3300" Font-Size="18px"></asp:Label><br>
                            <asp:CheckBox ID="chkPersist" runat="server" Text="Persist Cookie" Visible="False" />
                        </form>
                    </div>

                </td>
            </tr>
        </table>

    </div>
    <script src="vendor/bootstrap/js/bootstrap.bundle.min.js"></script>
</body>
</html>
<script runat="server">
    void Login_Click(Object sender, EventArgs e)
    {
        String adPath = "LDAP://sunbasket.local"; //Fully-qualified Domain Name
        LdapAuthentication adAuth = new LdapAuthentication(adPath);
        try
        {
            String auth = adAuth.IsAuthenticated(txtDomain.Text, txtUsername.Text, txtPassword.Text);
            if ("true" == auth)
            {
                String user = "";
                int length = txtUsername.Text.Length;
                if (length > 20)
                {
                    user = txtUsername.Text.Substring(0, 20);
                }
                else
                {
                    user = txtUsername.Text;
                }

                String groups = adAuth.GetGroups();

                //Create the ticket, and add the groups.
                bool isCookiePersistent = chkPersist.Checked;
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, user,
          DateTime.Now, DateTime.Now.AddMinutes(480), isCookiePersistent, txtPassword.Text);

                //Encrypt the ticket.
                String encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                //Create a cookie, and then add the encrypted ticket to the cookie as data.
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                if (true == isCookiePersistent)
                    authCookie.Expires = authTicket.Expiration;

                //Add the cookie to the outgoing cookies collection.
                Response.Cookies.Add(authCookie);

                //You can redirect now.
                Response.Redirect(FormsAuthentication.GetRedirectUrl(txtUsername.Text, false));


            }
            else if ("HR" == auth)
            {
                errorLabel.Text = "Please contact IT SUPPORT for permission.";
            }
            else if ("false" == auth)
            {
                errorLabel.Text = "Authentication did not succeed. Check user name and password.";
            }
        }
        catch (Exception ex)
        {
            errorLabel.Text = "Error authenticating. " + ex.Message;
        }
    }
</script>