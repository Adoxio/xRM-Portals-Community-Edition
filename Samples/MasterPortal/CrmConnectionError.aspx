<%@ Page Language="C#" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Adxstudio.Xrm.Web" %>
<%@ Import Namespace="Adxstudio.Xrm.Globalization" %>
<% Response.StatusCode = 503; %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
	<meta http-equiv="content-type" content="text/html; charset=UTF-8" />
	<title><%: Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Page_500_Crm_Connection_Error") %></title>
	<%--This loads culture-specific styles--%>
	<%
		var bodyFont = string.Empty;
		switch (CultureInfo.CurrentUICulture.LCID)
			{
				case LocaleIds.ChineseTraditional:
					bodyFont = "Microsoft JhengHei UI, ";
					break;
				case LocaleIds.Japanese:
					bodyFont = "Yu Gothic UI, ";
					break;
				case LocaleIds.Korean:
					bodyFont = "Malgun Gothic, ";
					break;
				case LocaleIds.ChineseSimplified:
					bodyFont = "Microsoft YaHei UI, ";
					break;
				case LocaleIds.ChineseHongKong:
					bodyFont = "Microsoft JhengHei UI, ";
					break;
				case LocaleIds.Thai:
					bodyFont = " Leelawadee UI, ";
					break;
				case LocaleIds.Hindi:
					bodyFont = "Nirmala UI, ";
					break;
			}
		bodyFont += "arial, sans-serif";
	%>
	<style type="text/css">
		body {
			background-color: #fff;
			color: #333;
			text-align: center;
			font-family: <%: bodyFont %>;
		}
		div.dialog {
			width: 23em;
			padding: 0 4em;
			margin: 4em auto 0 auto;
			border: 1px solid #ccc;
			border-right-color: #999;
			border-bottom-color: #999;
		}
		h1 {
			font-size: 100%;
			color: #f00;
			line-height: 1.5em;
		}
	</style>

</head>
<body>
	<%
		var error = Context.Error;

		var uri = Context.Request.Url.AbsoluteUri;
		var userAgent = Context.Request.UserAgent;
		var urlReferrer = Context.Request.UrlReferrer;
		var requestType = Context.Request.RequestType;

		var requestInfo = string.Format("AbsoluteUri: {0}, UserAgent: {1}, UrlReferrer: {2}, RequestType: {3}", uri, userAgent, urlReferrer, requestType);

		var guid = WebEventSource.Log.UnhandledException(error, requestInfo);

	%>
	<div class="dialog">
		<h1><%: Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Page_500_Crm_Connection_Error") %></h1>
		<p><%: string.Format(Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Crm_Connection_Error_Page_Text"), guid) %></p>
		<p><%: Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Page_500_We_Have_Been_Notified_Text")  %></p>
	</div>
</body>
</html>