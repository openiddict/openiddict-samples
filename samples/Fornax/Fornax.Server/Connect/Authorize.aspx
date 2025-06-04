<%@ Page Title="Authorize" Async="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Authorize.aspx.cs" Inherits="Fornax.Server.Connect.Authorize" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="MainContent">
    <h2>Authorization</h2>

    <%-- Flow the request parameters so they can be received by the Accept/Reject actions: --%>
    
    <% foreach (var parameter in string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ?
        from name in Request.Form.AllKeys
        from value in Request.Form.GetValues(name)
        select new KeyValuePair<string, string>(name, value) :
        from name in Request.QueryString.AllKeys
        from value in Request.QueryString.GetValues(name)
        select new KeyValuePair<string, string>(name, value)) { %>
        <input type="hidden" name="<%: parameter.Key %>" value="<%: parameter.Value %>" />
    <% } %>

    <p class="lead text-left">
        Do you want to grant <strong><asp:Label ID="ApplicationName" runat="server" /></strong> access to your data?
        (scopes requested: <asp:Label ID="Scope" runat="server" />)
    </p>

    <asp:Button runat="server" OnClick="Accept" Text="Yes" CssClass="btn btn-lg btn-success" />
    <asp:Button runat="server" OnClick="Deny" Text="No" CssClass="btn btn-lg btn-danger" />
</asp:Content>