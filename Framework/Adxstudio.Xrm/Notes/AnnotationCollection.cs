/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Notes
{
	class AnnotationCollection : IAnnotationCollection
	{
		private IEnumerable<IAnnotation> Enumerable { get; set; }

		private AnnotationCollection(bool permissionDenied)
		{
			Enumerable = System.Linq.Enumerable.Empty<IAnnotation>();
			TotalCount = 0;
			PermissionDenied = permissionDenied;
		}

		public AnnotationCollection(IEnumerable<IAnnotation> entities, int totalCount)
		{
			Enumerable = entities;
			TotalCount = totalCount;
			PermissionDenied = false;
		}

		public IEnumerator<IAnnotation> GetEnumerator()
		{
			return Enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int TotalCount { get; private set; }
		public bool PermissionDenied { get; private set; }

		public static IAnnotationCollection Empty(bool permissionDenied)
		{
			return new AnnotationCollection(permissionDenied);
		}
	}
}
