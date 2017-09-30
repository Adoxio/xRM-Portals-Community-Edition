/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Search.Index
{
	internal class UserQuery : SavedQuery
	{
		public UserQuery(Entity savedQuery) : base(savedQuery) { }
	}
}
