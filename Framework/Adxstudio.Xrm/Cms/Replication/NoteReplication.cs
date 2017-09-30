/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Replication of an Note (annotation).
	/// </summary>
	public class NoteReplication : CrmEntityReplication
	{
		public const string BlobReplicationKey = "BlobReplication";

		/// <summary>
		/// Class initialization
		/// </summary>
		/// <param name="source">Entity</param>
		/// <param name="context">Organization Service Context</param>
		public NoteReplication(Entity source, OrganizationServiceContext context) : base(source, context, "annotation") { }

		public override void Created()
		{
			var ownerid = Source.GetAttributeValue<EntityReference>("objectid");

			if (ownerid == null || ownerid.LogicalName != "adx_webfile")
			{
				return;
			}

			var ownerFile = Context.CreateQuery("adx_webfile").FirstOrDefault(f => f.GetAttributeValue<Guid?>("adx_webfileid") == ownerid.Id);

			if (ownerFile == null)
			{
				return;
			}

			var subscribedFiles = ownerFile.GetRelatedEntities(Context, "adx_webfile_masterwebfile", EntityRole.Referenced).ToList();
			
			if (!subscribedFiles.Any())
			{
				return;
			}

			var newNotes = new List<Tuple<Guid, Entity>>();
			var replication = (HttpContext.Current.Application[BlobReplicationKey] as Dictionary<string, Tuple<Guid, Guid>[]>
				?? (Dictionary<string, Tuple<Guid, Guid>[]>)(HttpContext.Current.Application[BlobReplicationKey] = new Dictionary<string, Tuple<Guid, Guid>[]>()));

			foreach (var file in subscribedFiles)
			{
				var notes = file.GetRelatedEntities(Context, "adx_webfile_Annotations");

				if (notes.Any())
				{
					// This file already has attachments, leave it alone.
					continue;
				}

				var replicatedNote = new Entity("annotation");

				replicatedNote.SetAttributeValue("documentbody", Source.GetAttributeValue("documentbody"));
				replicatedNote.SetAttributeValue("filename", Source.GetAttributeValue("filename"));
				replicatedNote.SetAttributeValue("mimetype", Source.GetAttributeValue("mimetype"));
				replicatedNote.SetAttributeValue("subject", Source.GetAttributeValue("subject"));
				replicatedNote.SetAttributeValue("notetext", Source.GetAttributeValue("notetext"));
				replicatedNote.SetAttributeValue("isdocument", Source.GetAttributeValue("isdocument") ?? false);
				replicatedNote.SetAttributeValue("objectid", new EntityReference("adx_webfile", file.Id));
				replicatedNote.SetAttributeValue("objecttypecode", "adx_webfile");

				newNotes.Add(new Tuple<Guid, Entity>(file.GetAttributeValue<EntityReference>("adx_websiteid").Id, replicatedNote));
				Context.AddObject(replicatedNote);
			}

			Context.SaveChanges();
			
			replication[Source.Id.ToString("N")] = newNotes.Select(n => new Tuple<Guid, Guid>(n.Item1, n.Item2.Id)).ToArray();
		}
	}
}
