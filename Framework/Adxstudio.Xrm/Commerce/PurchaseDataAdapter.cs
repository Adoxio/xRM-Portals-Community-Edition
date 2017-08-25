/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Client.Diagnostics;

namespace Adxstudio.Xrm.Commerce
{
	public abstract class PurchaseDataAdapter : IPurchaseDataAdapter
	{
		protected PurchaseDataAdapter(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		public abstract void CompletePurchase(bool fulfillOrder = false, bool createInvoice = false);

		public IPurchasable Select()
		{
			return Select(Enumerable.Empty<IPurchasableItemOptions>());
		}

		public abstract IPurchasable Select(IEnumerable<IPurchasableItemOptions> options);

		public abstract void UpdateShipToAddress(IPurchaseAddress address);

		protected virtual void TraceMethodError(string format, params object[] args)
		{
            ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("{0}, {1}", format, args));
        }

		protected virtual void TraceMethodInfo(string format, params object[] args)
		{
            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("{0}, {1}", format, args));
		}
	}
}
