/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Handlers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Caching;
	using System.ServiceModel.Syndication;
	using System.Web;
	using System.Xml;
	using Adxstudio.Xrm.Caching;
	using Adxstudio.Xrm.Configuration;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Caching;
	using Microsoft.Xrm.Client.Configuration;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Renders a feed of <see cref="ObjectCache"/> cache items.
	/// </summary>
	/// <remarks>
	/// The configuration with default values.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <configSections>
	///   <section name="adxstudio.xrm" type="Adxstudio.Xrm.Configuration.CrmSection, Adxstudio.Xrm"/>
	///  </configSections>
	/// 
	///  <system.webServer>
	///   <handlers>
	///    <add name="CacheFeed" verb="*" path="CacheFeed.axd" type="Adxstudio.Xrm.Web.Handlers.CacheFeedHandler, Adxstudio.Xrm"/>
	///   </handlers>
	///  </system.webServer>
	///  
	///  <adxstudio.xrm>
	///   <cacheFeed enabled="false" localOnly="true" objectCacheName="" showValues="true" traced="true"/>
	///  </adxstudio.xrm>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="AdxstudioCrmConfigurationManager"/>
	/// <seealso cref="CacheFeedElement"/>
	public class CacheFeedHandler : IHttpHandler
	{
		/// <summary>
		/// Possible content types of the cache feed output.
		/// </summary>
		private enum ContentType
		{
			Json, Xml
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public static class QueryKeys
		{
			/// <summary>
			/// Whether to show the cache footprint view with entity details. "true", default value = "false".
			/// </summary>
			public const string CacheFootprint = "cacheFootprint";

			/// <summary>
			/// Content type of the output. "xml", default value = "json".
			/// </summary>
			public const string ContentType = "contentType";

			/// <summary>
			/// Whether to show expanded detailed data. "true", default value = "false".
			/// </summary>
			public const string Expanded = "expanded";
		}

		public void ProcessRequest(HttpContext context)
		{
			var section = AdxstudioCrmConfigurationManager.GetCrmSection();
			var config = section.CacheFeed;

			if (!config.Enabled)
			{
				throw new HttpException("Feed isn't enabled.");
			}

			if (config.LocalOnly && !IsLocal(context.Request))
			{
				throw new HttpException("Feed is local only.");
			}

			// Get the output content type, let the querystring override web.config value. Default value is JSON.
			ContentType outputType = ContentType.Json;
			bool queryStringOverrideContentType = false;
			if (!string.IsNullOrEmpty(context.Request[QueryKeys.ContentType]))
			{
				if (context.Request[QueryKeys.ContentType].ToLower().Contains("xml"))
				{
					queryStringOverrideContentType = true;
					outputType = ContentType.Xml;
				}
				else if (context.Request[QueryKeys.ContentType].ToLower().Contains("json"))
				{
					// Keep output type as JSON
					queryStringOverrideContentType = true;
				}
			}
			if (!queryStringOverrideContentType && !string.IsNullOrEmpty(config.ContentType) && config.ContentType.ToLower().Contains("xml"))
			{
				// web.config override to XML
				outputType = ContentType.Xml;
			}

			switch (outputType)
			{
				case ContentType.Xml:
					context.Response.ContentType = "text/xml";
					break;
				case ContentType.Json:
				default:
					context.Response.ContentType = "text/json";
					break;
			}

			context.Trace.IsEnabled = config.Traced;
			var showValues = config.ShowValues;

			bool showAll;
			bool.TryParse(context.Request["showAll"], out showAll);

			bool expanded;
			bool.TryParse(context.Request[QueryKeys.Expanded], out expanded);

			var regionName = context.Request["regionName"];

			bool cacheFootprint;
			bool.TryParse(context.Request[QueryKeys.CacheFootprint], out cacheFootprint);

			var url = context.Request.Url;
			var alternateLink = new Uri(url.GetLeftPart(UriPartial.Path));

			bool clear;
			var key = context.Request["key"];
			var remove = context.Request["remove"];

			var objectCacheName = context.Request["objectCacheName"] ?? config.ObjectCacheName;

			if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(objectCacheName))
			{
				var cache = GetCache(objectCacheName);

				if (cache != null)
				{
					switch (outputType)
					{
						case ContentType.Xml:
							var feed = cache.GetFeed(key, null, null, alternateLink, alternateLink, showAll, regionName, showValues);
							Render(feed, context.Response.OutputStream, config.Stylesheet);
							break;
						case ContentType.Json:
						default:
							var json = cache.GetJson(key, null, null, alternateLink, alternateLink, showAll, regionName, showValues);
							Render(json, context.Response.OutputStream);
							break;
					}
				}
			}
			else if (!string.IsNullOrWhiteSpace(remove) && !string.IsNullOrWhiteSpace(objectCacheName))
			{
				var cache = GetCache(objectCacheName);

				if (cache != null)
				{
					Remove(cache, remove, alternateLink, showAll, regionName, context.Response.OutputStream, outputType, showValues && expanded, config.Stylesheet);
				}
			}
			else if (bool.TryParse(context.Request["clear"], out clear))
			{
				if (string.IsNullOrWhiteSpace(objectCacheName))
				{
					var caches = GetCaches(objectCacheName);
					foreach (var cache in caches) Clear(cache, alternateLink, showAll, regionName, context.Response.OutputStream, outputType, showValues && expanded, config.Stylesheet);
				}
				else
				{
					var cache = GetCache(objectCacheName);

					if (cache != null)
					{
						Clear(cache, alternateLink, showAll, regionName, context.Response.OutputStream, outputType, showValues && expanded, config.Stylesheet);
					}
				}
			}
			else if (cacheFootprint)
			{
				var caches = GetCaches(objectCacheName);
				RenderCacheFootprint(caches, context, outputType, expanded);
			}
			else
			{
				var caches = GetCaches(objectCacheName);
				RenderList(caches, alternateLink, showAll, regionName, context.Response.OutputStream, outputType, showValues && expanded, config.Stylesheet);
			}
		}

		private static ObjectCache GetCache(string objectCacheName)
		{
			var cache = CrmConfigurationManager.GetObjectCaches(objectCacheName).FirstOrDefault()
					?? CrmConfigurationManager.CreateObjectCache(objectCacheName);

			return cache;
		}

		private static IEnumerable<ObjectCache> GetCaches(string objectCacheName)
		{
			var caches = CrmConfigurationManager.GetObjectCaches(objectCacheName);

			if (caches.Any()) return caches;

			return new[] { CrmConfigurationManager.CreateObjectCache(objectCacheName) };
		}

		private static void RenderCacheFootprint(IEnumerable<ObjectCache> caches, HttpContext httpContext, ContentType outputType, bool expanded)
		{
			var response = httpContext.Response;

			switch (outputType)
			{
				case ContentType.Xml:
					{
						response.ContentEncoding = System.Text.Encoding.UTF8;
						response.Expires = -1;
						var footprint = caches.First().GetCacheFootprintXml(expanded, httpContext.Request.Url);
						footprint.Save(response.Output);
					}
					break;
				case ContentType.Json:
				default:
					using (StreamWriter writer = new StreamWriter(response.OutputStream))
					{
						var footprint = caches.First().GetCacheFootprintJson(expanded, httpContext.Request.Url);
						using (Newtonsoft.Json.JsonTextWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(writer) { QuoteName = false, Formatting = Newtonsoft.Json.Formatting.Indented })
						{
							Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
							serializer.Serialize(jsonWriter, footprint);
							jsonWriter.Flush();
						}
					}
					break;
			}
		}

		private static void RenderList(IEnumerable<ObjectCache> caches, Uri alternateLink, bool showAll, string regionName,
			Stream output, ContentType outputType, bool expanded, string stylesheet)
		{
			if (caches.Count() == 1)
			{
				RenderList(caches.First(), alternateLink, showAll, regionName, output, outputType, expanded, stylesheet);
				return;
			}

			switch (outputType)
			{
				case ContentType.Xml:
					var feeds = caches.Select(c => c.GetFeed(null, null, alternateLink, alternateLink, showAll, regionName, expanded)).ToList();

					if (feeds.Any())
					{
						var feed = new SyndicationFeed(
							feeds.Select(f => f.Title.Text).Aggregate((s1, s2) => s1 + ", " + s2),
							null,
							alternateLink,
							feeds.SelectMany(f => f.Items));

						Render(feed, output, stylesheet);
					}
					break;
				case ContentType.Json:
				default:
					var json = caches.Select(c => c.GetJson(null, null, alternateLink, alternateLink, showAll, regionName, expanded)).ToList();
					
					if (json.Count == 1)
					{
						Render(json.First(), output);
					}
					else if (json.Count > 1)
					{
						Render(new JArray(json), output);
					}

					break;
			}
		}

		private static void RenderList(ObjectCache cache, Uri alternateLink, bool showAll, string regionName, Stream output, ContentType outputType, bool expanded, string stylesheet)
		{
			switch (outputType)
			{
				case ContentType.Xml:
					var feed = cache.GetFeed(null, null, alternateLink, alternateLink, showAll, regionName, expanded);
					Render(feed, output, stylesheet);
					break;
				case ContentType.Json:
				default:
					var json = cache.GetJson(null, null, alternateLink, alternateLink, showAll, regionName, expanded);
					Render(json, output);
					break;
			}
		}

		private static void Clear(ObjectCache cache, Uri alternateLink, bool showAll, string regionName, Stream output, ContentType outputType, bool expanded, string stylesheet)
		{
			cache.RemoveAll();

			RenderList(cache, alternateLink, showAll, regionName, output, outputType, expanded, stylesheet);
		}

		private static void Render(SyndicationFeed feed, Stream output, string stylesheet)
		{
			var formatter = feed.GetAtom10Formatter();
			var settings = new XmlWriterSettings { Indent = true };
			using (var writer = XmlWriter.Create(output, settings))
			{
				if (!string.IsNullOrWhiteSpace(stylesheet))
				{
					var path = stylesheet.StartsWith("~") ? VirtualPathUtility.ToAbsolute(stylesheet) : stylesheet;
					writer.WriteProcessingInstruction("xml-stylesheet", @"type=""text/xsl"" href=""{0}""".FormatWith(path));
					// add padding to prevent browsers from recognizing the response as an atom feed
					writer.WriteComment(new string(' ', 512));
				}

				formatter.WriteTo(writer);
				writer.Flush();
			}
		}

		private static void Render(JToken feed, Stream output)
		{
			using (var writer = new StreamWriter(output))
			using (var jsonWriter = new JsonTextWriter(writer) { QuoteName = false, Formatting = Newtonsoft.Json.Formatting.Indented })
			{
				feed.WriteTo(jsonWriter);
			}
		}

		private static void Remove(ObjectCache cache, string key, Uri alternateLink, bool showAll, string regionName, Stream output, ContentType outputType, bool expanded, string stylesheet)
		{
			cache.Remove(key, regionName);

			RenderList(cache, alternateLink, showAll, regionName, output, outputType, expanded, stylesheet);
		}

		private static bool IsLocal(HttpRequest request)
		{
			var address = request.UserHostAddress;

			if (address == "127.0.0.1" || address == "::1")
			{
				return true;
			}

			if ((!string.IsNullOrEmpty(address)) && (address == request.ServerVariables["LOCAL_ADDR"]))
			{
				return true;
			}

			return false;
		}
	}
}
