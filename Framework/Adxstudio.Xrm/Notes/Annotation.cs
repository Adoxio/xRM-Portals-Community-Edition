/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Notes
{
	public class Annotation : IAnnotation
	{
		public Annotation()
		{
			
		}

		public Annotation(Entity entity, EntityReference regarding, Func<IAnnotationFile> getAnnotationFile)
		{
			AnnotationId = entity.GetAttributeValue<Guid>("annotationid");
			Subject = entity.GetAttributeValue<string>("subject");
			NoteText = entity.GetAttributeValue<string>("notetext");
			Regarding = regarding;
			FileAttachment = getAnnotationFile();
			CreatedOn = entity.GetAttributeValue<DateTime?>("createdon").GetValueOrDefault();

			Entity = entity;
		}

		public Entity Entity { get; set; }
		public Guid AnnotationId { get; set; }
		public string Subject { get; set; }
		public string NoteText { get; set; }
		public EntityReference Regarding { get; set; }
		public IAnnotationFile FileAttachment { get; set; }
		public DateTime CreatedOn { get; set; }
		public EntityReference Owner { get; set; }
	}
}
