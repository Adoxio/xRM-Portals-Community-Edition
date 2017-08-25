/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Xml;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using OrganizationServiceContextExtensions = Microsoft.Xrm.Portal.Cms.OrganizationServiceContextExtensions;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Generates a feed of job postings listed on a site.
	/// </summary>
	public class JobPostingsFeedHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "application/atom+xml";
			context.Response.ContentEncoding = Encoding.UTF8;

			var snippets = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());

			var titleSnippet = snippets.Select("Careers/Feed/Title");
			var descriptionSnippet = snippets.Select("Careers/Feed/Description");

			var feed = new SyndicationFeed(GetSyndicationItems())
			{
				Title = SyndicationContent.CreatePlaintextContent(titleSnippet == null ? ResourceManager.GetString("Job_Postings_Text") : titleSnippet.Value.ToString()),
				Description = SyndicationContent.CreatePlaintextContent(descriptionSnippet == null ? ResourceManager.GetString("Job_Postings_Text") : descriptionSnippet.Value.ToString())
			};

			using (var writer = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8))
			{
				feed.SaveAsAtom10(writer);
			}
		}

		protected virtual Uri GetSyndicationItemUri(IPortalContext portalContext, string applicationPageUrl, Entity posting)
		{
			return string.IsNullOrEmpty(applicationPageUrl)
				? null
				: new Uri(applicationPageUrl + "?jobid=" + posting.Id, UriKind.Relative);
		}

		protected virtual IEnumerable<SyndicationItem> GetSyndicationItems()
		{
			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();

			var query = from jp in serviceContext.CreateQuery("adx_jobposting")
				where jp.GetAttributeValue<EntityReference>("adx_websiteid") == portalContext.Website.ToEntityReference()
				where jp.GetAttributeValue<OptionSetValue>("statecode") != null && jp.GetAttributeValue<OptionSetValue>("statecode").Value == 0
				orderby jp.GetAttributeValue<DateTime?>("adx_name")
				select jp;

			var postings = query.ToArray().Where(e => IsOpen(e.GetAttributeValue<DateTime?>("adx_closingon")));

			var applicationPage = OrganizationServiceContextExtensions.GetPageBySiteMarkerName(portalContext.ServiceContext, portalContext.Website, "Job Application");
			var applicationPageUrl = applicationPage == null ? null : OrganizationServiceContextExtensions.GetUrl(portalContext.ServiceContext, applicationPage);

			return postings.Select(posting => new SyndicationItem(
				posting.GetAttributeValue<string>("adx_name"),
				SyndicationContent.CreateHtmlContent(posting.GetAttributeValue<string>("adx_description")),
				GetSyndicationItemUri(portalContext, applicationPageUrl, posting),
				posting.GetAttributeValue<Guid>("adx_jobpostingid").ToString(),
				posting.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow)));
		}

		private static bool IsOpen(DateTime? closingOn)
		{
			return closingOn == null || closingOn.Value.AddDays(1).Date >= DateTime.UtcNow;
		}
	}
}
