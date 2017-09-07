/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.AspNet.Mvc;
using Adxstudio.Xrm.Blogs;
using Adxstudio.Xrm.Cases;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Ideas;
using Adxstudio.Xrm.Issues;
using Adxstudio.Xrm.Web;
using Microsoft.Xrm.Sdk;

namespace Site.Helpers
{
	public static class UrlHelpers
	{
		public static string ActionWithQueryString(this UrlHelper url, string actionName, object routeValues)
		{
			var routeDictionary = new RouteValueDictionary(routeValues);

			var queryString = url.RequestContext.HttpContext.Request.QueryString;

			foreach (var key in queryString.Cast<string>().Where(key => !routeDictionary.ContainsKey(key) && !string.IsNullOrWhiteSpace(queryString[key])))
			{
				routeDictionary[key] = queryString[key];
			}

			return url.Action(actionName, routeDictionary);
		}

		public static bool RegistrationEnabled(this UrlHelper url)
		{
			var settings = GetAuthenticationSettings(url);

			return settings.RegistrationEnabled && !Adxstudio.Xrm.Configuration.PortalSettings.Instance.Ess.IsEss;
		}

		public static string SignInUrl(this UrlHelper url, string returnUrl = null)
		{
			return SignInUrlOwin(url, returnUrl);
		}

		private static string SignInUrlOwin(this UrlHelper url, string returnUrl = null)
		{
			var settings = GetAuthenticationSettings(url);

			return !string.IsNullOrWhiteSpace(settings.LoginButtonAuthenticationType)
				? url.Action("ExternalLogin", "Login",
					new
					{
						area = "Account",
						region = settings.Region,
						returnUrl = GetReturnUrl(url.RequestContext.HttpContext.Request, returnUrl),
						provider = settings.LoginButtonAuthenticationType
					})
				: LocalSignInUrl(url, returnUrl);
		}

		public static string LocalSignInUrl(this UrlHelper url, string returnUrl = null)
		{
			return GetAccountUrl(url, "Login", "Login", returnUrl);
		}

		public static string FacebookSignInUrl(this UrlHelper url)
		{
			return url.Action("FacebookExternalLogin", "Login", new { area = "Account" });
		}

		public static string SignOutUrl(this UrlHelper url, string returnUrl = null)
		{
			return GetAccountUrl(url, "LogOff", "Login", returnUrl);
		}

		public static string RegisterUrl(this UrlHelper url, string returnUrl = null)
		{
			return GetAccountUrl(url, "Register", "Login", returnUrl);
		}

		public static string RedeemUrl(this UrlHelper url, string returnUrl = null)
		{
			return GetAccountUrl(url, "RedeemInvitation", "Login", returnUrl);
		}

		private static string GetAccountUrl(UrlHelper url, string actionName, string controllerName, string returnUrl)
		{
			var settings = GetAuthenticationSettings(url);
			var routeValues = new
			{
				area = "Account",
				region = settings.Region,
				returnUrl = GetReturnUrl(url.RequestContext.HttpContext.Request, returnUrl)
			};

			return url.Action(actionName, controllerName, routeValues);
		}

		private static string GetReturnUrl(HttpRequestBase request, string returnUrl)
		{
			return request["ReturnUrl"] ?? returnUrl ?? request.RawUrl;
		}

		public static string SecureRegistrationUrl(this UrlHelper url, string returnUrl = null, string invitationCode = null)
		{
			return GetAccountRegistrationUrl(url, "Register", returnUrl, invitationCode);
		}

		private static string GetAccountRegistrationUrl(UrlHelper url, string actionName, string returnUrl, string invitationCode)
		{
			var returnLocalUrl = GetReturnUrl(url.RequestContext.HttpContext.Request, returnUrl);
			return url.RouteUrl(actionName, new { returnUrl = returnLocalUrl, invitationCode });
		}

