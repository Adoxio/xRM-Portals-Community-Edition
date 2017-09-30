/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Feedback
{
	public class FeedbackPolicyReader
	{
		private readonly Entity _relatedWebPage;

		public FeedbackPolicyReader(IDataAdapterDependencies dataAdapterDependencies, string siteMarker)
		{
			ISiteMarkerTarget result;
			if ((result = new SiteMarkerDataAdapter(dataAdapterDependencies).Select(siteMarker)) != null)
			{
				_relatedWebPage = result.Entity;
			}
		}

		public CommentPolicy GetCommentPolicy()
		{
			if (_relatedWebPage == null)
			{
				return CommentPolicy.Closed;
			}

			var commentPolicy = _relatedWebPage.GetAttributeValue<OptionSetValue>("adx_feedbackpolicy");

			if (commentPolicy == null)
			{
				return CommentPolicy.Closed;
			}

			return (CommentPolicy)commentPolicy.Value;
		}

		public bool IsRatingEnabled()
		{
			return _relatedWebPage != null && _relatedWebPage.GetAttributeValue<bool>("adx_enablerating");
		}
	}
}
