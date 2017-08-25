/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.KnowledgeArticles;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class KnowledgeDrop : PortalDrop
    {
        /// <summary>
        /// Categories Drop
        /// </summary>
        private readonly Lazy<CategoriesDrop> categoriesDrop;

        private readonly Lazy<KnowledgeArticlesDrop> _knowledgeArticlesDrop;

		public KnowledgeDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies) : base(portalLiquidContext)
		{
			if (dependencies == null) throw new ArgumentException("dependencies");

			PortalLiquidContext = portalLiquidContext;

			Dependencies = dependencies;

			_knowledgeArticlesDrop = new Lazy<KnowledgeArticlesDrop>(() => new KnowledgeArticlesDrop(portalLiquidContext, dependencies), LazyThreadSafetyMode.None);
            this.categoriesDrop = new Lazy<CategoriesDrop>(() => new CategoriesDrop(portalLiquidContext, dependencies), LazyThreadSafetyMode.None);
        }

		internal IDataAdapterDependencies Dependencies { get; private set; }

		internal IPortalLiquidContext PortalLiquidContext { get; private set; }

		public KnowledgeArticlesDrop Articles
		{
			get { return _knowledgeArticlesDrop.Value; }
		}
        /// <summary>
        /// Get Categories as a Category Drop
        /// </summary>
        public CategoriesDrop Categories
        {
            get { return this.categoriesDrop.Value; }
        }
    }
}
