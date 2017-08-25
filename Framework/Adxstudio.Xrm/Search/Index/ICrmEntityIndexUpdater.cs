/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Search.Index
{
	public interface ICrmEntityIndexUpdater : IDisposable
	{
		void DeleteEntity(string entityLogicalName, Guid id);

		void DeleteEntitySet(string entityLogicalName);

		void UpdateEntity(string entityLogicalName, Guid id);

		void UpdateEntitySet(string entityLogicalName);

		void UpdateEntitySet(string entityLogicalName, string entityAttribute, List<Guid> entityIds);

		void UpdateCmsEntityTree(string entityLogicalName, Guid rootEntityId, int? lcid = null);
    }
}
