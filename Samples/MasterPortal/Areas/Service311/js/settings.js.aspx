<%@ Page Language="C#" ContentType="text/javascript" Trace="false" %>
<%@ OutputCache Duration="28800" VaryByParam="None" VaryByContentEncoding="gzip;x-gzip;deflate" %>
if (typeof ADX == "undefined" || !ADX) {
	var ADX = {};
}
ADX.settings = (function () {
	var _export = {};
	var _mapSettings = {
		key: '<asp:Literal runat="server" Text="<%$ SiteSetting: map_credentials %>" />',
		zoom: <asp:Literal runat="server" Text="<%$ SiteSetting: map_zoom, 12 %>" />,
		latitude: <asp:Literal runat="server" Text="<%$ SiteSetting: map_latitude, 47.67858 %>" />,
		longitude: <asp:Literal runat="server" Text="<%$ SiteSetting: map_longitude, -122.13158 %>" />,
		restServiceUrl: '<asp:Literal runat="server" Text="<%$ SiteSetting: map_rest_url, https://dev.virtualearth.net?REST/v1/Locations %>" />',
		width: <asp:Literal runat="server" Text="<%$ SiteSetting: map_width %>" />,
		height: <asp:Literal runat="server" Text="<%$ SiteSetting: map_height %>" />,
		pushpin: {
			width: <asp:Literal runat="server" Text="<%$ SiteSetting: map_pushpin_width, 32 %>" />,
			height: <asp:Literal runat="server" Text="<%$ SiteSetting: map_pushpin_height, 39 %>" />
		},
		infobox: {
			offset: {
				x: <asp:Literal runat="server" Text="<%$ SiteSetting: map_infobox_offset_x, 25 %>" />,
				y: <asp:Literal runat="server" Text="<%$ SiteSetting: map_infobox_offset_y, 46 %>" />
			}
		}
	};
	var _serviceRequestSettings = {
		statusColors: '<asp:Literal runat="server" Text="<%$ SiteSetting: map_service_status_colors %>" />',
		priorityColors: '<asp:Literal runat="server" Text="<%$ SiteSetting: map_service_priority_colors %>" />'
	};
	_export.mapSettings = _mapSettings;
	_export.serviceRequestSettings = _serviceRequestSettings;
	return _export;
})();