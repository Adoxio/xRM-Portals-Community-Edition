/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Site.Pages;

namespace Site.Areas.Service311.Pages
{
	public partial class ServiceRequest : PortalPage
	{
		public string EntityLogicalName;

		protected void Page_Load(object sender, EventArgs e)
		{
			Thumbnail.ImageUrl = GetThumbnailUrl(Entity);

			CreateRequestLink.NavigateUrl = GetFirstChildUrl();

			var maxLatestArticlesSetting = ServiceContext.GetSiteSettingValueByName(Website, "service_request_max_kb_articles");

			int maxLatestArticles;

			maxLatestArticles = int.TryParse(maxLatestArticlesSetting, out maxLatestArticles) ? maxLatestArticles : 3;

			var latestArticles = Enumerable.Empty<Entity>().AsQueryable();

			var subject = Entity.GetAttributeValue<EntityReference>("adx_subjectid");

			if (subject != null)
			{
				latestArticles = XrmContext.CreateQuery("kbarticle")
					.Where(k => k.GetAttributeValue<OptionSetValue>("statecode") != null
						&& k.GetAttributeValue<OptionSetValue>("statecode").Value == (int)Enums.KbArticleState.Published
						&& k.GetAttributeValue<bool?>("msa_publishtoweb").GetValueOrDefault(false)
						&& k.GetAttributeValue<EntityReference>("subjectid").Id == subject.Id)
					.OrderByDescending(k => k.GetAttributeValue<DateTime>("createdon"))
					.Take(maxLatestArticles);
			}

			LatestArticlesList.DataSource = latestArticles;
			LatestArticlesList.DataBind();
		}

		protected string GetFirstChildUrl()
		{
			var current = System.Web.SiteMap.CurrentNode;

			return current == null
				? string.Empty
				: current.ChildNodes.Count < 1
					? string.Empty
					: current.ChildNodes[0].Url;
		}

		protected string GetKbArticleUrl(Entity kbarticle)
		{
			if (kbarticle == null)
			{
				return null;
			}

			try
			{
				return Url.Action("Index", "Article", new
				{
					number = kbarticle.GetAttributeValue<string>("number"),
					area = "KnowledgeBase"
				});
			}
			catch (ArgumentException)
			{
				return null;
			}
		}

		protected string GetThumbnailUrl(Entity webpageEntity)
		{
			if (webpageEntity == null)
			{
				return null;
			}

			var imageReference = webpageEntity.GetAttributeValue<EntityReference>("adx_image");

			if (imageReference == null)
			{
				return null;
			}

			var webfile = ServiceContext.CreateQuery("adx_webfile").FirstOrDefault(file => file.GetAttributeValue<Guid>("adx_webfileid") == imageReference.Id);

			if (webfile == null)
			{
				return null;
			}

			var url = new UrlBuilder(ServiceContext.GetUrl(webfile));

			return url.PathWithQueryString;
		}

		public static string HtmlEncode(object value)
		{
			return value == null ? string.Empty : System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(value.ToString(), true);
		}
	}
}
