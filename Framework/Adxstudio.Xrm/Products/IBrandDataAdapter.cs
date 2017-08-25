/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Products
{
	public interface IBrandDataAdapter
	{
		IBrand SelectBrand(Guid id);

		IEnumerable<IBrand> SelectBrands();
	}
}
