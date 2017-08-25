/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm
{
	internal static class EntityReferenceExtensions
	{
		internal static void AssertLogicalName(this EntityReference entityReference, params string[] expectedLogicalName)
		{
			// accept null values
			if (entityReference == null) return;

			if (!HasLogicalName(entityReference, expectedLogicalName))
			{
				throw new ArgumentException(
					ResourceManager.GetString("Expected_Entity_Object_Argument_Exception").FormatWith(
						string.Join(" or ", expectedLogicalName),
						entityReference.LogicalName));
			}
		}

		private static bool HasLogicalName(this EntityReference entityReference, params string[] expectedLogicalName)
		{
			return entityReference != null && expectedLogicalName.Contains(entityReference.LogicalName);
		}
	}
}
