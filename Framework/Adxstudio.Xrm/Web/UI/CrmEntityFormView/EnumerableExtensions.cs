/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	internal static class EnumerableExtensions
	{
		/// <summary>
		/// Randomize the order
		/// </summary>
		/// <param name="target"></param>
		/// <typeparam name="T"></typeparam>
		public static IEnumerable<T> Randomize<T>(this IEnumerable<T> target)
		{
			var r = new Random();

			return target.OrderBy(x => (r.Next()));
		}
	}
}
