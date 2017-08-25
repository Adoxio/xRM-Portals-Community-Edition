<%@ Page Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="Gallery.aspx.cs" Inherits="Site.Areas.EntityList.Pages.Gallery" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ OutputCache CacheProfile="User" %>
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Site.Areas.EntityList.Helpers" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/EntityList/css/lightbox.css") %>"/>
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/EntityList/css/gallery.css") %>"/>
	<%: Html.PackageRepositoryLink(Url) %>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageHeader" runat="server">
	<div class="page-header">
		<div class="pull-right package-installer">
			<a class="btn btn-success" href="<%: Html.PackageRepositoryInstallUrl(Url) %>"><span class="fa fa-plus-circle" aria-hidden="true"></span> <%: Html.SnippetLiteral("Gallery/PackageRepository/Install") ?? ResourceManager.GetString("Install_Gallery") %></a>
		</div>
		<h1><%: Html.TextAttribute("adx_name") %></h1>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<%: Html.HtmlAttribute("adx_copy", cssClass: "page-copy") %>
	
	<adx:EntityList ID="EntityListControl" IsGallery="True" ListCssClass="table table-striped" DefaultEmptyListText="<%$ ResourceManager:No_Items_To_Display %>" ClientIDMode="Static" runat="server" LanguageCode="<%$ SiteSetting: Language Code, 0 %>" PortalName="<%$ SiteSetting: Language Code %>" />
	
	<script id="gallery-template" type="text/x-handlebars-template">
		<div class="row gallery">
			<div class="col-sm-4">
				<div class="content-panel panel panel-default">
					<div class="list-group gallery-category-list">
						<a class="list-group-item" data-gallery-nav="category" data-gallery-nav-value=""><%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/All") ?? ResourceManager.GetString("All") %></a>
						{{#if HasFeatured}}
							<a class="list-group-item {{#iffeatured ActiveCategory}}active{{/iffeatured}}" data-gallery-nav="category" data-gallery-nav-value="Featured"><%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/Featured") ?? ResourceManager.GetString("Featured") %></a>
						{{/if}}
					</div>
				</div>
				{{#if NonFeaturedCategories}}
					<div class="content-panel panel panel-default">
						<div class="panel-heading">
							<h4><%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/Header") ?? ResourceManager.GetString("Categories") %></h4>
						</div>
						<div class="list-group gallery-category-list">
							{{#each NonFeaturedCategories}}
								<a class="list-group-item {{#ifeq this ../ActiveCategory}}active{{/ifeq}}" data-gallery-nav="category" data-gallery-nav-value="{{this}}" title="{{this}}">{{this}}</a>
							{{/each}}
						</div>
					</div>
				{{/if}}
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<h4><%: Html.SnippetLiteral("Gallery/PackageRepository/RespositoryURL/Header") ?? ResourceManager.GetString("Gallery_Install_URL") %></h4>
					</div>
					<div class="panel-body">
						<div>
							<input id="repository-url" class="repository-url form-control" type="text" value="<%: Html.PackageRepositoryUrl(Url) %>" readonly="readonly" />
							<div class="input-group-btn" style="display: none;">
								<a class="btn btn-default zeroclipboard" data-clipboard-target="repository-url" title="<%: Html.SnippetLiteral("Gallery/PackageRepository/RespositoryURL/CopyToClipboard") ?? ResourceManager.GetString("Copy_URL_To_Clipboard") %>">
									<span class="fa fa-clipboard" aria-hidden="true"></span>
								</a>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div class="col-sm-8">
				{{#if VisiblePackages}}
					<ul class="list-unstyled gallery-items">
						{{#with FeaturedPackage}}
							<li>
								<div class="media item featured">
									{{#if Icon}}
										<a class="media-left" href="{{URL}}" title="{{ Icon.Description }}">
											<img class="media-object gallery-icon" src="{{ Icon.URL }}" alt="{{ Icon.Description }}" />
										</a>
									{{else}}
										<a class="media-left" href="{{URL}}">
											<div class="media-object gallery-icon placeholder"></div>
										</a>
									{{/if}}
									<div class="media-body">
										<div class="gallery-item-heading">
											<div class="gallery-item-title">
												<h3 class="name"><a href="{{URL}}" title="{{DisplayName}}">{{DisplayName}}</a></h3>
											</div>
											<div class="metadata">
												<span class="label label-info">{{Version}}</span>
												{{#if IsFeatured}}
													<span class="label label-warning"><%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/Featured") ?? ResourceManager.GetString("Featured") %></span>
												{{/if}}
											</div>
										</div>
										<div class="summary">
											{{Summary}}
										</div>
										{{#if Images}}
											<div class="row">
												{{#take3 Images}}
													<div class="col-sm-4">
														<a class="thumbnail" href="{{URL}}" title="{{Description}}" data-lightbox="{{../URI}}"><img src="{{URL}}" alt="{{Description}}" /></a>
													</div>
												{{/take3}}
											</div>
										{{/if}}
										<div class="tags">
											<span class="label label-default">{{PublisherName}}</span>
											{{#each NonFeaturedCategories}}
												<span class="label label-success">{{this}}</span>
											{{/each}}
										</div>
									</div>
								</div>
							</li>
						{{/with}}
						{{#each NonFeaturedPackages}}
							<li>
								<div class="media item">
									{{#if Icon}}
										<a class="media-left" href="{{URL}}" title="{{ Icon.Description }}">
											<img class="media-object gallery-icon" src="{{ Icon.URL }}" alt="{{ Icon.Description }}" />
										</a>
									{{else}}
										<a class="media-left" href="{{URL}}">
											<div class="media-object gallery-icon placeholder"></div>
										</a>
									{{/if}}
									<div class="media-body">
										<div class="gallery-item-heading">
											<div class="gallery-item-title">
												<h3 class="name"><a href="{{URL}}" title="{{DisplayName}}">{{DisplayName}}</a></h3>
											</div>
											<div class="metadata">
												<span class="label label-info">{{Version}}</span>
												{{#if IsFeatured}}
													<span class="label label-warning"><%: Html.SnippetLiteral("Gallery/PackageRepository/Categories/Featured") ?? ResourceManager.GetString("Featured") %></span>
												{{/if}}
											</div>
										</div>
										<div class="summary">
											{{Summary}}
										</div>
										{{#if Images}}
											<div class="row">
												{{#take4 Images}}
													<div class="col-sm-3">
														<a class="thumbnail" href="{{URL}}" title="{{Description}}" data-lightbox="{{../URI}}"><img src="{{URL}}" alt="{{Description}}" /></a>
													</div>
												{{/take4}}
											</div>
										{{/if}}
										<div class="tags">
											<span class="label label-default">{{PublisherName}}</span>
											{{#each NonFeaturedCategories}}
												<span class="label label-success">{{this}}</span>
											{{/each}}
										</div>
									</div>
								</div>
							</li>
						{{/each}}
					</ul>
				{{else}}
					<div class="alert alert-block alert-info">
						<p><%: Html.HtmlSnippet("Gallery/PackageRepository/NoPackages", defaultValue: ResourceManager.GetString("No_Items_Were_Found_For_Current_View")) %></p>
					</div>
				{{/if}}
			</div>
		</div>
	</script>
	
	<script id="gallery-error-template" type="text/x-handlebars-template">
		<div class="gallery">
			<div class="alert alert-block alert-danger">
				<p><%: Html.HtmlSnippet("Gallery/PackageRepository/LoadError", defaultValue: ResourceManager.GetString("There_Was_Error_Loading_Data_For_This_Gallery")) %></p>
			</div>
		</div>
	</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="Scripts" runat="server">
	<script src="<%: Url.Content("~/Areas/EntityList/js/lightbox-2.6.min.js") %>"></script>
	<script src="<%: Url.Content("~/Areas/EntityList/js/ZeroClipboard.min.js") %>"></script>
	<script type="text/javascript">
		ZeroClipboard.config({ moviePath: '<%: Url.Content("~/Areas/EntityList/swf/ZeroClipboard.swf") %>', activeClass: 'active', hoverClass: 'hover' });
	</script>
	<script src="<%: Url.Content("~/Areas/EntityList/js/gallery.js") %>"></script>
</asp:Content>
