/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using GridMetadata = Adxstudio.Xrm.Web.UI.CrmEntityListView.GridMetadata;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EntityListViewDrop : PortalDrop
	{
		private readonly Lazy<EntityListViewColumnDrop[]> _columns;

		public EntityListViewDrop(IPortalLiquidContext portalLiquidContext, EntityView view)
			: base(portalLiquidContext)
		{
			if (view == null) throw new ArgumentNullException("view");

			View = view;

			var portalViewContext = portalLiquidContext.PortalViewContext;
            if (portalViewContext != null)
            {
                var entityListRef = portalViewContext.Entity.GetAttribute("adx_entitylist")?.Value as EntityReference;
                if (entityListRef != null)
                {
                    var entityList = portalLiquidContext.PortalOrganizationService.Retrieve("adx_entitylist", entityListRef.Id,
                        new ColumnSet("adx_settings"));

                    if (!string.IsNullOrWhiteSpace(entityList.GetAttributeValue<string>("adx_settings")))
                    {
                        try
                        {
                            var gridMetadataConfiguration = JsonConvert.DeserializeObject<GridMetadata>(entityList.GetAttributeValue<string>("adx_settings"),
                                new JsonSerializerSettings
                                {
                                    ContractResolver = JsonConfigurationContractResolver.Instance,
                                    TypeNameHandling = TypeNameHandling.Objects,
                                    Converters = new List<JsonConverter> { new GuidConverter() },
                                    Binder = new ActionSerializationBinder()
                                });

                            _columns = new Lazy<EntityListViewColumnDrop[]>(
                                () => View.Columns.Select(e => new EntityListViewColumnDrop(this, e, gridMetadataConfiguration.ColumnOverrides)).ToArray(),
                                LazyThreadSafetyMode.None);

                            return;
                        }
                        catch (Exception e)
                        {
                            ADXTrace.Instance.TraceError(TraceCategory.Application, e.ToString());
                        }
                    }
                }
            }

			_columns = new Lazy<EntityListViewColumnDrop[]>(() => View.Columns.Select(e => new EntityListViewColumnDrop(this, e)).ToArray(), LazyThreadSafetyMode.None);
		}

		public IEnumerable<EntityListViewColumnDrop> Columns { get { return _columns.Value; } }

        public string DisplayName { get { return View.DisplayName; } }
        public string EntityLogicalName { get { return View.EntityLogicalName; } }

        public string Id { get { return View.Id.ToString(); } }

		public int LanguageCode { get { return View.LanguageCode; } }

		public string Name { get { return View.Name; } }
		public string PrimaryKeyLogicalName { get { return View.PrimaryKeyLogicalName; } }

		public string SortExpression { get { return View.SortExpression; } }

		internal EntityView View { get; private set; }

		internal Guid ViewId { get { return View.Id; } }
	}
}