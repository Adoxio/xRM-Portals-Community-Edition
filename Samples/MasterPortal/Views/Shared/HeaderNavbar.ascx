<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import namespace="DevTrends.MvcDonutCaching" %>

<% var viewSupportsDonuts = ((bool?)ViewBag.ViewSupportsDonuts).GetValueOrDefault(false); %>
<% var relatedWebsites = Html.RelatedWebsites(linkTitleSiteSettingName:"Site Name"); %>
<% var searchEnabled = Html.BooleanSetting("Search/Enabled").GetValueOrDefault(true); %>
<% var searchUrl = searchEnabled ? Html.SiteMarkerUrl("Search") : null; %>
<% var searchFilterOptions = searchEnabled ? Html.SearchFilterOptions().ToArray() : Enumerable.Empty<KeyValuePair<string, string>>().ToArray(); %>
<% var searchFilterDefaultText = searchEnabled ? Html.SnippetLiteral("Default Search Filter Text", ResourceManager.GetString("All")) : null; %>
<% var searchFilterLabel = searchEnabled ? Html.SnippetLiteral("Header/Search/Filter/Label", ResourceManager.GetString("Search_Filter")) : null; %>
<% var searchLabel = searchEnabled ? Html.SnippetLiteral("Header/Search/Label", ResourceManager.GetString("Search_DefaultText")) : null; %>
<% var searchToolTip = searchEnabled ? Html.SnippetLiteral("Header/Search/ToolTip", ResourceManager.GetString("Search_DefaultText")) : null; %>
<% var shoppingCartUrl = Html.SiteMarkerUrl("Shopping Cart"); %>
<% var shoppingCartEnabled = string.IsNullOrEmpty(shoppingCartUrl); %>
<% var shoppingCartServiceUrl = shoppingCartEnabled ? Url.Action("Status", "ShoppingCart", new {area = "Commerce", __portalScopeId__ = Html.Website().EntityReference.Id}) : null; %>
<% var shoppingCartLinkText = shoppingCartEnabled ? Html.SnippetLiteral("Shopping Cart Status Link Text", ResourceManager.GetString("Cart")) : null; %>
<% var isAuthenticated = Request.IsAuthenticated; %>
<% var userName = isAuthenticated ? Html.AttributeLiteral(Html.PortalUser(), "fullname") : null; %>
<% var profileNavEnabled = isAuthenticated && Html.BooleanSetting("Header/ShowAllProfileNavigationLinks").GetValueOrDefault(true); %>
<% var profileNavigation = profileNavEnabled ? Html.WebLinkSet("Profile Navigation") : null; %>
<% var profileNavigationListItems = profileNavEnabled && profileNavigation != null ? profileNavigation.WebLinks.Select(e => Html.WebLinkListItem(e, false, false, maximumWebLinkChildDepth: 1)).ToArray() : Enumerable.Empty<IHtmlString>().ToArray(); %>
<% var profileUrl = profileNavEnabled ? null : Html.SiteMarkerUrl("Profile"); %>
<% var profileLinkText = profileNavEnabled ? null : Html.SnippetLiteral("Profile Link Text", ResourceManager.GetString("Profile")); %>
<% var signInUrl = !isAuthenticated ? Html.Action("SignInUrl", "Layout", new { area = "Portal" }, viewSupportsDonuts) : null; %>
<% var signInEnabled = !isAuthenticated && !string.IsNullOrWhiteSpace(Url.SignInUrl()); %>
<% var signInLabel = !isAuthenticated ? Html.SnippetLiteral("links/login", ResourceManager.GetString("Sign_In")) : null; %>
<% var signOutUrl = isAuthenticated ? Html.Action("SignOutUrl", "Layout", new { area = "Portal" }, viewSupportsDonuts) : null; %>
<% var signOutLabel = isAuthenticated ? Html.SnippetLiteral("links/logout", ResourceManager.GetString("Sign_Out")) : null; %>
<% var registrationEnabled = !isAuthenticated && Url.RegistrationEnabled(); %>
<% var registerUrl = registrationEnabled ? Html.Action("RegisterUrl", "Layout", new { area = "Portal" }, viewSupportsDonuts) : null; %>
<% var registerLabel = registrationEnabled ? Html.SnippetLiteral("links/register", ResourceManager.GetString("Register_DefaultText")) : null; %>

