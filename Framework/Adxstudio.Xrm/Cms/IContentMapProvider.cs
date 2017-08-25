/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Cms
{
	using System.Runtime.CompilerServices;

	public interface IContentMapProvider
	{
		T Using<T>(Func<ContentMap, T> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);
		void Using(Action<ContentMap> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);
		void Clear();
		EntityNode Refresh(ContentMap map, EntityReference reference);
        void Refresh(ContentMap map, List<EntityReference> reference);
        void Associate(ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities);
		void Disassociate(ContentMap map, EntityReference target, Relationship relationship, EntityReferenceCollection relatedEntities);
	}
}