		private static AuthenticationSettings GetAuthenticationSettings(this UrlHelper url)
		{
			var website = url.RequestContext.HttpContext.GetWebsite();
			var settings = website.GetAuthenticationSettings();
			var contextLanguageInfo = url.RequestContext.HttpContext.GetContextLanguageInfo();
			var region = (contextLanguageInfo.IsCrmMultiLanguageEnabled && ContextLanguageInfo.DisplayLanguageCodeInUrl)
				? contextLanguageInfo.ContextLanguage.Code : null;

			return new AuthenticationSettings
			{
				RegistrationEnabled = settings.RegistrationEnabled && settings.OpenRegistrationEnabled,
				LoginButtonAuthenticationType = settings.LoginButtonAuthenticationType,
				Region = region
			};
		}

		private class AuthenticationSettings
		{
			public bool RegistrationEnabled { get; set; }
			public string LoginButtonAuthenticationType { get; set; }
			public string Region { get; set; }
		}

		private const string _defaultAuthorUrl = null;

		public static string AuthorUrl(this UrlHelper urlHelper, IBlogAuthor author)
		{
			try
			{
				return author == null
						   ? _defaultAuthorUrl
						   : urlHelper.RouteUrl("PublicProfileBlogPosts", new { contactId = author.Id });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string AuthorUrl(this UrlHelper urlHelper, ICase @case)
		{
			if (@case == null || @case.ResponsibleContact == null)
			{
				return _defaultAuthorUrl;
			}

			try
			{
				return urlHelper.RouteUrl("PublicProfileForumPosts", new { contactId = @case.ResponsibleContact.Id });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string AuthorUrl(this UrlHelper urlHelper, IComment comment)
		{
			if (comment == null || comment.Author == null)
			{
				return _defaultAuthorUrl;
			}

			try
			{
				return comment.Author.EntityReference == null
						   ? comment.Author.WebsiteUrl
					: urlHelper.RouteUrl("PublicProfileForumPosts", new { contactId = comment.Author.EntityReference.Id });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string AuthorUrl(this UrlHelper urlHelper, IIdea idea)
		{
			try
			{
				return idea == null || idea.AuthorId == null
					? _defaultAuthorUrl
					: urlHelper.RouteUrl("PublicProfileIdeas", new { contactId = idea.AuthorId.Value });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string AuthorUrl(this UrlHelper urlHelper, IIssue issue)
		{
			try
			{
				return issue == null || issue.AuthorId == null
				 ? _defaultAuthorUrl
				 : urlHelper.RouteUrl("PublicProfileForumPosts", new { contactId = issue.AuthorId.Value });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string AuthorUrl(this UrlHelper urlHelper, IForumAuthor author)
		{
			try
			{
				return author == null || author.EntityReference == null
						   ? _defaultAuthorUrl
						   : urlHelper.RouteUrl("PublicProfileForumPosts", new { contactId = author.EntityReference.Id });
			}
			catch
			{
				return _defaultAuthorUrl;
			}
		}

		public static string UserImageUrl(this UrlHelper urlHelper, IForumAuthor author, int? size = null)
		{
			return author == null ? null : urlHelper.UserImageUrl(author.EmailAddress, size);
		}

		public static string UserImageUrl(this UrlHelper urlHelper, ICase @case, int? size = null)
		{
			return @case == null || string.IsNullOrEmpty(@case.ResponsibleContactEmailAddress)
				? null
				: urlHelper.UserImageUrl(@case.ResponsibleContactEmailAddress, size);
		}

		public static string UserImageUrl(this UrlHelper urlHelper, Entity contact, int? size = null)
		{
			return contact == null ? null : urlHelper.UserImageUrl(contact.GetAttributeValue<string>("emailaddress1"), size);
		}

		public static string UserImageUrl(this UrlHelper urlHelper, object email, int? size = null)
		{
			return email == null ? null : urlHelper.UserImageUrl(email.ToString(), size);
		}

		public static string UserImageUrl(this UrlHelper urlHelper, string email, int? size = null)
		{
			return VirtualPathUtility.ToAbsolute("~/xrm-adx/images/contact_photo.png");
		}
	}
}
