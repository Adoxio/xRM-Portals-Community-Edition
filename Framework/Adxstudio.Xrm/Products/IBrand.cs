/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Products
{
	public interface IBrand
	{
		string Description { get; }

		Guid Id { get; }

		string Name { get; }
	}
}
