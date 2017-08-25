/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Data
{
	public interface IPaginated
	{
		bool HasPreviousPage { get; }

		bool HasNextPage { get; }

		int PageNumber { get; }

		int TotalCount { get; }

		int TotalPages { get; }
	}
}
