<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="GalleryDetail.aspx.cs" Inherits="Site.Areas.EntityList.Pages.GalleryDetail" %>
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Adxstudio.Xrm" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Areas.EntityList.Helpers" %>
<%@ Import namespace="DevTrends.MvcDonutCaching" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/EntityList/css/lightbox.css") %>"/>
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/EntityList/css/gallery.css") %>"/>
	<asp:PlaceHolder ID="PackageHead" Visible="False" runat="server">
		<%: Html.PackageLink(Url, Package.UniqueName) %>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="Breadcrumbs" runat="server">
	<asp:PlaceHolder ID="PackageBreadcrumbs" Visible="False" runat="server">
		<ul class="breadcrumb">
			<% foreach (var node in Html.SiteMapPath()) { %>
				<% if (node.Item2 == SiteMapNodeType.Current) { %>
					<li class="active"><%: Package.DisplayName %></li>
				<% } else { %>
					<li>
						<a href="<%: node.Item1.Url %>" title="<%: node.Item1.Title %>"><%: node.Item1.Title %></a>
					</li>
				<% } %>
			<% } %>
		</ul>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PageBreadcrumbs" runat="server">
		<% Html.RenderPartial("Breadcrumbs"); %>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<asp:PlaceHolder ID="PackageHeader" Visible="False" runat="server">
		<div class="page-header">
			<div class="pull-right">
				<% var isAuthenticated = Request.IsAuthenticated; %>
				<% var signInUrl = !isAuthenticated ? Html.Action("SignInUrl", "Layout", new { area = "Portal" }) : null; %>
				<div class="btn-group">
					<% if (isAuthenticated) { %>
					<button type="button" class="btn btn-default" data-toggle="modal" data-target="#LicenseModal"><span class="fa fa-download" aria-hidden="true"></span> <%: Html.SnippetLiteral("Gallery/PackageRepository/Package/Download") ?? ResourceManager.GetString("Download_Button_Label") %></button>
					<% } else { %>
					<a type="button" class="btn btn-default" href="<%: signInUrl %>"><span class="fa fa-sign-in" aria-hidden="true"></span> <%: Html.SnippetLiteral("Gallery/PackageRepository/Package/SignInToDownload") ?? ResourceManager.GetString("Sign_In_To_Download") %></a>
					<% } %>
				</div>
				<div class="btn-group package-installer">
					<a class="btn btn-success" href="<%: Html.PackageInstallUrl(Url, Package.UniqueName) %>"><span class="fa fa-plus-circle" aria-hidden="true"></span> <%: Html.SnippetLiteral("Gallery/PackageRepository/Package/Install") ?? ResourceManager.GetString("Install_Package") %></a>
				</div>
			</div>
			<h1><% if (Package.Icon != null) { %><img class="gallery-icon" src="<%: Package.Icon.Url %>" alt="<%: Package.Icon.Description %>" /><% } %> <%: Package.DisplayName %></h1>
		</div>
		
		<!-- Modal -->
		<div class="modal fade in"	id="LicenseModal" tabindex="-1"	role="dialog" aria-labelledby="LicenseModalLabel">
			<div class="modal-dialog"	role="document">
				<div class="modal-content">
					<div class="modal-header">
						<button	type="button" class="close"	data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
						<h4	class="modal-title"	id="LicenseModalLabel"><%: Html.TextSnippet("Licensing/EULA/ModalTitle", true, "span", null, false, "License Agreement") %></h4>
					</div>
					<div class="modal-body">
					<%:	Html.HtmlSnippet("Licensing/EULA/Body")	%>
					</div>
					<div class="modal-footer">
						<button	type="button" class="btn btn-default" data-dismiss="modal"><%: Html.TextSnippet("Licensing/EULA/CloseButton", true, "span", null, false, "Decline")  %></button>
						<a type="button" class="btn	btn-primary" href="<%: Package.ContentUrl %>"><%: Html.TextSnippet("Licensing/EULA/AcceptButton", true, "span", null, false, "Accept")  %></a>
					</div>
				</div>
			</div>
		</div>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<asp:PlaceHolder ID="PackageContent" Visible="False" runat="server">
		<div class="row gallery-detail">
			<div class="col-sm-8">
				<div class="description">
					<%= Package.Description %>
				</div>
				<% if (Package.Images.Any()) { %>
					<div class="row">
						<% foreach (var batch in Package.Images.Batch(4)) { %>
							<% foreach (var image in batch) { %>
								<div class="col-sm-3">
									<a class="thumbnail" href="<%: image.Url %>" title="<%: image.Description %>" data-lightbox="gallery-images"><img src="<%: image.Url %>" alt="<%: image.Description %>" /></a>
								</div>
							<% } %>
						<% } %>
					</div>
				<% } %>
				<adx:EntityForm ID="EntityFormControl" runat="server" FormCssClass="crmEntityFormView" PreviousButtonCssClass="btn btn-default" NextButtonCssClass="btn btn-primary" SubmitButtonCssClass="btn btn-primary" ClientIDMode="Static" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
				<% if (Package.Dependencies.Any() || Package.Components.Any() || Package.Versions.Any()) { %>
					<div class="panel-group" id="related">
						<% if (Package.Dependencies.Any()) { %>
							<div class="panel panel-default">
								<div class="panel-heading">
									<h4 class="panel-title">
										<a data-toggle="collapse" data-parent="#related" href="#dependencies">
											<span class="badge pull-right"><%: Package.Dependencies.Count() %></span>
											<%: Html.SnippetLiteral("Gallery/PackageRepository/Dependencies/Header") ?? ResourceManager.GetString("Dependencies") %>
										</a>
									</h4>
								</div>
								<div id="dependencies" class="panel-collapse collapse in">
									<div class="panel-body">
										<ul class="list-unstyled">
											<% foreach (var dependency in Package.Dependencies) { %>
												<li class="clearfix">
													<div class="pull-right">
														<span class="label label-info"><%: dependency.Version %></span>
													</div>
													<%: dependency.DisplayName %>
												</li>
											<% } %>
										</ul>
									</div>
								</div>
							</div>
						<% } %>
						<% if (Package.Components.Any()) { %>
							<div class="panel panel-default">
								<div class="panel-heading">
									<h4 class="panel-title">
										<a data-toggle="collapse" data-parent="#related" href="#components">
											<span class="badge pull-right"><%: Package.Components.Count() %></span>
											<%: Html.SnippetLiteral("Gallery/PackageRepository/Components/Header") ?? ResourceManager.GetString("Components") %>
										</a>
									</h4>
								</div>
								<div id="components" class="panel-collapse collapse">
									<div class="panel-body">
										<ul class="list-unstyled">
											<% foreach (var component in Package.Components) { %>
												<li class="clearfix">
													<div class="pull-right">
														<span class="label label-info"><%: component.Version %></span>
													</div>
													<%: component.DisplayName %>
												</li>
											<% } %>
										</ul>
									</div>
								</div>
							</div>
						<% } %>
						<% if (Package.Versions.Any()) { %>
							<div class="panel panel-default">
								<div class="panel-heading">
									<h4 class="panel-title">
										<a data-toggle="collapse" data-parent="#related" href="#versions">
											<%: Html.SnippetLiteral("Gallery/PackageRepository/Versions/Header") ?? ResourceManager.GetString("Update_History") %>
										</a>
									</h4>
								</div>
								<div id="versions" class="panel-collapse collapse">
									<div class="panel-body">
										<ul class="list-unstyled">
											<% foreach (var version in Package.Versions) { %>
												<li>
													<h5 class="clearfix">
														<abbr class="timestamp pull-right" data-format="MMMM dd, yyyy"><%: version.ReleaseDate.ToString("r") %></abbr>
														<span class="label label-info"><%: version.Version %></span>
													</h5>
													<div class="description">
														<%= version.Description %>
													</div>
												</li>
											<% } %>
										</ul>
									</div>
								</div>
							</div>
						<% } %>
					</div>
				<% } %>
			</div>
			<div class="col-sm-4">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">
							<%: Html.SnippetLiteral("Gallery/PackageRepository/Version/Header") ?? ResourceManager.GetString("Current_Version") %>
						</h4>
					</div>
					<div class="panel-body">
						<span class="label label-info"><%: Package.Version %></span>
					</div>
				</div>
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">
							<%: Html.SnippetLiteral("Gallery/PackageRepository/ReleaseDate/Header") ?? ResourceManager.GetString("Release_Date_Label") %>
						</h4>
					</div>
					<div class="panel-body">
						<abbr class="timestamp" data-format="MMMM dd, yyyy"><%: Package.ReleaseDate.ToString("r") %></abbr>
					</div>
				</div>
				<% if (Package.Categories.Any()) { %>
					<div class="panel panel-default">
						<div class="panel-heading">
							<h4 class="panel-title">
								<%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/Header") ?? ResourceManager.GetString("Categories") %>
							</h4>
						</div>
						<div class="panel-body">
							<% foreach (var category in Package.Categories) { %>
								<span class="label label-success"><%: category.Name %></span>
							<% } %>
						</div>
					</div>
				<% } %>
				<% if (!string.IsNullOrWhiteSpace(Package.PublisherName)) { %>
					<div class="panel panel-default">
						<div class="panel-heading">
							<h4 class="panel-title">
								<%: Html.SnippetLiteral("Gallery/PackageRepository/Publisher/Header") ?? ResourceManager.GetString("Publisher") %>
							</h4>
						</div>
						<div class="panel-body">
							<span class="label label-default"><%: Package.PublisherName %></span>
						</div>
					</div>
				<% } %>
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">
							<%: Html.SnippetLiteral("Gallery/PackageRepository/RespositoryPackageURL/Header") ?? ResourceManager.GetString("Package_Install_URL") %>
						</h4>
					</div>
					<div class="panel-body">
						<div>
							<input id="repository-package-url" class="repository-url form-control" type="text" value="<%: Html.PackageUrl(Url, Package.UniqueName) %>" readonly="readonly" />
							<div class="input-group-btn" style="display: none;">
								<a class="btn btn-default zeroclipboard" data-clipboard-target="repository-package-url" title="<%: Html.SnippetLiteral("Gallery/PackageRepository/RespositoryPackageURL/CopyToClipboard") ?? ResourceManager.GetString("Copy_URL_To_Clipboard") %>">
									<span class="fa fa-clipboard" aria-hidden="true"></span>
								</a>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PackageNotFound" Visible="True" runat="server">
		<div class="alert alert-block alert-danger">
			<p><%: Html.TextSnippet("Gallery/PackageRepository/Package/NotFound", defaultValue:  ResourceManager.GetString("Requested_Package_Not_Found"), tagName: "span") %></p>
		</div>
	</asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/EntityList/js/lightbox-2.6.min.js") %>"></script>
	<script src="<%: Url.Content("~/Areas/EntityList/js/ZeroClipboard.min.js") %>"></script>
	<script type="text/javascript">
		ZeroClipboard.config({ moviePath: '<%: Url.Content("~/Areas/EntityList/swf/ZeroClipboard.swf") %>', activeClass: 'active', hoverClass: 'hover' });
	</script>
	<script src="<%: Url.Content("~/Areas/EntityList/js/gallery.js") %>"></script>
</asp:Content>
