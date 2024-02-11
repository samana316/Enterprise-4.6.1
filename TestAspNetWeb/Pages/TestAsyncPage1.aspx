<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="TestAsyncPage1.aspx.cs" Inherits="TestAspNetWeb.Pages.TestAsyncPage1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Text Here: <asp:Label runat="server" ID="Label1"></asp:Label>
        <br />
        <br />
        <asp:Button runat="server" ID="Button1" Text="Do Something" OnClick="Button1_Click" />
        <br />
        <br />
        Text Here: <asp:Label runat="server" ID="Label2"></asp:Label>
        <br />
        <br />
        <asp:Button runat="server" ID="Button2" Text="Do Something Else" OnClick="Button2_Click" />
    </div>
    </form>
</body>
</html>
