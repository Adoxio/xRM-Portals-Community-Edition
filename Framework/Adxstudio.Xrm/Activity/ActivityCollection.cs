/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Activity
{
	class ActivityCollection : IActivityCollection
	{
		private IEnumerable<IActivity> Enumerable { get; set; }

		private ActivityCollection(bool permissionDenied)
		{
			Enumerable = System.Linq.Enumerable.Empty<IActivity>();
			TotalCount = 0;
			PermissionDenied = permissionDenied;
		}

		public ActivityCollection(IEnumerable<IActivity> entities, int totalCount)
		{
			Enumerable = entities;
			TotalCount = totalCount;
			PermissionDenied = false;
		}

		public IEnumerator<IActivity> GetEnumerator()
		{
			return Enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int TotalCount { get; private set; }
		public bool PermissionDenied { get; private set; }

		public static IActivityCollection Empty(bool permissionDenied)
		{
			return new ActivityCollection(permissionDenied);
		}
	}
}
