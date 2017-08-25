/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityCollectionDrop : PortalDrop
	{
		private readonly EntityCollection _entityCollection;
		private Lazy<EntityDrop[]> _entities;

		public EntityCollectionDrop(IPortalLiquidContext portalLiquidContext, EntityCollection entityCollection) : base(portalLiquidContext)
		{
			if (entityCollection == null) throw new ArgumentNullException("entityCollection");

			_entityCollection = entityCollection;
			_entities = new Lazy<EntityDrop[]>(GetEntities, LazyThreadSafetyMode.None);
		}

		public string EntityName
		{
			get { return _entityCollection.EntityName; }
		}

		public IEnumerable<EntityDrop> Entities
		{
			get { return _entities.Value; }
		}

		public string MinActiveRowVersion
		{
			get { return _entityCollection.MinActiveRowVersion; }
		}

		public bool MoreRecords
		{
			get { return _entityCollection.MoreRecords; }
		}

		public string PagingCookie
		{
			get { return _entityCollection.PagingCookie; }
		}

		public int TotalRecordCount
		{
			get { return _entityCollection.TotalRecordCount; }
		}

		public bool TotalRecordCountLimitExceeded
		{
			get { return _entityCollection.TotalRecordCountLimitExceeded; }
		}

		private EntityDrop[] GetEntities()
		{
			return _entityCollection.Entities.Select(e => new EntityDrop(this, e)).ToArray();
		}
	}
}
