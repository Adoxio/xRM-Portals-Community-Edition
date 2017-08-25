/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Factory used to create entity replicates
	/// </summary>
	public class CrmEntityReplicationFactory
	{
		private readonly OrganizationServiceContext _context;

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="context">The Organization Service Context to be used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public CrmEntityReplicationFactory(OrganizationServiceContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			_context = context;
		}

		/// <summary>
		/// Gets the replication  entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public IReplication GetReplication(Entity entity)
		{
			if (entity == null)
			{
				return new NullReplication();
			}

			var entityName = entity.LogicalName;

			if (entityName == "annotation")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity is annotation, returning NoteReplication.");

				return new NoteReplication(entity, _context);
			}

			if (entityName == "adx_webfile")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity is adx_webfile, returning WebFileReplication.");

				return new WebFileReplication(entity, _context);
			}

			if (entityName == "adx_webpage")
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, "Source entity is adx_webpage, returning WebPageReplication.");

				return new WebPageReplication(entity, _context);
			}

			return new NullReplication();
		}
	}
}
