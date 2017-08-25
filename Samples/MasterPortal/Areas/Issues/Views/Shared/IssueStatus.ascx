<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Adxstudio.Xrm.Issues.IIssue>" %>
<%@ Import Namespace="Site.Areas.Issues" %>

<span class="label
<%= Model.Status == (int)IssueStatus.NewOrUnconfirmed ? " label-info" : string.Empty %>
<%= Model.Status == (int)IssueStatus.Inactive ? " label-default" : string.Empty %>
<%= Model.Status == (int)IssueStatus.Confirmed ? " label-primary" : string.Empty %>
<%= Model.Status == (int)IssueStatus.WorkaroundAvailable ? " label-success" : string.Empty %>
<%= Model.Status == (int)IssueStatus.Resolved ? " label-success" : string.Empty %>
<%= Model.Status == (int)IssueStatus.WillNotFix ? " label-warning" : string.Empty %>
<%= Model.Status == (int)IssueStatus.ByDesign ? " label-success" : string.Empty %>
<%= Model.Status == (int)IssueStatus.UnableToReproduce ? " label-danger" : string.Empty %>"><%: Regex.Replace(Enum.GetName(typeof(IssueStatus), Model.Status), "([a-z])([A-Z])", "$1 $2")%></span>
