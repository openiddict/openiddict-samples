<%@ Page Title="Authorize" Async="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Authorize.aspx.cs" Inherits="Fornax.Server.Connect.Authorize" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="MainContent">
    <h2>Authorization</h2>

    <%-- Flow the request parameters so they can be received by the Accept/Reject actions: --%>
    
    <% foreach (var parameter in Parameters) { %>
        <input type="hidden" name="<%: parameter.Key %>" value="<%: parameter.Value %>" />
    <% } %>

    <p class="lead text-left">
        Do you want to grant <strong><asp:Label ID="ApplicationName" runat="server" /></strong> access to your data?
        (scopes requested: <asp:Label ID="Scope" runat="server" />)
    </p>

    <asp:Button runat="server" OnClick="Accept" Text="Yes" CssClass="btn btn-lg btn-success" />
    <asp:Button runat="server" OnClick="Deny" Text="No" CssClass="btn btn-lg btn-danger" />
</asp:Content>