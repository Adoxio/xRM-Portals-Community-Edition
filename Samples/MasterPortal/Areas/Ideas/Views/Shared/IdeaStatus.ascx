<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Ideas.IIdea>" %>
<%@ Import Namespace="Adxstudio.Xrm.Ideas" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>

<span class="label
<%= Enum.IsDefined(typeof(IdeaStatus), Model.Status) ? string.Empty : " label-default" %>
<%= Model.Status == (int)IdeaStatus.New ? " label-info" : string.Empty %>
<%= Model.Status == (int)IdeaStatus.Inactive ? " label-default" : string.Empty %>
<%= Model.Status == (int)IdeaStatus.Accepted ? " label-primary" : string.Empty %>
<%= Model.Status == (int)IdeaStatus.Completed ? " label-success" : string.Empty %>
<%= Model.Status == (int)IdeaStatus.Rejected ? " label-danger" : string.Empty %>">
<%: Enum.IsDefined(typeof(IdeaStatus), Model.Status) ? ResourceManager.GetString(Enum.GetName(typeof(IdeaStatus), Model.Status)) : Model.StatusDisplayName %>&nbsp;</span>
