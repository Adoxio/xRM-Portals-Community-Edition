/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class EntitySiteMapDisplayOrderComparer : IComparer<Entity>
	{
		public int Compare(Entity x, Entity y)
		{
			if (x == null && y == null)
			{
				return 0;
			}

			if (x == null)
			{
				return 1;
			}

			if (y == null)
			{
				return -1;
			}

			int? xDisplayOrder;
				
			// Try get a display order value for x.
			if (x.Attributes.Contains("adx_displayorder"))
			{
				try
				{
					xDisplayOrder = x.GetAttributeValue<int?>("adx_displayorder");
				}
				catch
				{
					xDisplayOrder = null;
				}
			}
			else
			{
				xDisplayOrder = null;
			}

			int? yDisplayOrder;
				
			// Try get a display order value for y.
			if (y.Attributes.Contains("adx_displayorder"))
			{
				try
				{
					yDisplayOrder = y.GetAttributeValue<int?>("adx_displayorder");
				}
				catch
				{
					yDisplayOrder = null;
				}
			}
			else
			{
				yDisplayOrder = null;
			}

			// If neither has a display order, they are ordered equally.
			if (!(xDisplayOrder.HasValue || yDisplayOrder.HasValue))
			{
				return 0;
			}

			// If x has no display order, and y does, order x after y.
			if (!xDisplayOrder.HasValue)
			{
				return 1;
			}

			// If x has a display order, and y does not, order y after x.
			if (!yDisplayOrder.HasValue)
			{
				return -1;
			}

			// If both have display orders, order by the comparison of that value.
			return xDisplayOrder.Value.CompareTo(yDisplayOrder.Value);
		}
	}
}
