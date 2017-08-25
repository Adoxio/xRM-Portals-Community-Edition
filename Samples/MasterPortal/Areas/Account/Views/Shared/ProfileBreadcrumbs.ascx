<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<ul class="breadcrumb">
    <li>
        <a href="<%: SiteMap.RootNode.Url %>" title="<%: SiteMap.RootNode.Title %>"><%: SiteMap.RootNode.Title %></a>
    </li>
    <li>
        <a href="<%: Html.SiteMarkerUrl("Profile") %>" title="<%: Html.SnippetLiteral("Profile Link Text", ResourceManager.GetString("Profile_Text")) %>"><%: Html.SnippetLiteral("Profile Link Text", ResourceManager.GetString("Profile_Text")) %></a>
    </li>
    <li class="active"><%:ViewBag.PageTitle%></li>
</ul>