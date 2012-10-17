<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GittR.Web.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <h1>GittR</h1>
    <p>Simple issue management for GitHub</p>

    <asp:LoginView runat="server" ViewStateMode="Disabled">
        <AnonymousTemplate>
            <a href="<%: GetRouteUrl("Login", null) %>">Login with GitHub</a>
        </AnonymousTemplate>
        <LoggedInTemplate>
            <p>
                Welcome back <asp:LoginName runat="server" CssClass="username" />!
                <a href="<%: GetRouteUrl("Logout", null) %>">logout</a>
            </p>
        </LoggedInTemplate>
    </asp:LoginView>

    
</body>
</html>