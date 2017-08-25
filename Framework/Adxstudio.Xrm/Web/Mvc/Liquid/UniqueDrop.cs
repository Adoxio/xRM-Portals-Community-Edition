/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueDrop.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   The facet configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	using System;

	using DotLiquid;

	/// <summary>
	/// Servicing class for unique id in Liquid templates
	/// </summary>
	/// <seealso cref="DotLiquid.Drop" />
	public class UniqueDrop : Drop
	{
		/// <summary>
		/// Gets the new GUID-identifier.
		/// </summary>
		/// <value>
		/// The new GUID-identifier.
		/// </value>
		public object NewGuid
		{
			get { return Guid.NewGuid(); }
		}
	}
}
