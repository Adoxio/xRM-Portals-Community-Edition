/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc.Html;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Controls;

namespace Site.Areas.KnowledgeBase.Controls
{
	public partial class Search : PortalUserControl
	{
		protected Guid SubjectId { get; private set; }

		protected string SubjectName { get; private set; }

		protected void Page_Load(object sender, EventArgs args)
		{
			KnowledgeBaseQuery.Attributes["placeholder"] = Html.SnippetLiteral("Knowledge Base Search Query Placeholder", ResourceManager.GetString("Search_The_Knowledge_Base"));

			if (IsPostBack)
			{
				return;
			}

			KnowledgeBaseQuery.Text = Request.QueryString["kbquery"];

			Tuple<string, Guid, SubjectSource> subjectName;

			if (TryGetSubjectName(out subjectName))
			{
				SubjectId = subjectName.Item2;
				SubjectName = subjectName.Item1;
				KnowledgeBaseSubjectFilter.Items.Add(new ListItem(SubjectName, SubjectId.ToString()) { Selected = true });
				KnowledgeBaseSubjectFilter.Items.Add(new ListItem(Html.SnippetLiteral("Knowledge Base Default Search Filter Text", "All Articles"), string.Empty));
				Subject.Visible = true;
			}
		}

		protected void SubmitSearch_Click(object sender, EventArgs args)
		{
			var path = Html.SiteMarkerUrl("Knowledge Base Search Results");

			if (path == null)
			{
				throw new InvalidOperationException("Unable to retrieve the URL for Site Marker Knowledge Base Search Results.");
			}

			var url = new UrlBuilder(path);

			url.QueryString["kbquery"] = KnowledgeBaseQuery.Text;

			var subjectFilter = KnowledgeBaseSubjectFilter.SelectedValue;

			if (!string.IsNullOrEmpty(subjectFilter))
			{
				url.QueryString["subjectid"] = subjectFilter;
			}

			Response.Redirect(url.PathWithQueryString);
		}

		private enum SubjectSource
		{
			Page,
			Query
		}

		private bool TryGetSubjectId(out Tuple<Guid, SubjectSource> subjectId)
		{
			subjectId = null;

			Guid id;

			if (!string.IsNullOrEmpty(Request["subjectid"]) && Guid.TryParse(Request["subjectid"], out id))
			{
				subjectId = new Tuple<Guid, SubjectSource>(id, SubjectSource.Query);

				return true;
			}

			if (Entity.LogicalName != "adx_webpage")
			{
				return false;
			}

			var webPage = XrmContext.CreateQuery("adx_webpage")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_webpageid") == Entity.Id);

			if (webPage == null)
			{
				return false;
			}

			var subject = webPage.GetAttributeValue<EntityReference>("adx_subjectid");

			if (subject == null)
			{
				return false;
			}

			subjectId = new Tuple<Guid, SubjectSource>(subject.Id, SubjectSource.Page);

			return true;
		}

		private bool TryGetSubjectName(out Tuple<string, Guid, SubjectSource> subjectName)
		{
			subjectName = null;

			Tuple<Guid, SubjectSource> subjectId;

			if (!TryGetSubjectId(out subjectId))
			{
				return false;
			}

			var subject = XrmContext.CreateQuery("subject")
				.FirstOrDefault(e => e.GetAttributeValue<Guid>("subjectid") == subjectId.Item1);

			if (subject == null)
			{
				return false;
			}

			subjectName = new Tuple<string, Guid, SubjectSource>(subject.GetAttributeValue<string>("title"), subjectId.Item1, subjectId.Item2);

			return !string.IsNullOrEmpty(subjectName.Item1);
		}
	}
}