<div class="masthead hidden-xs" role="banner">
	<div class="container">
		<div class="toolbar">
			<% var headerNavigation = Html.WebLinkSet("Header Navigation"); %>
			<% if (headerNavigation != null) { %>
				<div class="toolbar-row">
					<div class="toolbar-item toolbar-text text-muted">
						<%: Html.TextAttribute(headerNavigation.Title) %>
					</div>
					<%: Html.WebLinksDropdowns(headerNavigation, "toolbar-item", "nav nav-pills", dropdownMenuCssClass: "pull-right") %>
				</div>
			<% } %>
			<div class="toolbar-row">
				<% if (searchEnabled) { %>
					<div class="toolbar-item toolbar-search">
						<form method="GET" action="<%: searchUrl %>" role="search">
							<label for="q" class="sr-only"><%: searchLabel %></label>
							<div class="input-group">
								<% if (searchFilterOptions.Any()) { %>
									<div class="btn-group btn-select input-group-btn" data-target="#filter" data-focus="#q">
										<button id="search-filter" type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
											<span class="selected"><%: searchFilterDefaultText %></span>
											<span class="caret"></span>
										</button>
										<ul class="dropdown-menu" role="menu" aria-labelledby="search-filter">
                                              <li>
                                                     <a href="#" title="<%: searchFilterDefaultText %>" data-value=""><%: searchFilterDefaultText %></a>
                                              </li>
                                              <% foreach (var option in searchFilterOptions) { %>
                                                     <li>
                                                           <a href="#" title="<%: option.Key %>" data-value="<%: option.Value %>"><%: option.Key %></a>
                                                     </li>
                                              <% } %>
                                       </ul>
									</div>
									<label for="filter" class="sr-only"><%: searchFilterLabel %></label>
									<select id="filter" name="filter" class="btn-select" aria-hidden="true" data-query="filter">
										<option value="" selected="selected"><%: searchFilterDefaultText %></option>
										<% foreach (var option in searchFilterOptions) { %>
											<option value="<%: option.Value %>"><%: option.Key %></option>
										<% } %>
									</select>
								<% } %>
								<input type="text" class="form-control" id="q" name="q" placeholder="<%: searchLabel %>" title="<%: searchLabel %>" data-query="q">
								<div class="input-group-btn">
									<button type="submit" class="btn btn-default" title="<%: searchToolTip %>"><span class="fa fa-search" aria-hidden="true"></span></button>
								</div>
							</div>
						</form>
					</div>
				<% } %>
				<div class="toolbar-item">
					<div class="btn-toolbar" role="toolbar">
						<% if (relatedWebsites.Any) { %>
							<div class="btn-group">
								<a href="#" class="btn btn-default dropdown-toggle" data-toggle="dropdown" title="<%: relatedWebsites.Current.Title %>">
									<span class="fa fa-globe" aria-hidden="true"></span> <%: relatedWebsites.Current.Title %> <span class="caret"></span>
								</a>
								<ul class="dropdown-menu" role="menu">
									<% foreach (var relatedWebsiteLink in relatedWebsites.Links) { %>
										<li><a href="<%: relatedWebsiteLink.Url %>" title="<%: relatedWebsiteLink.Title %>"><%: relatedWebsiteLink.Title %></a></li>
									<% } %>
								</ul>
							</div>
						<% } %>
						<% if (shoppingCartEnabled) { %>
							<div class="btn-group shopping-cart-status" data-href="<%: shoppingCartServiceUrl %>">
								<a class="btn btn-default" href="<%: shoppingCartUrl %>" title="<%: shoppingCartLinkText %>">
									<span class="fa fa-shopping-cart" aria-hidden="true"></span>
									<%: shoppingCartLinkText %>
									<span class="count">(<span class="value"></span>)</span>
								</a>
							</div>
						<% } %>
						<% if (isAuthenticated) { %>
							<div class="btn-group">
								<a href="#" class="btn btn-default dropdown-toggle" data-toggle="dropdown" role="button">
									<span class="fa fa-user" aria-hidden="true"></span>
									<span class="username"><%: userName %></span>
									<span class="caret"></span>
								</a>
								<ul class="dropdown-menu pull-right" role="menu" id="profile-dropdown">
									<% if (profileNavEnabled) { %>
										<% if (profileNavigation != null) { %>
											<% foreach (var item in profileNavigationListItems) { %>
												<%: item %>
											<% } %>
										<% } %>
									<% } else { %>
										<li><a href="<%: profileUrl %>" title="<%: profileLinkText %>"><%: profileLinkText %></a></li>
									<% } %>
									<li role="separator" class="divider"></li>
									<li>
										<a href="<%: signOutUrl %>" role="button">
											<span class="fa fa-sign-out" aria-hidden="true"></span>
											<%: signOutLabel %>
										</a>
									</li>
								</ul>
							</div>
						<% } else { %>
							<% if (registrationEnabled) { %>
								<div class="btn-group">
									<a class="btn btn-default" role="button" href="<%: registerUrl %>" title="<%: registerLabel %>">
										<%: registerLabel %>
									</a>
								</div>
							<% } %>
							<% if (signInEnabled) { %>	
								<div class="btn-group">
									<a class="btn btn-primary" role="button" href="<%: signInUrl %>" title="<%: signInLabel %>">
										<span class="fa fa-sign-in" aria-hidden="true"></span>
										<%: signInLabel %>
									</a>
								</div>
							<% } %>
						<% } %>
					</div>
				</div>
			</div>
		</div>
		<%: Html.HtmlSnippet("Header") %>    
	</div>
