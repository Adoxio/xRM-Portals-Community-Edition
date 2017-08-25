/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Routing;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Text;
using Microsoft.Owin;
using Microsoft.Xrm.Client;
using Adxstudio.Xrm.Configuration;

namespace Adxstudio.Xrm.AspNet.Cms
{
	public class WebsiteManager<TWebsite, TKey> : EntityManager<IWebsiteStore<TWebsite, TKey>, TWebsite, TKey>
		where TWebsite : CrmWebsite<TKey>
		where TKey : IEquatable<TKey>
	{
		public string WebsiteName { get; set; }

		public WebsiteManager(IWebsiteStore<TWebsite, TKey> store)
			: base(store)
		{
		}

		public virtual TWebsite Find(IOwinContext owinContext, PortalHostingEnvironment environment)
		{
			ThrowIfDisposed();

			return FindAsync(owinContext, environment).Result;
		}

		public virtual Task<TWebsite> FindAsync(IOwinContext owinContext, PortalHostingEnvironment environment)
		{
			ThrowIfDisposed();

			var request = owinContext != null
				? owinContext.Get<RequestContext>(typeof(RequestContext).FullName)
				: null;

			return FindAsync(request, environment);
		}

		public virtual TWebsite Find(RequestContext request, PortalHostingEnvironment environment)
		{
			ThrowIfDisposed();

			return FindAsync(request, environment).Result;
		}

		public virtual async Task<TWebsite> FindAsync(RequestContext request, PortalHostingEnvironment environment)
		{
			ThrowIfDisposed();

			// Attempt to match website by a special route parameter. Use by the front-side editing services to target the
			// correct website, in the absense of website URL paths.
			Guid portalScopeId;

			if (request != null && Guid.TryParse(request.RouteData.Values["__portalScopeId__"] as string, out portalScopeId))
			{
				return await Store.FindByIdAsync(ToKey(portalScopeId)).WithCurrentCulture();
			}

			// retrieve all websites in list format only containing the related website bindings

			var websites = Websites.ToList();

			if (!string.IsNullOrWhiteSpace(WebsiteName))
			{
				var website = await FindByNameAsync(WebsiteName).WithCurrentCulture();

				if (website == null)
				{
					CmsEventSource.Log.WebsiteBindingNotFoundByWebsiteName(WebsiteName);

					throw new ApplicationException("Unable to find a unique and active website with the name {0}.".FormatWith(WebsiteName));
				}

				return website;
			}
			else
			{
				var website = GetWebsiteByBinding(environment, websites) ?? GetWebsiteByAppSettingAndCreateBinding(environment, websites);

				if (website == null)
				{
					CmsEventSource.Log.WebsiteBindingNotFoundByHostingEnvironment(environment);

					throw new ApplicationException("Unable to find a unique and active website binding for the IIS site named {0} with a virtual path named {1}.".FormatWith(environment.SiteName, environment.ApplicationVirtualPath));
				}

				return await ExpandWebsite(website).WithCurrentCulture();
			}
		}

		public virtual IQueryable<TWebsite> Websites
		{
			get
			{
				var queryableStore = Store as IQueryableWebsiteStore<TWebsite, TKey>;

				if (queryableStore == null)
				{
					throw new NotSupportedException();
				}

				return queryableStore.Websites;
			}
		}

		protected string GetPath(RequestContext request)
		{
			if (request != null) return request.HttpContext.Request.RawUrl;

			return null;
		}

		protected virtual TWebsite GetWebsiteByBinding(PortalHostingEnvironment environment, IEnumerable<TWebsite> websites)
		{
			return GetWebsitesByBinding(environment, websites).SingleOrDefault();
		}

		protected virtual IEnumerable<TWebsite> GetWebsitesByBinding(PortalHostingEnvironment environment, IEnumerable<TWebsite> websites)
		{
			if (string.IsNullOrWhiteSpace(environment.SiteName)) yield break;

			foreach (var website in websites)
			{
				foreach (var binding in website.Bindings)
				{
					if (MatchDates(binding)
						&& MatchSiteName(binding.SiteName, environment.SiteName)
						&& MatchVirtualPath(binding.VirtualPath, environment.ApplicationVirtualPath))
					{
						// accept this website

						yield return website;
						break;
					}
				}
			}
		}

		private static bool _websiteByAppSettingAndCreateBinding;

		private TWebsite GetWebsiteByAppSettingAndCreateBinding(PortalHostingEnvironment environment, IEnumerable<TWebsite> websites)
		{
			if (!PortalSettings.Instance.UseOnlineSetup)
			{
				// PortalOnlineSetup app setting is not set to true, return null and do not create binding.
				return null;
			}

			var portalWebsiteId = ConfigurationManager.AppSettings["PortalPackageName"];

			Guid websiteId;

			if (!Guid.TryParse(portalWebsiteId, out websiteId))
			{
				// A valid guid was not found in the app setting.
				return null;
			}

			var website = websites.FirstOrDefault(site => site.Id.ToGuid() == websiteId);

			if (website == null)
			{
				// No website found with the id from the app setting.
				return null;
			}

			if (!_websiteByAppSettingAndCreateBinding)
			{
				_websiteByAppSettingAndCreateBinding = true;

				var bindingId = CreateWebsiteBindingAsync(environment, website).Result;
			}

			return website;
		}

		private async Task<TKey> CreateWebsiteBindingAsync(PortalHostingEnvironment environment, TWebsite tempWebsite)
		{
			var website = await this.ExpandWebsite(tempWebsite);

			// check if the binding already exists

			var existingBinding = website.Bindings
				.FirstOrDefault(binding => MatchSiteName(binding.SiteName, environment.SiteName) && MatchVirtualPath(binding.VirtualPath, environment.ApplicationVirtualPath));

			if (existingBinding != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, "A matching website binding already exists. Skip binding creation.");

				return this.ToKey(existingBinding.Entity.Id);
		}

			// binding does not exist

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Creating new website binding.");

			var newBinding = website.AddWebsiteBinding(environment);

			await this.Store.UpdateAsync(website);
			await this.RemovePotentialDuplicateBindingAsync(environment, website, newBinding);

			return this.ToKey(newBinding.Entity.Id);
		}

