/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.KnowledgeArticles;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class KnowledgeArticlesDrop : PortalDrop
	{
		private readonly Lazy<KnowledgeArticleDrop[]> _popularArticles;
		private readonly Lazy<KnowledgeArticleDrop[]> _recentArticles;
		private readonly Lazy<KnowledgeArticleDrop[]> _topArticles;
		
		public KnowledgeArticlesDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, int pageSize = 5, string languageLocaleCode = null) : base(portalLiquidContext)
		{
			if (dependencies == null) throw new ArgumentException("dependencies");

			PortalLiquidContext = portalLiquidContext;

			Dependencies = dependencies;

			var dataAdapter = new KnowledgeArticleAggregationDataAdapter(dependencies);

			_topArticles = new Lazy<KnowledgeArticleDrop[]>(() => dataAdapter.SelectTopArticles(pageSize, languageLocaleCode).Select(e => new KnowledgeArticleDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
			
			_recentArticles = new Lazy<KnowledgeArticleDrop[]>(() => dataAdapter.SelectRecentArticles(pageSize, languageLocaleCode).Select(e => new KnowledgeArticleDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

			_popularArticles = new Lazy<KnowledgeArticleDrop[]>(() => dataAdapter.SelectPopularArticles(pageSize, languageLocaleCode).Select(e => new KnowledgeArticleDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		internal IDataAdapterDependencies Dependencies { get; private set; }

		internal IPortalLiquidContext PortalLiquidContext { get; private set; }

		public IEnumerable<KnowledgeArticleDrop> Popular
		{
			get { return _popularArticles.Value.AsEnumerable(); }
		}

		public IEnumerable<KnowledgeArticleDrop> Recent
		{
			get { return _recentArticles.Value.AsEnumerable(); }
		}

		public IEnumerable<KnowledgeArticleDrop> Top
		{
			get { return _topArticles.Value.AsEnumerable(); }
		}
	}
}
