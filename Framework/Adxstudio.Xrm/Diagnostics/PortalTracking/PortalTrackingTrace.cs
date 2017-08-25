/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using System.Web;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using System.Web.Hosting;
	using Adxstudio.Xrm.Configuration;
	using Adxstudio.Xrm.Web;
	using Adxstudio.Xrm.Diagnostics.Trace;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Telemetry implementation for portal feature usage which will track the user's journey
	/// </summary>
	public class PortalTrackingTrace
	{

		/// <summary>
		/// Blog Post View Interaction Name
		/// </summary>
		private const string BlogPostViewInteractionName = "portal_viewblogpost";

		/// <summary>
		/// Forum Post View Interaction Name
		/// </summary>
		private const string ForumPostViewInteractionName = "portal_viewforumthread";

		/// <summary>
		/// Search Execute Interaction Name
		/// </summary>
		private const string SearchExecuteInteractionName = "portal_search";

		/// <summary>
		/// Knowledge Article View Interaction Name
		/// </summary>
		private const string KnowledgeArticleViewInteractionName = "portal_viewknowledgearticle";

		/// <summary>
		/// Put Interactions Uri Suffix
		/// </summary>
		private const string PutInteractionsUriSuffix = "/data/Interactions?api-version=2016-01-01";

		/// <summary>
		/// The variable for lazy initialization of <see cref="PortalFeatureTrace"/>.
		/// </summary>
		private static readonly Lazy<PortalTrackingTrace> Instance = new Lazy<PortalTrackingTrace>();


		/// <summary>
		/// The variable for HubUri/>.
		/// </summary>
		private static readonly string HubUri = Environment.GetEnvironmentVariable(Diagnostics.Constants.Uri);

		/// <summary>
		/// The variable for Sig/>.
		/// </summary>
		private static readonly string Sig = Environment.GetEnvironmentVariable(Diagnostics.Constants.AccessKey);

		/// <summary>
		/// The variable for PolicyName/>.
		/// </summary>
		private static readonly string PolicyName = Environment.GetEnvironmentVariable(Diagnostics.Constants.AccessKeyName);

		/// <summary>
		/// The variable for PortalId/>.
		/// </summary>
		private static readonly string PortalId = Environment.GetEnvironmentVariable(Diagnostics.Constants.PortalId);

		/// <summary>The base address uri.</summary>
		private static readonly string BaseAddressUri = Environment.GetEnvironmentVariable(Diagnostics.Constants.Uri);

		/// <summary>The http client.</summary>
		private static HttpClient client;

		/// <summary>
		/// The variable for httpClient/>.
		/// </summary>
		private static HttpClient HttpClient
		{
			get
			{
				return client ?? (string.IsNullOrWhiteSpace(BaseAddressUri) ? null : client = new HttpClient { BaseAddress = new Uri(BaseAddressUri) });
			}
		}

		/// <summary>
		/// Lazy initialization of PortalFeatureTrace
		/// </summary>
		public static PortalTrackingTrace TraceInstance
		{
			get
			{
				return Instance.Value;
			}

		}

		/// <summary>
		/// Unsubmitted tracking events that will be submitted to DCI
		/// </summary>
		private ConcurrentBag<JourneyDetail> trackingEvents = new ConcurrentBag<JourneyDetail>();

		/// <summary>
		/// Enum of feature usage level
		/// 1 - Emits portal feature usage events
		/// 2 - Emits portal button click events
		/// </summary>
		private enum Feature
		{
			PageLanding = 1,
			ButtonClick = 2

		}

		/// <summary>
		/// Computes the token signature
		/// </summary>
		/// <param name="uri">URI param</param>
		/// <param name="sharedKey">Shared Key</param>
		/// <param name="policyName">Policy Name Key</param>
		/// <returns>token signature</returns>
		private static string ComputeTokenSignature(string uri, string sharedKey, string policyName)
		{
			string signatureStringFormat = "sig={0}",
				expiryStringFormat = "se={0}",
				keyNameStringFormat = "skn={0}",
				resourceStringFormat = "sr={0}",
				tokenFormat = "{0}&{1}&{2}&{3}";

			DateTime epochBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime expiry = DateTime.Now.AddYears(1);
			var expiryString = ((long)(expiry - epochBaseTime).TotalSeconds).ToString("D");

			var requestUri = new Uri(uri);
			var resourceUrl = requestUri.Host + HttpUtility.UrlDecode(requestUri.AbsolutePath);

			var fields = new List<string> { HttpUtility.UrlEncode(resourceUrl), expiryString };
			using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey)))
			{
				var requestString = string.Join("\n", fields);
				var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(requestString)));

				return string.Format(
					tokenFormat,
					string.Format(signatureStringFormat, HttpUtility.UrlEncode(signature)),
					string.Format(expiryStringFormat, HttpUtility.UrlEncode(expiryString)),
					string.Format(keyNameStringFormat, HttpUtility.UrlEncode(policyName)),
					string.Format(resourceStringFormat, HttpUtility.UrlEncode(resourceUrl)));
			}
		}

		/// <summary>
		/// Returns the HttpContextWrapper
		/// </summary>
		protected HttpContextBase HttpContextBase
		{
			get
			{
				if (HttpContext.Current == null)
				{
					return null;
				}
				return new HttpContextWrapper(HttpContext.Current);
			}
		}

		/// <summary>
		/// Returns the userId
		/// </summary>
		protected Guid UserId
		{
			get
			{
				if (HttpContextBase != null)
				{
					return new Guid(HttpContextBase.GetUser().Id);
				}
				else
				{
					return Guid.Empty;
				}
			}
		}

		/// <summary>
		/// Customer searched a term
		/// </summary>
		/// <param name="logType">logType param</param>
		/// <param name="entityIdOrSearchTerm">entityIdOrSearchTerm param</param>
		/// <param name="title">title param</param>
		public void Log(string logType, string entityIdOrSearchTerm, string title)
		{
			// if the user has DCI configured.
			if (!string.IsNullOrEmpty(HubUri))
			{
				// if the user is not logged in, don't bother logging it.
				var context = this.HttpContextBase;

				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					$"LogJourneyEvent: Got Log message for LogType: {logType} Id: {entityIdOrSearchTerm}");

				if (context != null && context.User != null && context.User.Identity.IsAuthenticated)
				{
					switch (logType)
					{
						case Diagnostics.Constants.Blog:
							this.LogBlogPost(entityIdOrSearchTerm, title);
							break;
						case Diagnostics.Constants.Article:
							this.LogJourneyEvent(entityIdOrSearchTerm, title);
							break;
						case Diagnostics.Constants.Forum:
							this.LogForumEvent(entityIdOrSearchTerm, title);
							break;
						case Diagnostics.Constants.Search:
							this.LogSearchEvent(entityIdOrSearchTerm);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Customer searched a term
		/// </summary>
		/// <param name="blogpostid">blogpostid param</param>
		/// <param name="blogposttitle">blogposttitle param</param>
		public void LogBlogPost(string blogpostid, string blogposttitle)
		{
			// have to check for the type of tech we're using
			if (Diagnostics.Constants.PortalTrackingTechnology.ResolveAppSetting() == Diagnostics.Constants.DCI)
			{

				var timestamp = DateTime.UtcNow;
				var guid = Guid.NewGuid();
				try
				{
					// request body as JSON describing interaction
					var json = new JObject
						{
							{ Diagnostics.Constants.PortalTrackingInteractionType.ResolveAppSetting(), BlogPostViewInteractionName },
							{ Diagnostics.Constants.PortalTrackingCrmInteractionId.ResolveAppSetting(), guid },
							{ Diagnostics.Constants.PortalTrackingTimeStamp.ResolveAppSetting(), timestamp },
							{ Diagnostics.Constants.PortalTrackingContactId.ResolveAppSetting(), this.UserId },
							{ Diagnostics.Constants.PortalTrackingBlogPostId.ResolveAppSetting(), blogpostid },
							{ Diagnostics.Constants.PortalTrackingBlogPostTitle.ResolveAppSetting(), blogposttitle },
							{ Diagnostics.Constants.PortalTrackingPortalId.ResolveAppSetting(), PortalId }
						};

					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						$"LogJourneyEvent: Logging Blog message for Blog CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");

					this.Activate(json);
				}
				catch (Exception ex)
				{
					var message = string.Empty;
					while (ex != null)
					{
						message = message + " \n" + ex.Message;
						ex = ex.InnerException;
					}
					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: Blog received unexpected exception. Message: {message} CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");
				}
			}
		}

		/// <summary>
		/// Customer searched a term
		/// </summary>
		/// <param name="forumpostid">forumpostid param</param>
		/// <param name="forumposttitle">forumposttitle param</param>
		public void LogForumEvent(string forumpostid, string forumposttitle)
		{
			// have to check for the type of tech we're using
			if (Diagnostics.Constants.PortalTrackingTechnology.ResolveAppSetting() == Diagnostics.Constants.DCI)
			{

				var timestamp = DateTime.UtcNow;
				var guid = Guid.NewGuid();
				try
				{
					// request body as JSON describing interaction
					var json = new JObject
						{
							{ Diagnostics.Constants.PortalTrackingInteractionType.ResolveAppSetting(), ForumPostViewInteractionName },
							{ Diagnostics.Constants.PortalTrackingCrmInteractionId.ResolveAppSetting(), guid },
							{ Diagnostics.Constants.PortalTrackingTimeStamp.ResolveAppSetting(), timestamp },
							{ Diagnostics.Constants.PortalTrackingContactId.ResolveAppSetting(), this.UserId },
							{ Diagnostics.Constants.PortalTrackingForumPostId.ResolveAppSetting(), forumpostid },
							{ Diagnostics.Constants.PortalTrackingForumPostTitle.ResolveAppSetting(), forumposttitle },
							{ Diagnostics.Constants.PortalTrackingPortalId.ResolveAppSetting(), PortalId }
						};

					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						$"LogJourneyEvent: Logging Forum message for Forum CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");

					this.Activate(json);
				}
				catch (Exception ex)
				{
					var message = string.Empty;
					while (ex != null)
					{
						message = message + " \n" + ex.Message;
						ex = ex.InnerException;
					}
					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: Forum log received unexpected exception. Message: {message} CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");
				}
			}
		}

		/// <summary>
		/// Customer searched a term
		/// </summary>
		/// <param name="searchTerm">search Term</param>
		public void LogSearchEvent(string searchTerm)
		{
			// have to check for the type of tech we're using
			if (Diagnostics.Constants.PortalTrackingTechnology.ResolveAppSetting() == Diagnostics.Constants.DCI)
			{
				var timestamp = DateTime.UtcNow;
				var guid = Guid.NewGuid();

				try
				{
					// request body as JSON describing interaction
					var json = new JObject
						{
							{ Diagnostics.Constants.PortalTrackingInteractionType.ResolveAppSetting(), SearchExecuteInteractionName },
							{ Diagnostics.Constants.PortalTrackingCrmInteractionId.ResolveAppSetting(), Guid.NewGuid() },
							{ Diagnostics.Constants.PortalTrackingTimeStamp.ResolveAppSetting(), timestamp },
							{ Diagnostics.Constants.PortalTrackingContactId.ResolveAppSetting(), this.UserId },
							{ Diagnostics.Constants.PortalTrackingSearchString.ResolveAppSetting(), searchTerm },
							{ Diagnostics.Constants.PortalTrackingPortalId.ResolveAppSetting(), PortalId },
						};

					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						$"LogJourneyEvent: Logging Search message for Forum CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");

					this.Activate(json);
				}
				catch (Exception ex)
				{
					var message = string.Empty;
					while (ex != null)
					{
						message = message + " \n" + ex.Message;
						ex = ex.InnerException;
					}

					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: Search log received unexpected exception. Message: {message} CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");
				}
			}
		}


		/// <summary>
		/// Customer visited a page
		/// </summary>
		/// <param name="knowledgeArticleId">knowledgeArticleId Id</param>
		/// <param name="knowledgeArticleTitle">knowledgeArticle Title</param>
		public void LogJourneyEvent(string knowledgeArticleId, string knowledgeArticleTitle)
		{
			// have to check for the type of tech we're using
			if (Diagnostics.Constants.PortalTrackingTechnology.ResolveAppSetting() == Diagnostics.Constants.DCI)
			{
				var timestamp = DateTime.UtcNow;
				var guid = Guid.NewGuid();

				try
				{
					// request body as JSON describing interaction
					var json = new JObject
						{
							{ Diagnostics.Constants.PortalTrackingInteractionType.ResolveAppSetting(), KnowledgeArticleViewInteractionName },
							{ Diagnostics.Constants.PortalTrackingTimeStamp.ResolveAppSetting(), timestamp },
							{ Diagnostics.Constants.PortalTrackingCrmInteractionId.ResolveAppSetting(), guid },
							{ Diagnostics.Constants.PortalTrackingContactId.ResolveAppSetting(), this.UserId },
							{ Diagnostics.Constants.PortalTrackingKnowledgeArticleId.ResolveAppSetting(), knowledgeArticleId },
							{ Diagnostics.Constants.PortalTrackingPortalId.ResolveAppSetting(), PortalId },
							{ Diagnostics.Constants.PortalTrackingKnowledgeArticleTitle.ResolveAppSetting(), knowledgeArticleTitle }
						};
					
					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						$"LogJourneyEvent: Logging KB message for Forum CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");

					this.Activate(json);

				}
				catch (Exception ex)
				{
					var message = string.Empty;
					while (ex != null)
					{
						message = message + " \n" + ex.Message;
						ex = ex.InnerException;
					}
					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: DCI log received unexpected exception. Message: {message} CrmInteractionId: {guid} PortalId: {PortalId} TimeStamp: {timestamp}");
				}
			}
		}
		
		/// <summary>
		/// Auth UCI Task Call
		/// </summary>
		/// <param name="interactionJson">Json param</param>
		/// <returns>true boolean</returns>
		private void Activate(JObject interactionJson)
		{
			HostingEnvironment.QueueBackgroundWorkItem(ct => this.AuthDCICall(interactionJson, HubUri, PolicyName, Sig, PortalId));
		}

		/// <summary>Auth UCI Call</summary>
		/// <param name="interactionJson">Json param</param>
		/// <param name="hubUri">hubUri param</param>
		/// <param name="policyName">policyName param</param>
		/// <param name="sig">sig param</param>
		/// <param name="portalId">portalId param</param>
		/// <returns>bool of success call</returns>
		private async Task AuthDCICall(JObject interactionJson, string hubUri, string policyName, string sig, string portalId)
		{
			try
			{
				if (string.IsNullOrEmpty(hubUri) || string.IsNullOrEmpty(sig) || string.IsNullOrEmpty(policyName) || HttpClient == null)
				{
					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: The DCI App Settings are not set in webapp {PortalId}");

					return;
				}

				var token = ComputeTokenSignature(hubUri, sig, policyName);

				var request = new HttpRequestMessage()
				{
					RequestUri = new Uri(hubUri + PutInteractionsUriSuffix),
					Method = HttpMethod.Post,
					Content = new StringContent(interactionJson.ToString(), Encoding.UTF8, "application/json")
				};
				request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", token);
				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					$"LogJourneyEvent: Pre-DCI Post PortalId: {portalId}");

				var result = await HttpClient.SendAsync(request);

				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.DCI, HttpContextBase, "dci", 1, null, "create");

				ADXTrace.Instance.TraceInfo(TraceCategory.Application,
					$"LogJourneyEvent: Post-DCI Post PortalId: {portalId} {result.StatusCode}");

				if (!result.IsSuccessStatusCode)
				{
					var response = await result.Content.ReadAsStringAsync();

					ADXTrace.Instance.TraceError(TraceCategory.Exception,
						$"LogJourneyEvent: The DCI post was not successful. Http Code: {result.StatusCode} HttpContent: {response} PortalId: {portalId}");

					request.Dispose();

					result.Dispose();
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application,
						$"LogJourneyEvent: The DCI post was successful. Http Code: {result.StatusCode} PortalId: {portalId}");

					request.Dispose();

					result.Dispose();
				}

			}
			catch (Exception ex)
			{
				var message = string.Empty;
				while (ex != null)
				{
					message = message + " \n" + ex.Message;
					ex = ex.InnerException;
				}
				ADXTrace.Instance.TraceError(TraceCategory.Exception,
					$"LogJourneyEvent: DCI received unexpected exception. Message: {message} CrmInteractionId: {interactionJson[Diagnostics.Constants.PortalTrackingCrmInteractionId]} PortalId: {portalId}");
			}
			
		}
	}
}