		private async Task<TWebsite> ExpandWebsite(TWebsite website)
		{
			// retrieve the fully expanded website given a website in list format

			return await Store.FindByIdAsync(website.Id).WithCurrentCulture();
		}

		private IWebsiteStore<TWebsite, TKey> GetWebsiteStore()
		{
			var cast = Store as IWebsiteStore<TWebsite, TKey>;
			if (cast == null)
			{
				throw new NotSupportedException("Store does not implement IWebsiteStore<TWebsite>.");
			}
			return cast;
		}

		private static bool MatchDates(CrmWebsiteBinding binding)
		{
			var now = DateTime.UtcNow;
			return binding.ReleaseDate.GetValueOrDefault(DateTime.MinValue) < now && binding.ExpirationDate.GetValueOrDefault(DateTime.MaxValue) > now;
		}

		private static bool MatchSiteName(string bindingSiteName, string siteName)
		{
			// match a binding by site name and virtual path

			// Azure Compute Emulator uses a format containing the role instance id and solution name
			// example: deployment18(9).MasterPortalAzure.MasterPortal_IN_1_Adxstudio Portals

			// Azure proper uses a format containing the role instance id but without the solution prefix
			// example: MasterPortal_IN_1_Adxstudio Portals

			// Cassini may append a numeric suffix which should be filtered out
			// example: CommunityPortal(1)

			if (string.IsNullOrWhiteSpace(bindingSiteName)) return false;

			var match = siteNamePattern.Match(siteName);
			var filteredSiteName = match.Success && match.Groups["site"].Success ? match.Groups["site"].Value : siteName;
			var mask = new Mask(bindingSiteName, RegexOptions.IgnoreCase);

			return mask.IsMatch(filteredSiteName);
		}

		private static readonly Regex siteNamePattern = new Regex(@"^.+_IN_\d+_(?<site>.+)$|^(?<site>[^\(\)]+)\(\d+\)$", RegexOptions.IgnoreCase);

		private static bool MatchVirtualPath(string bindingVirtualPath, string virtualPath)
		{
			if (string.IsNullOrWhiteSpace(bindingVirtualPath))
			{
				// accept an empty value for the root application

				return virtualPath == "/";
			}

			var maskVirtualPath = bindingVirtualPath.StartsWith("/") ? bindingVirtualPath : "/" + bindingVirtualPath;
			var mask = new Mask(maskVirtualPath, RegexOptions.IgnoreCase);

			return mask.IsMatch(virtualPath);
		}

		private async Task RemovePotentialDuplicateBindingAsync(PortalHostingEnvironment environment, TWebsite tempWebsite, CrmWebsiteBinding newBinding)
		{
			// Wait for a random time span between 400ms and 800ms.
			var random = new Random();
			var milliseconds = random.Next(400, 800);
			Thread.Sleep(milliseconds);

			// Query for the website bindings to see if a duplicate exists.
			var website = await this.ExpandWebsite(tempWebsite);
			var bindings = website.Bindings
				.Where(binding => MatchSiteName(binding.SiteName, environment.SiteName) && MatchVirtualPath(binding.VirtualPath, environment.ApplicationVirtualPath))
				.ToList();

			if (bindings.Count > 1)
			{
				// Remove the binding if it was created by this thread.
				if (tempWebsite.RemoveWebsiteBinding(newBinding))
				{
					await this.Store.UpdateAsync(tempWebsite);

					ADXTrace.Instance.TraceWarning(TraceCategory.Application, "Duplicate website binding detected and deleted.");
				}
			}
		}
	}
}
