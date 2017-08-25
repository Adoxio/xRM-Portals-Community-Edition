/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
    using System;

    public class RelatedProduct : IRelatedProduct
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }

		public string Url { get; private set; }

		public RelatedProduct(Guid id, string name, string url)
		{
            Id = id;
			Name = name;
			Url = url;
		}
	}
}
