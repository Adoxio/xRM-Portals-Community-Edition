/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Adxstudio.Xrm.Ideas;

namespace Site.Areas.Ideas.ViewModels
{
	public class IdeasViewModel
	{
		public int IdeaForumCount { get; set; }

		public IEnumerable<IIdeaForum> IdeaForums { get; set; }
	}
}
