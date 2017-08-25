/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Commerce
{
	public interface IPurchaseDataAdapter
	{
		void CompletePurchase(bool fulfillOrder = false, bool createInvoice = false);

		IPurchasable Select();

		IPurchasable Select(IEnumerable<IPurchasableItemOptions> options);

		void UpdateShipToAddress(IPurchaseAddress address);
	}
}
