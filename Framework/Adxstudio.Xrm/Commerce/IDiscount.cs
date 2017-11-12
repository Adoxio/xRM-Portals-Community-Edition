/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Represents a discount
	/// </summary>
	public interface IDiscount
	{
		decimal Amount { get; }

		string Code { get; }

		Guid Id { get; }

		string Name { get; }

		DiscountScope? Scope { get; }

		DiscountType? Type { get; }
	}

	public enum DiscountScope
	{
		Product = 756150000,
		Order = 756150001
	}

	public enum DiscountType
	{
		Percentage = 756150000,
		Amount = 756150001
	}
}
