/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.XPath;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Cms.Badges.Controllers
{
	[PortalView]
	public class BadgesController : Controller
	{

		private class BadgeComparer : System.Collections.Generic.IEqualityComparer<Entity>
		{

			public bool Equals(Entity x, Entity y)
			{
				return x.GetAttributeAliasedValue<Guid>("badgeType.adx_badgetypeid") == y.GetAttributeAliasedValue<Guid>("badgeType.adx_badgetypeid");
			}

			public int GetHashCode(Entity obj)
			{
				return obj.GetAttributeAliasedValue<Guid>("badgeType.adx_badgetypeid").GetHashCode();
			}
		}

		[HttpGet]
		[OutputCache(CacheProfile = "User")]
		public ActionResult GetBadges(string userid, string type)
		{
			var response = FetchBadges(userid);

			if (response == null)
			{
				return Content(string.Empty);
			}

			var entities = response.EntityCollection.Entities
				.Where(e => e.GetAttributeAliasedValue<DateTime?>("badge.adx_expirydate").GetValueOrDefault(DateTime.MaxValue) >= DateTime.UtcNow).Distinct(new BadgeComparer())
				.ToArray();

			if (!entities.Any())
			{
				return Content(string.Empty);
			}

			string badges;

			switch (type)
			{
				case "basic-badges":
					badges = BasicBadges(entities);
					break;
				case "profile-badges":
					badges = ProfileBadges(entities);
					break;
				default:
					badges = BasicBadges(entities);
					break;
			}

			return Content(badges);
		}

		private static RetrieveMultipleResponse FetchBadges(string contactid)
		{
			string parentAccountId = string.Empty;
			
			
			try
			{
				using (var context = PortalCrmConfigurationManager.CreateServiceContext())
				{
					var AccountQueryResult = context.Execute(new RetrieveRequest
					{
						ColumnSet = new ColumnSet("parentcustomerid"),
						Target = new EntityReference("contact", new Guid(contactid)),
					}); 
					if (AccountQueryResult.Results != null)
					{
						var contactParentAccount = ((Entity)AccountQueryResult.Results.First().Value).GetAttributeValue<EntityReference>("parentcustomerid");
						parentAccountId = contactParentAccount != null ? contactParentAccount.Id.ToString() : string.Empty;
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			var fetchXml = XDocument.Parse(@"
				<fetch mapping=""logical"">
					<entity name=""adx_badge"" >
						<attribute name=""adx_badgeid"" />
						<attribute name=""adx_expirydate"" />
						<filter type=""and"">
							<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							<filter type=""or"">
							</filter>
						</filter>
						<link-entity link-type=""outer"" name=""adx_badgetype"" from=""adx_badgetypeid"" to=""adx_badgetypeid"" alias=""badgeType"">
							<attribute name=""adx_badgetypeid"" />
							<attribute name=""adx_displaytext"" />
							<attribute name=""adx_description"" />
							<attribute name=""adx_iconname"" />
							<attribute name=""adx_backgroundcolor"" />
							<filter type=""and"">
								<condition attribute=""statecode"" operator=""eq"" value=""0"" />
							</filter>
						</link-entity>
					</entity>
				</fetch>");



			var filter = fetchXml.XPathSelectElement("//entity[@name='adx_badge']/filter/filter[@type='or']");

			filter.AddFetchXmlFilterCondition("adx_contactid", "eq", contactid);
			if (parentAccountId != string.Empty)
			{
				filter.AddFetchXmlFilterCondition("adx_accountid", "eq", parentAccountId);
			}
			
			try
			{
				using (var context = PortalCrmConfigurationManager.CreateServiceContext())
				{
					var response = (RetrieveMultipleResponse)context.Execute(new RetrieveMultipleRequest
					{
						Query = new FetchExpression(fetchXml.ToString())
					});

					return response;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		private string BasicBadges(Entity[] entities)
		{
			if (!entities.Any())
			{
				return string.Empty;
			}

			var container = new TagBuilder("div");

			container.AddCssClass("badges");

			var badgeContent = entities.Aggregate(new StringBuilder(), (sb, e) => sb.AppendLine(Badge(e))).ToString();

			if (string.IsNullOrWhiteSpace(badgeContent))
			{
				return string.Empty;
			}

			container.InnerHtml += badgeContent;

			return container.ToString();
		}

		private string ProfileBadges(Entity[] entities)
		{
			return BasicBadges(entities);
		}

		private string Badge(Entity entity)
		{
			var displayText = entity.GetAttributeAliasedValue<string>("badgeType.adx_displaytext");
			var iconClass = entity.GetAttributeAliasedValue<string>("badgeType.adx_iconname");
			var backgroundColor = entity.GetAttributeAliasedValue<string>("badgeType.adx_backgroundcolor");
			var description = entity.GetAttributeAliasedValue<string>("badgeType.adx_description");

			if (string.IsNullOrWhiteSpace(displayText))
			{
				return string.Empty;
			}

			var tag = new TagBuilder(string.IsNullOrWhiteSpace(description) ? "span" : "a");

			tag.AddCssClass("label label-default");
			tag.Attributes["title"] = displayText;

			if (!string.IsNullOrWhiteSpace(backgroundColor))
			{
				tag.Attributes["style"] = "background-color: {0};".FormatWith(backgroundColor);
			}

			if (!string.IsNullOrWhiteSpace(description))
			{
				tag.Attributes["role"] = "button";
				tag.Attributes["tabindex"] = "0";
				tag.Attributes["data-toggle"] = "popover";
				tag.Attributes["data-trigger"] = "focus";
				tag.Attributes["data-placement"] = "auto bottom";
				tag.Attributes["data-content"] = description;
			}

			if (!string.IsNullOrWhiteSpace(iconClass))
			{
				var icon = new TagBuilder("span");

				icon.AddCssClass("badge-icon");
				icon.AddCssClass(iconClass);

				icon.Attributes["aria-hidden"] = "true";

				tag.InnerHtml += icon + " ";
			}

			var title = new TagBuilder("span");

			title.AddCssClass("badge-title");
			title.SetInnerText(displayText);

			tag.InnerHtml += title.ToString();
			
			return tag.ToString();
		}
	}
}
