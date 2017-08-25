/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Services;
using Microsoft.Xrm.Client.Configuration;

namespace Adxstudio.Xrm.Web
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class TimeTagAutoCompleteHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public virtual string ContextName { get; set; }

		public virtual int MaxResultsDefault
		{
			get { return 25; }
		}

		public virtual string MaxResultsQueryStringField
		{
			get { return "limit"; }
		}

		public virtual string TagNameQueryStringField
		{
			get { return "q"; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "application/json";
		
			var prefix = context.Request.QueryString[TagNameQueryStringField];

			int maxResults = int.TryParse(context.Request.QueryString[MaxResultsQueryStringField], out maxResults)
				? maxResults
				: MaxResultsDefault;

			var results = string.IsNullOrEmpty(prefix) ? GetTimeTagNameSuggestions(maxResults) : GetTimeTagNameCompletions(prefix, maxResults);

			var ser = new DataContractJsonSerializer(results.GetType());

			ser.WriteObject(context.Response.OutputStream, results); 
		}

		protected virtual List<TimeTagItem> GetTimeTagNameCompletions(string tagNamePrefix, int max)
		{
			var context = CrmConfigurationManager.CreateContext(ContextName);

			var timeTags = context.CreateQuery("adx_psa_timetag").Where(t => t.GetAttributeValue<string>("adx_name").StartsWith(tagNamePrefix, StringComparison.InvariantCultureIgnoreCase)).OrderBy(t => t.GetAttributeValue<string>("adx_name")).Take(max);
						  
			var tagsList = new List<TimeTagItem>();

			foreach (var tag in timeTags)
			{
				tagsList.Add(new TimeTagItem { Id = tag.GetAttributeValue<Guid>("adx_psa_timetagid").ToString(), Name = tag.GetAttributeValue<string>("adx_name") });
			}

			return tagsList;
		}

		protected virtual List<TimeTagItem> GetTimeTagNameSuggestions(int max)
		{
			var context = CrmConfigurationManager.CreateContext(ContextName);

			var timeTags = context.CreateQuery("adx_psa_timetag").Where(t => t.GetAttributeValue<bool>("adx_system")).OrderBy(t => t.GetAttributeValue<string>("adx_name")).Take(max);

			var tagsList = new List<TimeTagItem>();

			foreach (var tag in timeTags)
			{
				tagsList.Add(new TimeTagItem { Id = tag.GetAttributeValue<Guid>("adx_psa_timetagid").ToString(), Name = tag.GetAttributeValue<string>("adx_name") });
			}

			return tagsList;
		}
	}

	[DataContract]
	public class TimeTagItem
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Id { get; set; }
	}
}
