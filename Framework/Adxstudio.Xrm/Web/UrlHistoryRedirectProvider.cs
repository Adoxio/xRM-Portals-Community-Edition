/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Services.Query;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// An <see cref="IRedirectProvider"/> which implements its <see cref="Match"/> based on adx_urlhistory entities
	/// associated with a given adx_website.
	/// </summary>
	public class UrlHistoryRedirectProvider : IRedirectProvider
	{
		private static readonly string[] _homePaths = new[] { "~", "/", "~/" };
		public string PortalName { get; private set; }

		public UrlHistoryRedirectProvider(string portalName)
		{
			PortalName = portalName;
		}

		protected OrganizationServiceContext CreateContext()
		{
			return PortalCrmConfigurationManager.CreateServiceContext(PortalName);
		}

		public IRedirectMatch Match(Guid websiteID, UrlBuilder url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url");
			}

			using (var context = CreateContext())
			{
				var website = context.RetrieveSingle(
					"adx_website", 
					FetchAttribute.All, 
					new[] {
						new Condition("statecode", ConditionOperator.Equal, 0),
						new Condition("adx_websiteid", ConditionOperator.Equal, websiteID) },
					true);

				if (website == null)
				{
					return new FailedRedirectMatch();
				}

				var appRelativePath = VirtualPathUtility.ToAppRelative(url.Path);
				var match = FindPage(context, website, appRelativePath);

				// Matches must have actually used the URL history (not be live currently) to count as a match.
				if (match.Success && match.UsedHistory)
				{
					return new UrlHistoryMatch(context.GetUrl(match.WebPage));
				}

				return new FailedRedirectMatch();
			}
		}

		private static Entity FindHistoryMatchPage(OrganizationServiceContext context, Entity website, string path)
		{
			var fetch = new Fetch
			{
				Entity = new FetchEntity("adx_urlhistory")
				{
					Attributes = new[]
					{
						new FetchAttribute("adx_webpageid")
					},
					Filters = new List<Filter>
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_websiteid", ConditionOperator.Equal, website.Id),
								new Condition("adx_name", ConditionOperator.Equal, path)
							}
						}
					},
					Orders = new[]
					{
						new Order("adx_changeddate", OrderType.Descending)
					}
				}
			};

			var historyMatch = context.RetrieveSingle(fetch);

			if (historyMatch == null)
			{
				return null;
			}

			var historyWebPageReference = historyMatch.GetAttributeValue<EntityReference>("adx_webpageid");

			if (historyWebPageReference == null)
			{
				return null;
			}

			fetch = new Fetch
			{
				Entity = new FetchEntity("adx_webpage")
				{
					Filters = new List<Filter>
					{
						new Filter
						{
							Conditions = new[]
							{
								new Condition("statecode", ConditionOperator.Equal, 0),
								new Condition("adx_webpageid", ConditionOperator.Equal, historyWebPageReference.Id)
							}
						}
					}
				}
			};

			return context.RetrieveSingle(fetch);
		}

		private static IMatch FindPage(OrganizationServiceContext context, Entity website, string path)
		{
			var mappedPageResult = UrlMapping.LookupPageByUrlPath(context, website, path);

			if (mappedPageResult.Node != null)
			{
				return new NonHistoryMatch(mappedPageResult.Node);
			}

			var historyMatchPage = FindHistoryMatchPage(context, website, path);

			if (historyMatchPage != null)
			{
				return new HistoryMatch(historyMatchPage);
			}

			// find the last slash in the path
			var lastSlash = path.LastIndexOf('/');

			if (lastSlash != -1)
			{
				// we found a slash
				var pathBeforeSlash = path.Substring(0, lastSlash);
				var pathAfterSlash = path.Substring(lastSlash + 1);

				var parentMatch = (string.IsNullOrWhiteSpace(pathBeforeSlash) || _homePaths.Contains(pathBeforeSlash))
					// do a final traversal against the home page
					? GetHomePage(context, website)
					// see if we can find a path for the parent url
					: FindPage(context, website, pathBeforeSlash);

				if (parentMatch.Success)
				{
					var foundParentPageID = parentMatch.WebPage.ToEntityReference();

					var fetch = new Fetch
					{
						Entity = new FetchEntity("adx_webpage")
						{
							Filters = new List<Filter>
							{
								new Filter
								{
									Conditions = new[]
									{
										new Condition("statecode", ConditionOperator.Equal, 0),
										new Condition("adx_parentpageid", ConditionOperator.Equal, foundParentPageID.Id),
										new Condition("adx_partialurl", ConditionOperator.Equal, pathAfterSlash)
									}
								}
							}
						}
					};

					// we found a parent path, now rebuild the path of the child
					var child = context.RetrieveSingle(fetch);

					if (child != null)
					{
						return new SuccessfulMatch(child, parentMatch.UsedHistory);
					}

					var parentPath = context.GetApplicationPath(parentMatch.WebPage);

					if (parentPath == null)
					{
						return new FailedMatch();
					}

					if (parentPath.AppRelativePath.TrimEnd('/') == path)
					{
						// prevent stack overflow
						return new FailedMatch();
					}

					var newPath = parentPath.AppRelativePath + (parentPath.AppRelativePath.EndsWith("/") ? string.Empty : "/") + pathAfterSlash;

					if (newPath == path)
					{
						return new FailedMatch();
					}

					var childMatch = FindPage(context, website, newPath);

					if (childMatch.Success)
					{
						return new HistoryMatch(childMatch.WebPage);
					}
				}
			}

			return new FailedMatch();
		}

		private static IMatch GetHomePage(OrganizationServiceContext context, Entity website)
		{
			var homePage = context.GetPageBySiteMarkerName(website, "Home");

			return homePage == null
				? (IMatch)new FailedMatch()
				: new NonHistoryMatch(homePage);
		}

		private interface IMatch
		{
			bool Success { get; }

			bool UsedHistory { get; }

			Entity WebPage { get; }
		}

		private class FailedMatch : IMatch
		{
			public bool Success
			{
				get { return false; }
			}

			public bool UsedHistory
			{
				get { return false; }
			}

			public Entity WebPage
			{
				get { return null; }
			}
		}

		private class SuccessfulMatch : IMatch
		{
			public SuccessfulMatch(Entity webPage, bool usedHistory)
			{
				if (webPage == null)
				{
					throw new ArgumentNullException("webPage");
				}

				WebPage = webPage;
				UsedHistory = usedHistory;
			}

			public bool Success
			{
				get { return true; }
			}

			public bool UsedHistory { get; private set; }

			public Entity WebPage { get; private set; }
		}

		private class HistoryMatch : SuccessfulMatch
		{
			public HistoryMatch(Entity webPage) : base(webPage, true) { }
		}

		private class NonHistoryMatch : SuccessfulMatch
		{
			public NonHistoryMatch(Entity webPage) : base(webPage, false) { }
		}
	}
}
