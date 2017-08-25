/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Search;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SearchIndexQueryResultDrop : PortalDrop
	{
		private Lazy<EntityDrop> _entity;

		public SearchIndexQueryResultDrop(IPortalLiquidContext portalLiquidContext, ICrmEntitySearchResult result) : base(portalLiquidContext)
		{
			if (result == null) throw new ArgumentNullException("result");

			Result = result;

			_entity = new Lazy<EntityDrop>(GetEntity, LazyThreadSafetyMode.None);
		}

		public EntityDrop Entity
		{
			get { return _entity.Value; }
		}

		public string Fragment
		{
			get { return Result.Fragment; }
		}

		public string Id
		{
			get { return Result.EntityID.ToString(); }
		}

		public string LogicalName
		{
			get { return Result.EntityLogicalName; }
		}

		public int Number
		{
			get { return Result.ResultNumber; }
		}

		public float Score
		{
			get { return Result.Score; }
		}

		public string Title
		{
			get { return Result.Title; }
		}

		public string Url
		{
			get { return Result.Url.ToString(); }
		}

		protected ICrmEntitySearchResult Result { get; private set; }

		public override object BeforeMethod(string method)
		{
			if (string.Equals(method, "logicalname", StringComparison.OrdinalIgnoreCase))
			{
				return LogicalName;
			}

			return base.BeforeMethod(method);
		}

		private EntityDrop GetEntity()
		{
			var entity = Result.Entity;

			return entity == null ? null : new EntityDrop(this, entity);
		}
	}
}
