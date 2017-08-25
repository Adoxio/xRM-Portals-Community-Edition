<%@ Language="C#" MasterPageFile="~/MasterPages/WebForms.master" AutoEventWireup="true" CodeBehind="ServiceRequestsMap.aspx.cs" Inherits="Site.Areas.Service311.Pages.ServiceRequestsMap" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %> 
<%@ Import Namespace="Adxstudio.Xrm.Web.Mvc.Html" %>
<%@ Import Namespace="Microsoft.Xrm.Sdk" %>
<%@ Import Namespace="Site.Areas.Service311" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/jquery-ui-1.10.4.datepicker.min.css") %>" />
	<link rel="stylesheet" href="<%: Url.Content("~/Areas/Service311/css/311.css") %>" />
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentBottom" runat="server">
	<asp:ScriptManagerProxy runat="server">
		<Scripts>
			<asp:ScriptReference Path="~/Areas/Service311/js/jquery-ui-1.10.4.datepicker.min.js" />
			<asp:ScriptReference Path="~/Areas/Service311/js/json2.min.js" />
			<asp:ScriptReference Path="~/Areas/Service311/js/date.format.min.js" />
			<asp:ScriptReference Path="//ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0&s=1" />
			<asp:ScriptReference Path="~/Areas/Service311/js/settings.js.aspx" />
			<asp:ScriptReference Path="~/Areas/Service311/js/servicemap.js?v=1" />
		</Scripts>
	</asp:ScriptManagerProxy>
	
	<div class="row">
		<div class="col-md-3">
			<div id="searchOptions">
				<div class="content-panel panel panel-default">
					<div class="panel-heading">
						<div class="pull-right">
							<button type="button" class="btn btn-default btn-xs" onclick="ADX.serviceMap.reset();">
								<span class="fa fa-refresh" aria-hidden="true"></span>
								<%: Html.SnippetLiteral("311 Map Search Reset Button Text") ?? ResourceManager.GetString("Reset") %>
							</button>
						</div>
						<h4>
							<a data-toggle="collapse" data-parent="#searchOptions" href="#searchOptionFields"  title='<%= Adxstudio.Xrm.Resources.ResourceFiles.strings.ResourceManager.GetString("Search_DefaultText") %>'>
								<span class="fa fa-map-marker" aria-hidden="true"></span>
								<adx:Snippet runat="server" SnippetName="311 Map Search Title Text" DefaultText="<%$ ResourceManager:Search_DefaultText %>" Editable="true" EditType="text" />
							</a>
						</h4>
					</div>
					<div id="searchOptionFields" class="panel-collapse collapse in">
						<div class="panel-body">
							<div class="form-group">
								<input type="text" id="location-query" class="form-control" placeholder="<%: Html.SnippetLiteral("311 Map Search Location Text") ?? ResourceManager.GetString("Location") %>" />
							</div>
							<div class="form-group">
								<label for="dates">
									<adx:Snippet runat="server" SnippetName="311 Map Search Date Title Text" DefaultText="<%$ ResourceManager:Date_DefaultText %>" Editable="true" EditType="text" />
								</label>
								<select id="dates" class="form-control">
									<option value="0">Last 7 days</option>
									<option value="1">Last 30 days</option>
									<option value="2">Last 12 months</option>
									<option value="3">Other</option>
								</select>
							</div>
							<div id="datesFilter">
								<div class="form-group">
									<label for="dateFrom">
										<adx:Snippet runat="server" SnippetName="311 Map Search Date From Title Text" DefaultText="<%$ ResourceManager:From_DefaultText %>" Editable="true" EditType="text" />
									</label>
									<input id="dateFrom" type="text" class="form-control" />
								</div>
								<div class="form-group">
									<label for="dateTo">
										<adx:Snippet runat="server" SnippetName="311 Map Search Date To Title Text" DefaultText="<%$ ResourceManager:To_DefaultText %>" Editable="true" EditType="text" />
									</label>
									<input id="dateTo" type="text" class="form-control" />
								</div>
							</div>
							<div class="form-group">
								<label for="status">
									<adx:Snippet runat="server" SnippetName="311 Map Search Status Title Text" DefaultText="<%$ ResourceManager:Status_DefaultText %>" Editable="true" EditType="text" />
								</label>
								<asp:ListView ID="ServiceRequestStatusList" runat="server">
									<LayoutTemplate>
										<select id="status" class="form-control">
											<asp:PlaceHolder ID="ItemPlaceHolder" runat="server"/>
										</select>
									</LayoutTemplate>
									<ItemTemplate>
										<option value='<%#: Eval("Id") %>'><%#: Eval("Name") %></option>
									</ItemTemplate>
								</asp:ListView>
							</div>
							<div class="form-group">
								<label for="priority">
									<adx:Snippet runat="server" SnippetName="311 Map Search Priority Title Text" DefaultText="<%$ ResourceManager:Priority_DefaultText %>" Editable="true" EditType="text" />
								</label>
								<asp:ListView ID="ServiceRequestPriorityList" runat="server">
									<LayoutTemplate>
										<select id="priority" class="form-control">
											<asp:PlaceHolder ID="ItemPlaceHolder" runat="server"/>
										</select>
									</LayoutTemplate>
									<ItemTemplate>
										<option value='<%#: Eval("Id") %>'><%#: Eval("Name") %></option>
									</ItemTemplate>
								</asp:ListView>
							</div>
							<div class="form-group">
								<label for="types">
									<adx:Snippet runat="server" SnippetName="311 Map Search Type Title Text" DefaultText="<%$ ResourceManager:Type_DefaultText %>" Editable="true" EditType="text" />
								</label>
								<asp:ListView ID="ServiceRequestTypesList" runat="server">
									<LayoutTemplate>
										<select id="types" class="form-control">
											<asp:PlaceHolder ID="ItemPlaceHolder" runat="server"/>
										</select>
									</LayoutTemplate>
									<ItemTemplate>
										<option value='<%#: Eval("Id") %>'><%#: Eval("Name") %></option>
									</ItemTemplate>
								</asp:ListView>
							</div>
							<div class="form-group">
								<div class="checkbox">
									<label>
										<input id="adx-map-show-alerts" name="adx-map-show-alerts" type="checkbox" />
										<adx:Snippet runat="server" SnippetName="311 Map Search Show Alerts Title Text" DefaultText="<%$ ResourceManager:Show_Alerts_DefaultText %>" Editable="true" EditType="text" />
									</label>
								</div>
							</div>
							<button id="search" type="button" class="btn btn-primary btn-block btn-lg" onclick="ADX.serviceMap.mapIt();">
								<span class="fa fa-map-marker" aria-hidden="true"></span>
								<%: Html.SnippetLiteral("311 Map Search Button Text") ?? ResourceManager.GetString("Search_DefaultText") %>
							</button>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div class="col-md-9">
			<div id="mapContainer">
				<div id="serviceMap"></div>
			</div>
	
			<asp:ListView ID="ServiceRequestTypesLegendList" runat="server">
				<LayoutTemplate>
					<div class="content-panel panel panel-default">
						<div class="panel-heading">
							<h4>
								<span class="fa fa-info-circle" aria-hidden="true"></span>
								<adx:Snippet runat="server" SnippetName="311 Map Legend Title Text" DefaultText="<%$ ResourceManager:Legend_DefaultText %>" Editable="true" EditType="text" />
							</h4>
						</div>
						<div class="panel-body">
							<asp:PlaceHolder ID="ItemPlaceHolder" runat="server"/>
						</div>
					</div>
				</LayoutTemplate>
				<ItemTemplate>
					<div class="legend">
						<%# ServiceRequestHelpers.BuildServiceRequestTypeThumbnailImageTag(ServiceContext, Container.DataItem as Entity) %>
						<h6><%# ((Entity)Container.DataItem).GetAttributeValue<string>("adx_name") %></h6>
					</div>
				</ItemTemplate>
			</asp:ListView>
		</div>
	</div>

	<script type="text/javascript">
		try {
			ADX.serviceMap.initialize('serviceMap', "<%: Url.Action("Search", "Map", new{ area = "Service311" }) %>", true);
		}
		catch(err) {
			alert(err.message);
		}

		// disable the enter keypress, it resets the map
		$("#location-query").keypress(function (e) {
			if (e.keyCode == 13) {
				return false;
			}
		});
		// use keyup to click the search, keyup will not interfere with browser autocomplete
		$("#location-query").keyup(function (e) {
			if (e.keyCode == 13) {
				$("#search").click();
			}
		});
	</script>
</asp:Content>
