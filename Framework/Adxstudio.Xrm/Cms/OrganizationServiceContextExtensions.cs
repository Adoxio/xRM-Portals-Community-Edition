/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Tagging;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Web.UI.WebForms;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// Helper methods on the <see cref="OrganizationServiceContext"/> class.
	/// </summary>
	public static class OrganizationServiceContextExtensions
	{
		#region Generic

		/// <summary>
		/// Retrieves the website corresponding to an entity according to the <see cref="IEntityWebsiteProvider"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <param name="portalName"></param>
		/// <returns></returns>
		public static Entity GetWebsite(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var websiteProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityWebsiteProvider>();

			return websiteProvider.GetWebsite(context, entity);
		}

		public static string GetUrl(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

			return urlProvider.GetUrl(context, entity);
		}

		public static ApplicationPath GetApplicationPath(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

			return urlProvider.GetApplicationPath(context, entity);
		}

		#endregion

		#region Page Comments

		public static QueryExpression SelectCommentsByPage(PagingInfo pageInfo, Guid regardingobjectid, bool includeUnapprovedComments, bool? chronologicalComments = null)
		{
			var query = new QueryExpression("feedback")
			{
				PageInfo = pageInfo
			};

			query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, regardingobjectid);
			query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
			query.Criteria.AddCondition("comments", ConditionOperator.NotNull);
			if (!includeUnapprovedComments)
			{
				query.Criteria.AddCondition("adx_approved", ConditionOperator.Equal, true);
			}
			query.ColumnSet = new ColumnSet()
			{
				Columns = { "createdbycontact", "adx_createdbycontact", "adx_authorurl", "adx_contactemail", "comments", "createdon", "title", "createdbycontact", "adx_approved" }
			};
			query.AddOrder("createdon", chronologicalComments == null || chronologicalComments.Value ? OrderType.Ascending : OrderType.Descending);

			LinkEntity link = query.AddLink("contact", "createdbycontact", "contactid", JoinOperator.LeftOuter);
			link.EntityAlias = "author";
			link.Columns.AddColumn("fullname");
			link.Columns.AddColumn("firstname");
			link.Columns.AddColumn("lastname");
			link.Columns.AddColumn("emailaddress1");
			return query;
		}

		public static PagingInfo GetPageInfo(int startRowIndex, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
				throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}
			var pageNumber = 1;
			if (startRowIndex > 0)
			{
				pageNumber = (int)startRowIndex / maximumRows + 1;
			}
			return new PagingInfo
			{
				PageNumber = pageNumber,
				Count = maximumRows
			};
		}

		public static IDictionary<Guid, Tuple<string, string, string, IRatingInfo>> FetchPageCommentExtendedData(this OrganizationServiceContext serviceContext, IEnumerable<Guid> commentIds)
		{
			return FetchPageCommentExtendedData(serviceContext, commentIds.ToArray())
				.ToDictionary(data => data.Item1, data => new Tuple<string, string, string, IRatingInfo>(data.Item2, data.Item3, data.Item4, data.Item5));
		}

		private static IEnumerable<Tuple<Guid, string, string, string, IRatingInfo>> FetchPageCommentExtendedData(
			OrganizationServiceContext serviceContext, Guid[] commentIds)
		{
			if (!FeatureCheckHelper.IsFeatureEnabled(FeatureNames.Feedback))
			{
				return new List<Tuple<Guid, string, string, string, IRatingInfo>>();
			}
			if (!commentIds.Any())
			{
				return Enumerable.Empty<Tuple<Guid, string, string, string, IRatingInfo>>();
			}

			var fetchXml = XDocument.Parse(@"
					<fetch mapping=""logical"">
						<entity name=""feedback"">
							<filter type=""and"">
							</filter>
							<link-entity link-type=""outer"" name=""contact"" from=""contactid"" to=""createdbycontact"" alias=""author"">
								<attribute name=""contactid"" />
								<attribute name=""fullname"" />
								<attribute name=""firstname"" />
								<attribute name=""lastname"" />
								<attribute name=""emailaddress1"" />
								<attribute name=""websiteurl"" />
							</link-entity>
						</entity>
					</fetch>");

			var filter = fetchXml.Descendants("filter").First();

			filter.AddFetchXmlFilterInCondition(FeedbackMetadataAttributes.PageCommentIdAttribute,
				commentIds.Select(id => id.ToString()));

			var response = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(fetchXml.ToString())
			});

			var aggregateFetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"" aggregate=""true"">
					<entity name=""feedback"">
						<attribute name=""regardingobjectid"" alias=""ratingcount"" aggregate=""countcolumn""/>
						<attribute name=""rating"" alias=""ratingsum"" aggregate=""sum"" />
						<attribute name=""rating"" alias=""value"" groupby=""true"" />
						<link-entity name=""feedback"" from=""feedbackid"" to=""regardingobjectid"">
							<attribute name=""feedbackid"" alias=""commentid"" groupby=""true"" />
							<filter type=""and"" />
						</link-entity>
					</entity>
				</fetch>");


			var aggregateFilter = aggregateFetchXml.Descendants("filter").First();

			aggregateFilter.AddFetchXmlFilterInCondition(FeedbackMetadataAttributes.PageCommentIdAttribute,
				commentIds.Select(id => id.ToString()));

			var aggregateResponse = (RetrieveMultipleResponse)serviceContext.Execute(new RetrieveMultipleRequest
			{
				Query = new FetchExpression(aggregateFetchXml.ToString())
			});

			return commentIds.Select(id =>
			{
				var entity = response.EntityCollection.Entities.FirstOrDefault(e => e.Id == id);

				if (entity == null)
				{
					return new Tuple<Guid, string, string, string, IRatingInfo>(id, null, null, null, null);
				}

				var authorName = Localization.LocalizeFullName(entity.GetAttributeAliasedValue<string>("author.firstname"), entity.GetAttributeAliasedValue<string>("author.lastname"));
				var authorUrl = entity.GetAttributeAliasedValue<string>("author.websiteurl");
				var authorEmail = entity.GetAttributeAliasedValue<string>("author.emailaddress1");

				var aggregateResults = aggregateResponse.EntityCollection.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id);

				var aggregateYesResult = aggregateResponse.EntityCollection.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id)
					.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 1);

				var aggregateNoResult = aggregateResponse.EntityCollection.Entities
					.Where(e => e.GetAttributeAliasedValue<Guid?>("commentid") == id)
					.FirstOrDefault(e => e.GetAttributeAliasedValue<int?>("value") == 0);

				var yesCount = (aggregateYesResult != null)
					? aggregateYesResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0
					: 0;

				var noCount = (aggregateNoResult != null)
					? aggregateNoResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0
					: 0;

				int ratingCount = 0;
				int ratingSum = 0;

				foreach (var aggregateResult in aggregateResults)
				{
					ratingCount = ratingCount + aggregateResult.GetAttributeAliasedValue<int?>("ratingcount") ?? 0;
					ratingSum = ratingSum + aggregateResult.GetAttributeAliasedValue<int?>("ratingsum") ?? 0;
				}

				double averageRating = 0;

				if (ratingCount == 0)
				{
					averageRating = 0;
				}
				else
				{
					averageRating = (double)ratingSum / (double)ratingCount;
				}

				var ratingInfo = new RatingInfo(yesCount, noCount, averageRating, ratingCount, ratingSum);

				return new Tuple<Guid, string, string, string, IRatingInfo>(id, authorName, authorUrl, authorEmail, ratingInfo);
			});
		}

		#endregion

		#region Tags

		public static IEnumerable<Entity> GetPageTags(this OrganizationServiceContext context)
		{
			return context.CreateQuery("adx_pagetag").ToList();
		}

		private static Entity GetPageTagByName(this OrganizationServiceContext context, string tagName)
		{
			return context.CreateQuery("adx_pagetag").ToList().Where(pt => TagName.Equals(pt.GetAttributeValue<string>("adx_name"), tagName)).FirstOrDefault();
		}

		/// <summary>
		/// Adds a Page Tagtag association by name to a Web Page.
		/// </summary>
		/// <param name="pageId">The ID of the Web Page whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be associated with the page (will be created if necessary).
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void AddTagToWebPageAndSave(this OrganizationServiceContext context, Guid pageId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (pageId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "pageId");
			}

			var page = context.CreateQuery("adx_webpage").Single(p => p.GetAttributeValue<Guid>("adx_webpageid") == pageId);

			var tag = context.GetPageTagByName(tagName);

			// If the tag doesn't exist, create it
			if (tag == null)
			{
				tag = new Entity("adx_pagetag");
				tag["adx_name"] = tagName;

				context.AddObject(tag);
				context.SaveChanges();
				context.ReAttach(page);
				context.ReAttach(tag);
			}

			if (!page.GetRelatedEntities(context, "adx_pagetag_webpage").Any(t => t.GetAttributeValue<Guid>("adx_pagetagid") == tag.Id))
			{
				context.AddLink(page, new Relationship("adx_pagetag_webpage"), tag);

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Removes a Page Tagtag association by name from a Web Page.
		/// </summary>
		/// <param name="pageId">The ID of the Web Page whose tags will be affected.</param>
		/// <param name="tagName">
		/// The name of the tag to be dis-associated from the page.
		/// </param>
		/// <remarks>
		/// <para>
		/// This operation may call SaveChanges on this context--please ensure any queued
		/// changes are mananged accordingly.
		/// </para>
		/// </remarks>
		public static void RemoveTagFromWebPageAndSave(this OrganizationServiceContext context, Guid pageId, string tagName)
		{
			if (context.MergeOption == MergeOption.NoTracking)
			{
				throw new ArgumentException("The OrganizationServiceContext.MergeOption cannot be MergeOption.NoTracking.", "context");
			}

			if (string.IsNullOrEmpty(tagName))
			{
				throw new ArgumentException("Can't be null or empty.", "tagName");
			}

			if (pageId == Guid.Empty)
			{
				throw new ArgumentException("Argument must be a non-empty GUID.", "pageId");
			}

			var page = context.CreateQuery("adx_webpage").Single(p => p.GetAttributeValue<Guid>("adx_webpageid") == pageId);

			var tag = context.GetPageTagByName(tagName);

			// If the tag doesn't exist, do nothing
			if (tag == null)
			{
				return;
			}

			context.DeleteLink(page, new Relationship("adx_pagetag_webpage"), tag);
			context.SaveChanges();
		}

		#endregion

		#region Web Page
		
		/// <summary>
		/// Retrieves the child pages of this page.
		/// </summary>
		public static IEnumerable<Entity> GetChildPages(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("adx_webpage");

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			var rootPage = langContext.GetRootWebPageEntity(context, webPage);
			var childPages = rootPage.GetRelatedEntities(context, "adx_webpage_webpage", EntityRole.Referenced);
			
			if (langContext.IsCrmMultiLanguageEnabled)
			{
				var currentLanguage = langContext.ContextLanguage.EntityReference;
				// filter-out current-language child pages
				childPages = childPages.Where(page => currentLanguage.Equals(page.GetAttributeValue<EntityReference>("adx_webpagelanguageid")) && !page.GetAttributeValue<bool>("adx_isroot"));
			}
			return childPages;
		}

		/// <summary>
		/// Retrieves the child files of this page.
		/// </summary>
		public static IEnumerable<Entity> GetChildFiles(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("adx_webpage");

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			var rootPage = langContext.GetRootWebPageEntity(context, webPage);
			var childFiles = rootPage.GetRelatedEntities(context, "adx_webpage_webfile");
			return childFiles;
		}

		/// <summary>
		/// Retrieves the visible child pages of this page.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPages(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("adx_webpage");

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			var rootPage = langContext.GetRootWebPageEntity(context, webPage);
			var childPages = context.GetChildPages(rootPage);
			var visibleChildPages = childPages.Where(cp => !cp.GetAttributeValue<bool?>("adx_hiddenfromsitemap").GetValueOrDefault(false));
			return visibleChildPages;
		}

		/// <summary>
		/// Retrieves the child shortcut nodes
		/// </summary>
		public static IEnumerable<Entity> GetChildShortcuts(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("adx_webpage");

			var langContext = HttpContext.Current.GetContextLanguageInfo();
			var rootPage = langContext.GetRootWebPageEntity(context, webPage);
			var childShortcuts = rootPage.GetRelatedEntities(context, "adx_parentwebpage_shortcut");
			return childShortcuts;
		}

		private class SurveyEqualityComparer : IEqualityComparer<Entity>
		{
			public bool Equals(Entity x, Entity y)
			{
				if (x == null && y == null)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				return GetId(x) == GetId(y);
			}

			public int GetHashCode(Entity survey)
			{
				if (survey == null)
				{
					throw new ArgumentNullException("survey");
				}

				return GetId(survey).GetHashCode();
			}

			private static Guid? GetId(Entity survey)
			{
				return survey.GetAttributeValue<Guid?>("adx_surveyid");
			}
		}

		#endregion

		#region Website

		public static IEnumerable<Entity> GetLinkSets(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var webLinkSets = website.GetRelatedEntities(context, "adx_website_weblinkset");
			return webLinkSets;
		}

		public static Entity GetLinkSetByName(this OrganizationServiceContext context, Entity website, string webLinkSetName)
		{
			var webLinkSets = context.GetLinkSets(website);
			return webLinkSets.FirstOrDefault(wls => wls.GetAttributeValue<string>("adx_name") == webLinkSetName);
		}

		public static IEnumerable<Entity> GetMembershipTypes(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var membershipTypes = website.GetRelatedEntities(context, "adx_website_membershiptype");
			return membershipTypes;
		}

		public static Entity GetMembershipTypeByName(this OrganizationServiceContext context, Entity website, string membershipTypeName)
		{
			var membershipTypes = context.GetMembershipTypes(website);
			return membershipTypes.FirstOrDefault(mt => mt.GetAttributeValue<string>("adx_name") == membershipTypeName);
		}

		public static Entity GetPageBySiteMarkerName(this OrganizationServiceContext context, Entity website, string siteMarkerName)
		{
			website.AssertEntityName("adx_website");

			var siteMarkers = new SiteMarkerDataAdapter(new WebsiteDataAdapterDependencies(context, website, PortalCrmConfigurationManager.CreatePortalContext()));

			var siteMarkerTarget = siteMarkers.Select(siteMarkerName);

			return siteMarkerTarget == null ? null : siteMarkerTarget.Entity;
		}

		public static IEnumerable<Entity> GetSiteSettings(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var siteSettings = website.GetRelatedEntities(context, "adx_website_sitesetting");
			return siteSettings;
		}

		public static Entity GetSiteSettingByName(this OrganizationServiceContext context, Entity website, string siteSettingName)
		{
			var siteSettings = context.GetSiteSettings(website);
			return (from s in siteSettings where s.GetAttributeValue<string>("adx_name") == siteSettingName select s).FirstOrDefault();
		}

		public static string GetSiteSettingValueByName(this OrganizationServiceContext context, Entity website, string siteSettingName)
		{
			website.AssertEntityName("adx_website");

			var portalViewContext = new PortalViewContext(new WebsiteDataAdapterDependencies(context, website, PortalCrmConfigurationManager.CreatePortalContext()), SiteMap.Provider);

			return portalViewContext.Settings.GetValue(siteSettingName);
		}

		public static IEnumerable<Entity> GetSiteMarkers(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var siteMarkers = website.GetRelatedEntities(context, "adx_website_sitemarker");
			return siteMarkers;
		}

		public static Entity GetSiteMarkerByName(this OrganizationServiceContext context, Entity website, string siteMarkerName)
		{
			var siteMarkers = context.GetSiteMarkers(website);
			return siteMarkers.FirstOrDefault(sm => sm.GetAttributeValue<string>("adx_name") == siteMarkerName);
		}

		/// <summary>
		/// Retrieves visible child Web Page entities for a given site-marker.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPagesBySiteMarker(this OrganizationServiceContext context, Entity website, string siteMarker)
		{
			website.AssertEntityName("adx_website");

			var findPages =
				from cwp in context.CreateQuery("adx_webpage")
				join wp in context.CreateQuery("adx_webpage")
					on cwp.GetAttributeValue<EntityReference>("adx_parentpageid").Id equals wp.GetAttributeValue<Guid>("adx_webpageid")
				join sm in context.CreateQuery("adx_sitemarker")
					on wp.GetAttributeValue<Guid>("adx_webpageid") equals sm.GetAttributeValue<EntityReference>("adx_pageid").Id
				// filter to current site
				where sm.GetAttributeValue<EntityReference>("adx_pageid") != null && sm.GetAttributeValue<EntityReference>("adx_websiteid") == website.ToEntityReference() && sm.GetAttributeValue<string>("adx_name") == siteMarker
				select cwp;

			return findPages.Cast<Entity>().ToList();
		}

		public static TimeZoneInfo GetTimeZone(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("adx_website");

			var timezoneid = context.GetSiteSettingValueByName(website, "timezone/id");

			return !string.IsNullOrEmpty(timezoneid)
				? TimeZoneInfo.FindSystemTimeZoneById(timezoneid)
				: TimeZoneInfo.Local;
		}

		public static IEnumerable<Entity> GetPublishedForums(this OrganizationServiceContext context, Entity website, string portalName = null)
		{
			website.AssertEntityName("adx_website");

			var forums = website.GetRelatedEntities(context, "adx_website_communityforum");

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

			return forums.Where(f => securityProvider.TryAssert(context, f, CrmEntityRight.Read));
		}

		public static IEnumerable<Entity> GetPublishedEvents(this OrganizationServiceContext context, Entity website, string portalName = null)
		{
			website.AssertEntityName("adx_website");

			var events = website.GetRelatedEntities(context, "adx_website_event");

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

			return events.Where(e => securityProvider.TryAssert(context, e, CrmEntityRight.Read));
		}

		public static IEnumerable<Entity> GetRecentlyPublishedEvents(this OrganizationServiceContext context, Entity website, int daysOut = 7, string portalName = null)
		{
			website.AssertEntityName("adx_website");

			var now = DateTime.Now.Floor(RoundTo.Minute);

			return
				from evnt in context.GetPublishedEvents(website)
				where evnt.GetAttributeValue<DateTime>("createdon") > now.AddDays(-daysOut)
				select evnt;
		}

		public static IEnumerable<Entity> GetPublishedEventsForScheduledDates(this OrganizationServiceContext context, Entity website, TimeSpan duration)
		{
			website.AssertEntityName("adx_website");

			return
				from evnt in context.GetPublishedEvents(website)
				where context.GetScheduledDates(evnt, duration).Any()
				select evnt;
		}

		public static IEnumerable<Entity> GetPublishedEventsForScheduledDates(this OrganizationServiceContext context, Entity website, DateTime firstDate, DateTime lastDate)
		{
			website.AssertEntityName("adx_website");

			return
				from evnt in context.GetPublishedEvents(website)
				where context.GetScheduledDates(evnt, firstDate, lastDate).Any()
				select evnt;
		}

		public static IEnumerable<DateTime> GetScheduledDatesForPublishedEvents(this OrganizationServiceContext context, Entity website, TimeSpan duration)
		{
			website.AssertEntityName("adx_website");

			return
				from evnt in context.GetPublishedEvents(website)
				from date in context.GetScheduledDates(evnt, duration)
				select date;
		}

		public static IEnumerable<DateTime> GetScheduledDatesForPublishedEvents(this OrganizationServiceContext context, Entity website, DateTime firstDate, DateTime lastDate)
		{
			website.AssertEntityName("adx_website");

			return
				from evnt in context.GetPublishedEvents(website)
				from date in context.GetScheduledDates(evnt, firstDate, lastDate)
				select date;
		}

		public static IEnumerable<Entity> GetPublishedSurveys(this OrganizationServiceContext context, Entity website, string portalName = null)
		{
			website.AssertEntityName("adx_website");

			var surveys = website.GetRelatedEntities(context, "adx_website_survey");

			var securityProvider = PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName);

			return surveys.Where(s => securityProvider.TryAssert(context, s, CrmEntityRight.Read));
		}

		#endregion

		#region Web Link Set

		public static IOrderedEnumerable<Entity> GetOrderedWebLinks(this OrganizationServiceContext context, Entity webLinkSet)
		{
			webLinkSet.AssertEntityName("adx_weblinkset");

			var webLinks = webLinkSet.GetRelatedEntities(context, "adx_weblinkset_weblink");
			return webLinks.OrderBy(wl => wl.GetAttributeValue<int?>("adx_displayorder"));
		}

		#endregion

		#region Web Links

		/// <summary>
		/// Retrieves a list of visible child pages for this web link.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPagesForWebLink(this OrganizationServiceContext context, Entity webLink)
		{
			webLink.AssertEntityName("adx_weblink");

			var page = webLink.GetRelatedEntity(context, "adx_webpage_weblink");
			return page == null ? null : context.GetVisibleChildPages(page);
		}

		#endregion

		#region Web File

		/// <summary>
		/// Retrieves the CRM notes that the file is uploaded to.
		/// </summary>
		public static IEnumerable<Entity> GetNotes(this OrganizationServiceContext context, Entity file)
		{
			file.AssertEntityName("adx_webfile");

			var notes = file.GetRelatedEntities(context, "adx_webfile_Annotations");
			return notes;
		}

		/// <summary>
		/// Retrieves the CRM note that the file is uploaded to.
		/// </summary>
		public static Entity GetNote(this OrganizationServiceContext context, Entity file)
		{
			var notes = context.GetNotes(file);
			return notes.FirstOrDefault();
		}

		#endregion

		#region adx_sitemarker

		public static Entity GetWebPage(this OrganizationServiceContext context, Entity siteMarker)
		{
			siteMarker.AssertEntityName("adx_sitemarker");

			var webPage = siteMarker.GetRelatedEntity(context, "adx_webpage_sitemarker");
			return webPage;
		}

		#endregion

		#region adx_setting

		public static IEnumerable<Entity> GetSettings(this OrganizationServiceContext context)
		{
			var settings = context.CreateQuery("adx_setting");
			return settings;
		}

		public static Entity GetSettingByName(this OrganizationServiceContext context, string settingName)
		{
			var settings = context.GetSettings();
			return (from s in settings where s.GetAttributeValue<string>("adx_name") == settingName select s).FirstOrDefault();
		}

		public static string GetSettingValueByName(this OrganizationServiceContext context, string settingName)
		{
			var setting = context.GetSettingByName(settingName);
			return (setting == null ? null : setting.GetAttributeValue<string>("adx_value"));
		}

		#endregion

		private class WebsiteDataAdapterDependencies : DataAdapterDependencies
		{
			public WebsiteDataAdapterDependencies(OrganizationServiceContext serviceContext, Entity website, IPortalContext portalContext, string portalName = null) : base(
					serviceContext,
					PortalCrmConfigurationManager.CreateCrmEntitySecurityProvider(portalName),
					PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>(),
					website.ToEntityReference(),
					portalContext.User == null ? null : portalContext.User.ToEntityReference())
			{
				PortalName = portalName;
			}

			public override OrganizationServiceContext GetServiceContextForWrite()
			{
				return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
			}
		}
	}
}
