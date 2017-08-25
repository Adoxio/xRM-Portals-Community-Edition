/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Represents possible user-submitted options for a given <see cref="IPurchasableItem"/>.
	/// </summary>
	public interface IPurchasableItemOptions
	{
		string Instructions { get; }

		bool? IsSelected { get; }

		decimal? Quantity { get; }

		EntityReference QuoteProduct { get; }
	}
}
