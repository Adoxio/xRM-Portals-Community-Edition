/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Globalization;
using Adxstudio.Xrm.Forums;

namespace Site.Areas.Forums
{
	public static class ForumHelpers
	{
		public static string PostedOn(IForumPostInfo postInfo, string format)
		{
			if (postInfo == null) return string.Empty;

			return postInfo.PostedOn.ToString(format, CultureInfo.InvariantCulture);
		}
	}
}
