/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Search.Index
{
	public class EventedIndexUpdater : ICrmEntityIndexUpdater
	{
		private readonly Action _onUpdate;
		private readonly ICrmEntityIndexUpdater _updater;

        public EventedIndexUpdater(ICrmEntityIndexUpdater updater, Action onUpdate)
		{
			if (updater == null)
			{
				throw new ArgumentNullException("updater");
			}

			if (onUpdate == null)
			{
				throw new ArgumentNullException("onUpdate");
			}

			_updater = updater;
			_onUpdate = onUpdate;
		}

		public void DeleteEntity(string entityLogicalName, Guid id)
		{
			_updater.DeleteEntity(entityLogicalName, id);

			OnUpdate();
		}

		public void DeleteEntitySet(string entityLogicalName)
		{
            _updater.DeleteEntitySet(entityLogicalName);

			OnUpdate();
		}

		public void Dispose()
		{
            _updater.Dispose();
		}

		public void UpdateEntity(string entityLogicalName, Guid id)
		{
            _updater.UpdateEntity(entityLogicalName, id);

            OnUpdate();
		}

		public void UpdateEntitySet(string entityLogicalName)
		{
            _updater.UpdateEntitySet(entityLogicalName);

            OnUpdate();
		}

		public void UpdateEntitySet(string entityLogicalName, string entityAttribute, List<Guid> entityIds)
		{
			_updater.UpdateEntitySet(entityLogicalName, entityAttribute, entityIds);

			OnUpdate();
		}

		public void UpdateCmsEntityTree(string entityLogicalName, Guid rootEntityId, int? lcid = null)
        {
			_updater.UpdateCmsEntityTree(entityLogicalName, rootEntityId, lcid);

            OnUpdate();
        }

		private void OnUpdate()
		{
			_onUpdate();
		}
	}
}
