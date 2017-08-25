<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import namespace="Adxstudio.Xrm.Web.Mvc.Html" %>

<% var footerLinks = Html.WebLinkSet("Footer"); %>
<% if (footerLinks != null) { %>
	<div class="footer well" role="complementary">
		<div class="container">
			<div class="row">
				<div class="col-md-4">
					<%: Html.HtmlAttribute(footerLinks.Copy) %>
				</div>
				<div class="col-md-8 <%: footerLinks.Editable ? "xrm-entity xrm-editable-adx_weblinkset" : string.Empty %>" data-weblinks-maxdepth="2">
					<% if (footerLinks.WebLinks.Any()) { %>
						<% var rowIndex = 0; %>
						<% while (true) { %>
							<%
								var rowLinks = footerLinks.WebLinks.Skip(rowIndex * 4).Take(4).ToArray();
								if (!rowLinks.Any()) break;
								rowIndex++;
							%>
							<ul class="row list-unstyled">
								<% foreach (var link in rowLinks) { %>
									<li class="col-sm-2">
                                        <%if (link.Url == null)
                                            {%>
                                            <h4><a href="#"  title="Support" ><%: Html.AttributeLiteral(link.Name) %></a></h4>
                                        <%}else { %>
                                            <h4><%:  Html.WebLink(link) %></h4>
                                        <%} %>
										<% if (link.Description != null && link.Description.Value != null) { %>
											<%: Html.HtmlAttribute(link.Description, cssClass: "weblink-description") %>
										<% } %>
										<ul class="list-unstyled">
											<% if (link.DisplayPageChildLinks) { %>
												<% foreach (var childNode in Html.SiteMapChildNodes(link.Url)) { %>
													<li>
														<a href="<%: childNode.Url %>" title="<%: childNode.Title %>"><%: childNode.Title %></a>
                                                        
													</li>
												<% } %>
											<% } else { %>
												<% foreach (var childLink in link.WebLinks) { %>
													<%: Html.WebLinkListItem(childLink, maximumWebLinkChildDepth: 0) %>
												<% } %>
											<% } %>
										</ul>
									</li>
								<% } %>
							</ul>
						<% } %>
					<% } else { %>
						<ul class="row list-unstyled"></ul>
					<% } %>
					<% if (footerLinks.Editable) { %>
						<%: Html.WebLinkSetEditingMetadata(footerLinks) %>
					<% } %>
				</div>
			</div>
		</div>
	</div>
<% } %>
<div class="footer-bottom" role="contentinfo">
	<div class="container">
		<%: Html.HtmlSnippet("Footer") %>
	</div>
</div>
