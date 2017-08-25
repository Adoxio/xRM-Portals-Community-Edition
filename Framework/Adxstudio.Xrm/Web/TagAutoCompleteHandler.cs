/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Events;
using Adxstudio.Xrm.Forums;
using Adxstudio.Xrm.Tagging;
using Microsoft.Xrm.Portal;

namespace Adxstudio.Xrm.Web
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class TagAutoCompleteHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public virtual int MaxResultsDefault
		{
			get { return 10; }
		}

		public virtual string MaxResultsQueryStringField
		{
			get { return "count"; }
		}

		public virtual string TagNameQueryStringField
		{
			get { return "q"; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var prefix = context.Request.QueryString[TagNameQueryStringField];

			if (string.IsNullOrEmpty(prefix))
			{
				return;
			}

			int maxResults = int.TryParse(context.Request.QueryString[MaxResultsQueryStringField], out maxResults)
				? maxResults
				: MaxResultsDefault;

			var completions = GetTagNameCompletions(prefix);
			
			if  (completions.Count() > maxResults)
			{
				completions = completions.Take(maxResults);
			}
					
			foreach (var completion in completions)
			{
				context.Response.Write(completion + Environment.NewLine);
			}
		}

		protected virtual IEnumerable<string> GetTagNameCompletions(string tagNamePrefix)
		{
			var context = PortalContext.Current.ServiceContext;

			var pageTagCompletions =
				from pt in context.GetPageTags().ToList()
				let pti = new PageTagInfo(pt)
				where pti.Name.StartsWith(tagNamePrefix, TagName.Comparison)
				select pti.Name;

			var forumThreadTagCompletions =
				from ftt in context.GetForumThreadTags().ToList()
				let ftti = new ForumThreadTagInfo(ftt)
				where ftti.Name.StartsWith(tagNamePrefix, TagName.Comparison)
				select ftti.Name;

			var eventTagCompletions =
				from et in context.GetEventTags()
				let name = et.GetAttributeValue<string>("adx_name")
				where name.StartsWith(tagNamePrefix, TagName.Comparison)
				select name;

			var comparer = new TagNameComparer();

			return pageTagCompletions
				.Union(eventTagCompletions, comparer)
				.Union(forumThreadTagCompletions, comparer);
		}
	}
}
