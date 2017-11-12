/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Core.Flighting;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Feedback
{
	public class Comment : IComment
	{
		private readonly Lazy<bool> _editable;
		private readonly Lazy<ApplicationPath> _getDeletePath;
		private readonly Lazy<ApplicationPath> _getEditPath;

		public Comment(
			Entity feedback,
			Lazy<ApplicationPath> getEditPath = null,
			Lazy<ApplicationPath> getDeletePath = null,
			Lazy<bool> editable = null,
			IRatingInfo ratingInfo = null,
			bool ratingEnabled = false)
		{
			feedback.ThrowOnNull("entity");
			feedback.AssertEntityName("feedback");

			Entity = feedback;

			var authorReference = feedback.GetAttributeValue<EntityReference>("createdbycontact");
			if (authorReference != null)
			{
				var authorNameAttribute = Localization.LocalizeFullName(
					feedback.GetAttributeAliasedValue<string>("author.firstname"),
					feedback.GetAttributeAliasedValue<string>("author.lastname"));
				var authorEmailAttribute = feedback.GetAttributeAliasedValue<string>("author.emailaddress1");
				Author = new Author(authorReference,
					authorNameAttribute ?? string.Empty,
					authorEmailAttribute ?? string.Empty);
			}
			else
			{
				var authorName = feedback.Contains("adx_createdbycontact") ? feedback["adx_createdbycontact"].ToString() : string.Empty;
				if (!string.IsNullOrWhiteSpace(authorName))
				{
					var authorUrl = feedback.Contains("adx_authorurl") ? feedback["adx_authorurl"].ToString() : string.Empty;
					var authorauthorEmailUrl = feedback.Contains("adx_contactemail") ? feedback["adx_contactemail"].ToString() : string.Empty;
					Author = new Author(authorName, authorUrl, authorauthorEmailUrl);
				}
			}

			Content = feedback.GetAttributeValue<string>("comments");
			Date = feedback.GetAttributeValue<DateTime?>("createdon") ?? feedback.GetAttributeValue<DateTime>("createdon");
			Name = feedback.GetAttributeValue<string>("title");
			IsApproved = feedback.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault();

			_getEditPath = getEditPath;
			_getDeletePath = getDeletePath;
			_editable = editable;
			RatingInfo = ratingInfo;
			RatingEnabled = ratingEnabled;

			if (FeatureCheckHelper.IsFeatureEnabled(FeatureNames.TelemetryFeatureUsage))
			{
				PortalFeatureTrace.TraceInstance.LogFeatureUsage(FeatureTraceCategory.Feedback, HttpContext.Current, "read_feedback", 1, feedback.ToEntityReference(), "read");
			}
		}

		public IRatingInfo RatingInfo { get; private set; }

		public bool RatingEnabled { get; private set; }

		public IAuthor Author { get; private set; }

		public ApplicationPath DeletePath
		{
			get { return _getDeletePath == null ? null : _getDeletePath.Value; }
		}

		public ApplicationPath EditPath
		{
			get { return _getEditPath == null ? null : _getEditPath.Value; }
		}

		public bool Editable
		{
			get { return _editable != null && _editable.Value; }
		}

		public Entity Entity { get; private set; }

		public bool IsApproved { get; private set; }

		public string Content { get; private set; }

		public DateTime Date { get; private set; }

		public string Name { get; private set; }
	}
}
