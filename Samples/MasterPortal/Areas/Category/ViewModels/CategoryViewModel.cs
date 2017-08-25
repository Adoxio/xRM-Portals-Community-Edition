/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Category;

namespace Site.Areas.Category.ViewModels
{
	public class CategoryViewModel
	{
        public string Number { get; set; }

		public IEnumerable<RelatedArticle> RelatedArticles { get; set; }

		public IEnumerable<ChildCategory> ChildCategories { get; set; }

		public string Title { get; set; }

	}
}
