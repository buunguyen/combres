<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormSample._Default" %>
<%@ Import Namespace="Combres" %>
<%@ Register Assembly="Combres" Namespace="Combres" TagPrefix="cr" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Client-side Resource Combine</title>
    <%-- Generate links to resource sets --%>
    <%= WebExtensions.CombresLink("siteCss") %>
    <%= WebExtensions.CombresLink("siteJs") %>
    <%= WebExtensions.CombresLink("dotLessCss", new { media = "screen" }) %>
    <cr:Include ID="cr" runat="server" SetName="siteJs" HtmlAttributes="a|b|c|d" />

    <%-- Generate JavaScript variables pointing to URLs of resource sets --%>
    <%= WebExtensions.EnableClientUrls() %>
</head>
<body>
    <div id="txt">If you see this message, Combres does NOT work.  Please check Combres documentation to make sure the configuration is done properly.</div>

    <script language="javascript">
        $(function () {
            $("#txt").text("If you see this message, Combres works!  View source and browse to Combres URLs if you want to be sure.");
        })
    </script>
</body>
</html>