</div>
<div class="header-navbar navbar navbar-default navbar-static-top" role="navigation">
	<div class="container">
		<div class="navbar-header">
			<button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#header-navbar-collapse">
				<span class="sr-only">Toggle navigation</span>
				<span class="icon-bar"></span>
				<span class="icon-bar"></span>
				<span class="icon-bar"></span>
			</button>
			<div class="navbar-left visible-xs">
				<%: Html.HtmlSnippet("Mobile Header") %>
			</div>
		</div>
		<div id="header-navbar-collapse" class="navbar-collapse collapse">
			<div class="navbar-left hidden-xs">
				<%: Html.HtmlSnippet("Navbar Left") %>
			</div>
			<div class="visible-xs navbar-left">
				<ul class="nav navbar-nav">
					<% if (isAuthenticated) { %>
						<li class="dropdown">
							<a href="#" class="dropdown-toggle" data-toggle="dropdown" title="<%: userName %>">
								<span class="fa fa-user" aria-hidden="true"></span>
								<span class="username"><%: userName %></span>
								<span class="caret"></span>
							</a>
							<ul class="dropdown-menu">
								<% if (profileNavEnabled) { %>
									<% if (profileNavigation != null) { %>
										<% foreach (var item in profileNavigationListItems) { %>
											<%: item %>
										<% } %>
									<% } %>
								<% } else { %>
									<li><a href="<%: profileUrl %>" title="<%: profileLinkText %>"><%: profileLinkText %></a></li>
								<% } %>
								<li role="separator" class="divider"></li>
								<li>
									<a href="<%: signOutUrl %>" title="<%: signOutLabel %>">
										<span class="fa fa-sign-out" aria-hidden="true"></span>
										<%: signOutLabel %>
									</a>
								</li>
							</ul>
						</li>
					<% } else { %>
						<% if (signInEnabled) { %>
							<li>
								<a href="<%: signInUrl %>" title="<%: signInLabel %>">
									<span class="fa fa-sign-in" aria-hidden="true"></span>
									<%: signInLabel %>
								</a>
							</li>
						<% } %>
						<% if (registrationEnabled) { %>
							<li>
								<a href="<%: registerUrl %>" title="<%: registerLabel %>"><%: registerLabel %></a>
							</li>
						<% } %>
					<% } %>
					<% if (relatedWebsites.Any) { %>
						<li class="dropdown">
							<a href="#" class="dropdown-toggle" data-toggle="dropdown" title="<%: relatedWebsites.Current.Title %>">
								<span class="fa fa-globe" aria-hidden="true"></span> <%: relatedWebsites.Current.Title %> <span class="caret"></span>
							</a>
							<ul class="dropdown-menu" role="menu">
								<% foreach (var relatedWebsiteLink in relatedWebsites.Links) { %>
									<li><a href="<%: relatedWebsiteLink.Url %>" title="<%: relatedWebsiteLink.Title %>"><%: relatedWebsiteLink.Title %></a></li>
								<% } %>
							</ul>
						</li>
					<% } %>
					<% if (shoppingCartEnabled) { %>
						<li class="shopping-cart-status" data-href="<%: shoppingCartServiceUrl %>">
							<a href="<%: shoppingCartUrl %>" title="<%: shoppingCartLinkText %>">
								<span class="fa fa-shopping-cart" aria-hidden="true"></span>
								<%: shoppingCartLinkText %>
								<span class="count">(<span class="value"></span>)</span>
							</a>
						</li>
					<% } %>
				</ul>
				<% if (searchEnabled) { %>
					<form class="navbar-form navbar-search" method="GET" action="<%: searchUrl %>" role="search">
						<label for="q-xs" class="sr-only"><%: searchLabel %></label>
						<div class="input-group">
							<% if (searchFilterOptions.Any()) { %>
								<div class="btn-group btn-select input-group-btn" data-target="#filter-xs" data-focus="#q-xs">
									<button id="search-filter-xs" type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
										<span class="selected"><%: searchFilterDefaultText %></span>
										<span class="caret"></span>
									</button>
									<ul class="dropdown-menu" role="menu" aria-labelledby="search-filter-xs">
										<li>
											<a data-value="" title="<%: searchFilterDefaultText %>"><%: searchFilterDefaultText %></a>
										</li>
										<% foreach (var option in searchFilterOptions) { %>
											<li>
												<a data-value="<%: option.Value %>" title="<%: option.Key %>"><%: option.Key %></a>
											</li>
										<% } %>
									</ul>
								</div>
								<label for="filter-xs" class="sr-only"><%: searchFilterLabel %></label>
								<select id="filter-xs" name="filter" class="btn-select" aria-hidden="true" data-query="filter">
									<option value="" selected="selected"><%: searchFilterDefaultText %></option>
									<% foreach (var option in searchFilterOptions) { %>
										<option value="<%: option.Value %>"><%: option.Key %></option>
									<% } %>
								</select>
							<% } %>
							<input type="text" class="form-control" id="q-xs" name="q" placeholder="<%: searchLabel %>" title="<%: searchLabel %>" data-query="q">
							<div class="input-group-btn">
								<button type="submit" class="btn btn-default" title="<%: searchToolTip %>"><span class="fa fa-search" aria-hidden="true"></span></button>
							</div>
						</div>
					</form>
				<% } %>
			</div>
			<% Html.RenderAction("HeaderPrimaryNavigation", "Layout", new { area = "Portal" }, true); %>
			<div class="navbar-right hidden-xs">
				<%: Html.HtmlSnippet("Navbar Right") %>
			</div>
		</div>
	</div>
</div>
<% if (Html.BooleanSetting("Header/ShowChildNavbar").GetValueOrDefault(false)) { %>
	<% Html.RenderAction("HeaderChildNavbar", "Layout", new { area = "Portal" }, true); %>
<% } %>