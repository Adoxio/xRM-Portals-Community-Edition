/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Forums
{
	public interface IForumInfo
	{
		IForumPostInfo LatestPost { get; }
	}

	internal class UnknownForumInfo : IForumInfo
	{
		public IForumPostInfo LatestPost
		{
			get { return null; }
		}
	}
}
