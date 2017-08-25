/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web.Mvc;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class AuthorDrop : PortalUrlDrop
	{
		private readonly Lazy<string> _basicBadgesUrl;
		private readonly Lazy<string> _imageUrl;
		private readonly Lazy<string> _profileBadgesUrl;
		private readonly Lazy<string> _url;
		
		public AuthorDrop(IPortalLiquidContext portalLiquidContext, string name, string email, Guid id, Func<UrlHelper, string> getUrl)
			: base(portalLiquidContext)
		{
			if (getUrl == null) throw new ArgumentNullException("getUrl");

			Name = name;
			Email = email;
			UserId = id;

			_imageUrl = new Lazy<string>(GetImageUrl, LazyThreadSafetyMode.None);
			_url = new Lazy<string>(() => getUrl(UrlHelper), LazyThreadSafetyMode.None);

			_basicBadgesUrl = new Lazy<string>(GetBasicBadgesUrl, LazyThreadSafetyMode.None);
			_profileBadgesUrl = new Lazy<string>(GetProfileBadgesUrl, LazyThreadSafetyMode.None);
		}
		
		public AuthorDrop(IPortalLiquidContext portalLiquidContext, IBlogAuthor author)
			: this(
				portalLiquidContext,
				author.Name,
				author.EmailAddress,
				author.Id,
				url => url.RouteUrl("PublicProfileBlogPosts", new { contactId = author.Id }))
		{
			if (author == null) throw new ArgumentNullException("author");
		}

		public AuthorDrop(IPortalLiquidContext portalLiquidContext, IForumAuthor author)
			: this(
				portalLiquidContext,
				author.DisplayName,
				author.EmailAddress,
				author.EntityReference.Id,
				url => url.RouteUrl("PublicProfileForumPosts", new { contactId = author.EntityReference.Id }))
		{
			if (author == null) throw new ArgumentNullException("author");
		}

		public string ImageUrl
		{
			get { return _imageUrl.Value; }
		}

		public string BasicBadgesURL
		{
			get { return _basicBadgesUrl.Value; }
		}

		public string ProfileBadgesURL
		{
			get { return _profileBadgesUrl.Value; }
		}

		public string Name { get; private set; }

		public string Email { get; private set; }

		public Guid UserId { get; private set; }

		public string EmailAddress
		{
			get { return Email; }
		}

		public override string Url
		{
			get { return _url.Value; }
		}

		private string GetImageUrl()
		{
			return GetUserImageUrl(Email);
		}

		private string GetBasicBadgesUrl()
		{
			return UrlHelper.RouteUrl("PortalBadges", new
			{
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id,
				userId = UserId,
				type = "basic-badges"
			});
		}

		private string GetProfileBadgesUrl()
		{
			return UrlHelper.RouteUrl("PortalBadges", new
			{
				__portalScopeId__ = PortalViewContext.Website.EntityReference.Id,
				userId = UserId,
				type = "profile-badges"
			});
		}
	}
}
