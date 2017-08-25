/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Data;
using Adxstudio.Xrm.Ideas;

namespace Site.Areas.Ideas.ViewModels
{
	public class IdeaForumViewModel
	{
		public IIdeaForum IdeaForum { get; set; }

		public PaginatedList<IIdea> Ideas { get; set; }

		public string CurrentStatusLabel { get; set; }
	}
}
