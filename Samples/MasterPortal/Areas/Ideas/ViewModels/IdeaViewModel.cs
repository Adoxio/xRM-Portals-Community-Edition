/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Ideas;

namespace Site.Areas.Ideas.ViewModels
{
	public class IdeaViewModel
	{
		public IdeaCommentsViewModel Comments { get; set; }
		
		public IIdea Idea { get; set; }
		
		public IIdeaForum IdeaForum { get; set; }
	}
}
