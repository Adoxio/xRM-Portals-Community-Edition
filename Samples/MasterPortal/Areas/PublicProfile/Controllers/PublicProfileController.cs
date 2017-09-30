/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Ideas;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Site.Areas.PublicProfile.ViewModels;

namespace Site.Areas.PublicProfile.Controllers
{
	[PortalView, PortalSecurity]
	public class PublicProfileController : Controller
	{
		[HttpGet]
		public ActionResult ProfileBlogPosts(string contactId, int? page)
		{
			Guid guid;

			if (!Guid.TryParse(contactId, out guid))
			{
				return HttpNotFound();
			}

			var service = this.HttpContext.GetOrganizationService();
			var contact = service.RetrieveSingle(CreateContactQuery(guid));

			if (contact == null)
			{
				return HttpNotFound();
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			var dependencies = new Adxstudio.Xrm.Blogs.PortalContextDataAdapterDependencies(portal, requestContext: Request.RequestContext);
			var blogDataAdapter = new AuthorWebsiteBlogAggregationDataAdapter(guid, dependencies);
			var website = this.HttpContext.GetWebsite();

			var profileViewModel = new ProfileViewModel
			{
				BlogCount = blogDataAdapter.SelectBlogCount(),
				User = contact,
				Website = website.Entity
			};

			EnityEnablePermission(profileViewModel);
			profileViewModel.BlogPosts = new PaginatedList<IBlogPost>(page, profileViewModel.BlogCount,
				blogDataAdapter.SelectPosts);

			return View(profileViewModel);
		}

		[HttpGet]
		public ActionResult ProfileIdeas(string contactId, int? page)
		{
			Guid guid;

			if (!Guid.TryParse(contactId, out guid))
			{
				return HttpNotFound();
			}

			var service = Request.GetOwinContext().GetOrganizationService();
			var contact = service.RetrieveSingle(CreateContactQuery(guid));

			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			if (contact == null)
			{
				return HttpNotFound();
			}

			var ideaDataAdapter = new WebsiteIdeaUserAggregationDataAdapter(guid);

			var profileViewModel = new ProfileViewModel
			{
				IdeaCount = ideaDataAdapter.SelectIdeaCount(),
				User = contact,
				Website = portal.Website
			};

			EnityEnablePermission(profileViewModel);
			profileViewModel.Ideas = new PaginatedList<IIdea>(page, profileViewModel.IdeaCount, ideaDataAdapter.SelectIdeas);

			return View(profileViewModel);
		}

		[HttpGet]
		public ActionResult ProfileForumPosts(string contactId, int? page)
		{
			Guid guid;

			if (!Guid.TryParse(contactId, out guid))
			{
				return HttpNotFound();
			}

			var service = Request.GetOwinContext().GetOrganizationService();
			var contact = service.RetrieveSingle(CreateContactQuery(guid));

			if (contact == null)
			{
				return HttpNotFound();
			}

			var portal = PortalCrmConfigurationManager.CreatePortalContext();

			var forumDataAdapter =
				new WebsiteForumPostUserAggregationDataAdapter(guid,
					new Adxstudio.Xrm.Forums.PortalContextDataAdapterDependencies(portal));

			var profileViewModel = new ProfileViewModel
			{
				ForumPostCount = forumDataAdapter.SelectPostCount(),
				User = contact,
				Website = portal.Website
			};

			EnityEnablePermission(profileViewModel);
			profileViewModel.ForumPosts = new PaginatedList<IForumPost>(page, profileViewModel.ForumPostCount,
				forumDataAdapter.SelectPostsDescending);

			return View(profileViewModel);
		}

		[HttpGet]
		public ActionResult ProfileRedirect(string contactId)
		{
			return new RedirectToSiteMarkerResult("Profile");
		}

		private void EnityEnablePermission(ProfileViewModel profileViewModel)
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext(null);
			var response = (RetrieveMetadataChangesResponse)serviceContext.Execute(new RetrieveMetadataChangesRequest
			{
				Query = new EntityQueryExpression
				{
					Properties = new MetadataPropertiesExpression("LogicalName"),
				}
			});

			var allEntities = response.EntityMetadata.ToDictionary(e => e.LogicalName, StringComparer.OrdinalIgnoreCase);

			if (profileViewModel != null)
			{
				profileViewModel.IsForumPostsEnable = allEntities.ContainsKey("adx_communityforumpost");
				profileViewModel.IsBlogPostsEnable = allEntities.ContainsKey("adx_blog");
				profileViewModel.IsIdeasEnable = allEntities.ContainsKey("adx_idea");
			}
		}

		private Fetch CreateContactQuery(Guid contactId)
		{
			var fetchExpression = new Fetch
			{
				Entity = new FetchEntity("contact")
				{
					Attributes = new List<FetchAttribute>
					{
						new FetchAttribute("firstname"),
						new FetchAttribute("lastname"),
						new FetchAttribute("adx_organizationname"),
						new FetchAttribute("websiteurl"),
						new FetchAttribute("adx_publicprofilecopy"),
						new FetchAttribute("createdon"),
						new FetchAttribute("emailaddress1")
					},
					Filters = new List<Adxstudio.Xrm.Services.Query.Filter>
					{
						new Adxstudio.Xrm.Services.Query.Filter
						{
							Conditions = new[] {
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("contactid", ConditionOperator.Equal, contactId)
							}
						}
					}
				}
			};

			return fetchExpression;
		}
	}
}
