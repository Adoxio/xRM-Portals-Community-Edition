/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// Describes a content map relationship.
	/// </summary>
	public class RelationshipDefinition
	{
		public static RelationshipDefinition Create<T1, T2>(
			string solution,
			string foreignEntityLogicalName,
			string foreignIdAttributeName,
			Version version,
			Func<T1, T2> toOne,
			Func<T2, IEnumerable<T1>> toMany,
			Action<T2, T1> associate,
			Action<T2, T1, T2> disassociate = null)
			where T1 : EntityNode
			where T2 : EntityNode
		{
			return new RelationshipDefinition
			{
				ForeignEntityLogicalname = foreignEntityLogicalName,
				ForeignIdAttributeName = foreignIdAttributeName,
				Solution = solution,
				ToOne = toOne != null ? node => toOne(node as T1) : (Func<EntityNode, EntityNode>)null,
				ToMany = toMany != null ? node => toMany(node as T2) : (Func<EntityNode, IEnumerable<EntityNode>>)null,
				Associate = associate != null ? (source, target) => associate(source as T2, target as T1) : (Action<EntityNode, EntityNode>)null,
				Disassociate = disassociate != null ? (source, target, id) => disassociate(source as T2, target as T1, id as T2) : (Action<EntityNode, EntityNode, EntityNode>)null,
				IntroducedVersion = version,
			};
		}

		private RelationshipDefinition() { }

		public string ForeignEntityLogicalname { get; private set; }
		public string ForeignIdAttributeName { get; private set; }
		public string Solution { get; set; }
		public Func<EntityNode, EntityNode> ToOne { get; private set; }
		public Func<EntityNode, IEnumerable<EntityNode>> ToMany { get; private set; }
		public Action<EntityNode, EntityNode> Associate { get; private set; }
		public Action<EntityNode, EntityNode, EntityNode> Disassociate { get; private set; }

		public Version IntroducedVersion { get; private set; }
	}

	/// <summary>
	/// Describes a content map many-to-many relationship.
	/// </summary>
	public class ManyRelationshipDefinition
	{
		public string Solution { get; set; }
		public string SchemaName { get; private set; }
		public string IntersectEntityname { get; private set; }

		public Version IntroducedVersion { get; private set; }

		public ManyRelationshipDefinition(string solution, string schemaName, string intersectEntityName, Version version)
		{
			Solution = solution;
			SchemaName = schemaName;
			IntersectEntityname = intersectEntityName;
			IntroducedVersion = version;
		}
	}
}
