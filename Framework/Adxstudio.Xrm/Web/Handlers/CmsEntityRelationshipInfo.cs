/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	public class CmsEntityRelationshipInfo
	{
		public CmsEntityRelationshipInfo(Relationship relationship, bool isCollection, string referencedEntity, string referencingEntity)
		{
			if (relationship == null) throw new ArgumentNullException("relationship");
			if (referencedEntity == null) throw new ArgumentNullException("referencedEntity");
			if (referencingEntity == null) throw new ArgumentNullException("referencingEntity");

			Relationship = relationship;
			IsCollection = isCollection;
			ReferencedEntity = referencedEntity;
			ReferencingEntity = referencingEntity;
		}

		public bool IsCollection { get; private set; }

		public string ReferencedEntity { get; private set; }

		public string ReferencingEntity { get; private set; }

		public Relationship Relationship { get; private set; }
	}
}
